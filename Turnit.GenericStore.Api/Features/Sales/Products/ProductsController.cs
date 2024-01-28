using System;
using System.Collections.Generic;
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
        _session = session;
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
        var product = await _session.GetAsync<Product>(productId);
        if (product == null)
            throw new Exception("Product not found.");

        var category = await _session.GetAsync<Category>(categoryId);
        if (category == null)
            throw new Exception("Category not found.");

        var productCategoryLink = await _session
            .Query<ProductCategoryLink>()
            .SingleOrDefaultAsync(x => x.Product.Id == productId && x.Category.Id == categoryId);
        
        if (productCategoryLink != null)
            throw new Exception("Product already in category.");

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
        var product = await _session.GetAsync<Product>(productId);
        if (product == null)
            throw new Exception("Product not found.");

        var category = await _session.GetAsync<Category>(categoryId);
        if (category == null)
            throw new Exception("Category not found.");

        var productCategoryLink = await _session
            .Query<ProductCategoryLink>()
            .SingleOrDefaultAsync(x => x.Product.Id == productId && x.Category.Id == categoryId);
        
        if (productCategoryLink == null)
            throw new Exception("Product not in category.");

        using (var transaction = _session.BeginTransaction())
        {
            await _session.DeleteAsync(productCategoryLink);
            await transaction.CommitAsync();
        }

        return Ok();
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
}