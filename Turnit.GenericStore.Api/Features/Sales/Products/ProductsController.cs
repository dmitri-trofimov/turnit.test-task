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

    private IQueryable<Product> GetProductsQuery(Guid? categoryId, bool includeAvailability,
        bool includeCategories)
    {
        var productsQuery = _session.Query<Product>();

        if (categoryId != null)
            productsQuery = productsQuery.Where(p => p.Categories.Any(c => c.Id == categoryId));

        if (includeCategories)
            productsQuery = productsQuery.Fetch(x => x.Categories);

        if (includeAvailability)
            productsQuery = productsQuery.Fetch(x => x.AvailableInStores);

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
                ? p.Categories.Select(cat => new ProductCategoryModel { Id = cat.Id, Name = cat.Name }).ToArray()
                : null,
            AvailableInStores = includeAvailability
                ? p.AvailableInStores.Select(avail => new ProductAvailabilityModel
                    {
                        StoreId = avail.Store.Id, StoreName = avail.Store.Name, AvailableCount = avail.AvailableCount
                    })
                    .ToArray()
                : null
        }).ToArray();

        return result;
    }
}