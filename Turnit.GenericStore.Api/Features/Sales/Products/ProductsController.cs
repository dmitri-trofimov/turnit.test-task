using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Linq;
using Turnit.GenericStore.Api.Entities;

namespace Turnit.GenericStore.Api.Features.Sales.Products;

[Route("products")]
public class ProductsController : ApiControllerBase
{
    private readonly ISession _session;

    public ProductsController(ISession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    [HttpGet]
    [Route("by-category/{categoryId:guid}")]
    public async Task<ProductByCategoryModel[]> ProductsByCategory(Guid categoryId, bool includeAvailability = false)
    {
        var productsQuery = GetProductsQuery(categoryId, includeAvailability, false);
        var products = await productsQuery.ToListAsync();

        var result = GetProductModelsFromProducts<ProductByCategoryModel>(products, includeAvailability, false);

        return result;
    }

    [HttpGet]
    [Route("")]
    public async Task<ProductModel[]> AllProducts(bool includeAvailability = false, bool includeCategories = false)
    {
        var productsQuery = GetProductsQuery(null, includeAvailability, includeCategories);
        var products = await productsQuery.ToListAsync();

        var result = GetProductModelsFromProducts<ProductModel>(products, includeAvailability, includeCategories);

        return result;
    }

    [HttpPut]
    [Route("{productId:guid}/category/{categoryId:guid}")]
    public async Task<IActionResult> AddProductToCategory(Guid productId, Guid categoryId)
    {
        var product = await _session.GetAsync<Product>(productId) ?? throw new Exception("Product not found.");
        var category = await _session.GetAsync<Category>(categoryId) ?? throw new Exception("Category not found.");

        var productCategoryLink = await _session
            .Query<ProductCategoryLink>()
            .SingleOrDefaultAsync(x => x.Product.Id == productId && x.Category.Id == categoryId);

        if (productCategoryLink != null)
            return Ok();

        using (var transaction = _session.BeginTransaction())
        {
            productCategoryLink = new ProductCategoryLink { Product = product, Category = category };
            await _session.SaveAsync(productCategoryLink);

            await transaction.CommitAsync();
        }

        return Ok();
    }

    [HttpDelete]
    [Route("{productId:guid}/category/{categoryId:guid}")]
    public async Task<IActionResult> RemoveProductFromCategory(Guid productId, Guid categoryId)
    {
        var product = await _session.GetAsync<Product>(productId) ?? throw new Exception("Product not found.");
        var category = await _session.GetAsync<Category>(categoryId) ?? throw new Exception("Category not found.");

        var productCategoryLink = await _session
            .Query<ProductCategoryLink>()
            .SingleOrDefaultAsync(x => x.Product.Id == product.Id && x.Category.Id == category.Id);

        if (productCategoryLink == null)
            return Ok();

        using (var transaction = _session.BeginTransaction())
        {
            await _session.DeleteAsync(productCategoryLink);
            await transaction.CommitAsync();
        }

        return Ok();
    }

    [HttpPost]
    [Route("{productId:guid}/book")]
    public async Task<IActionResult> BookProductInStores(
        Guid productId,
        [FromBody] StoreQuantityModel[] storeQuantities)
    {
        var product = await _session.GetAsync<Product>(productId) ?? throw new Exception("Product not found.");
        var results = new List<BookProductInStoreResult>();

        using (var transaction = _session.BeginTransaction())
        {
            foreach (var storeQuantity in storeQuantities)
            {
                var storeResult = await BookProductInStore(product.Id, storeQuantity.StoreId, storeQuantity.Quantity);
                results.Add(storeResult);
            }

            await transaction.CommitAsync();
        }

        return Ok(results);
    }

    private async Task<BookProductInStoreResult> BookProductInStore(Guid productId, Guid storeId, int quantity)
    {
        try
        {
            if (quantity < 1)
                throw new Exception("Quantity must be greater than zero.");

            var store = await _session.GetAsync<Store>(storeId) ?? throw new Exception("Store not found.");

            var productStoreLink = await _session
                .Query<ProductStoreLink>()
                .SingleOrDefaultAsync(x => x.Product.Id == productId && x.Store.Id == store.Id);

            if (productStoreLink == null)
                throw new Exception("Product is not presented in store.");

            if (productStoreLink.AvailableCount < quantity)
                throw new Exception("Insufficient product quantity in store.");

            productStoreLink.AvailableCount -= quantity;
            await _session.SaveOrUpdateAsync(productStoreLink);

            return new BookProductInStoreResult
            {
                StoreId = storeId,
                IsSuccess = true
            };
        }
        catch (Exception ex)
        {
            return new BookProductInStoreResult
            {
                StoreId = storeId,
                IsSuccess = false,
                Message = ex.Message
            };
        }
    }

    private IQueryable<Product> GetProductsQuery(Guid? categoryId, bool includeAvailability,
        bool includeCategories)
    {
        var productsQuery = _session.Query<Product>();

        if (categoryId != null)
            productsQuery = productsQuery.Where(p => p.ProductCategoryLinks.Any(c => c.Id == categoryId));

        if (includeCategories)
            productsQuery = productsQuery
                .FetchMany(x => x.ProductCategoryLinks)
                .ThenFetch(x => x.Product);

        if (includeAvailability)
            productsQuery = productsQuery
                .FetchMany(x => x.ProductStoreLinks)
                .ThenFetch(x => x.Store);

        return productsQuery;
    }

    private static TProductModel[] GetProductModelsFromProducts<TProductModel>(IEnumerable<Product> products,
        bool includeAvailability,
        bool includeCategories) where TProductModel : ProductModel, new()
    {
        var result = products.Select(p => new TProductModel
        {
            Id = p.Id,
            Name = p.Name,
            Categories = includeCategories
                ? p.ProductCategoryLinks
                    .Select(cat => new ProductCategoryModel { Id = cat.Category.Id, Name = cat.Category.Name })
                    .ToArray()
                : null,
            AvailableInStores = includeAvailability
                ? p.ProductStoreLinks.Select(avail => new ProductAvailabilityModel
                    {
                        StoreId = avail.Store.Id,
                        StoreName = avail.Store.Name,
                        AvailableCount = avail.AvailableCount
                    })
                    .ToArray()
                : null
        }).ToArray();

        return result;
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    private class BookProductInStoreResult
    {
        public Guid StoreId { get; init; }
        public bool IsSuccess { get; init; }
        public string Message { get; init; }
    }
}