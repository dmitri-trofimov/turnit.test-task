using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Linq;
using Turnit.GenericStore.Api.Entities;

namespace Turnit.GenericStore.Api.Features.Sales.Stores;

[Route("stores")]
public class StoresController : ApiControllerBase
{
    private readonly ISession _session;

    public StoresController(ISession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    [HttpGet]
    [Route("")]
    public async Task<IActionResult> GetAllStores()
    {
        var stores = await _session
            .Query<Store>()
            .Select(x => new StoreModel { Id = x.Id, Name = x.Name })
            .ToListAsync();

        return Ok(stores);
    }

    [HttpPost]
    [Route("{storeId:guid}/restock")]
    public async Task<IActionResult> RestockProductsInStore(
        Guid storeId,
        [FromBody] ProductQuantityModel[] productQuantities)
    {
        var store = await _session.GetAsync<Store>(storeId) ?? throw new Exception("Store not found.");
        var results = new List<RestockProductInStoreResult>();

        using (var transaction = _session.BeginTransaction())
        {
            foreach (var productQuantity in productQuantities)
            {
                var storeResult =
                    await RestockProductInStore(productQuantity.ProductId, store.Id, productQuantity.Quantity);

                results.Add(storeResult);
            }

            await transaction.CommitAsync();
        }

        return Ok(results);
    }

    private async Task<RestockProductInStoreResult> RestockProductInStore(Guid productId, Guid storeId, int quantity)
    {
        try
        {
            if (quantity < 1)
                throw new Exception("Quantity must be greater than zero.");

            var product = await _session.GetAsync<Product>(productId) ?? throw new Exception("Product not found.");

            var productStoreLink = await _session
                .Query<ProductStoreLink>()
                .SingleOrDefaultAsync(x => x.Product.Id == product.Id && x.Store.Id == storeId);

            if (productStoreLink == null)
                throw new Exception("Product is not presented in store.");

            productStoreLink.AvailableCount += quantity;
            await _session.SaveOrUpdateAsync(productStoreLink);

            return new RestockProductInStoreResult
            {
                ProductId = productId,
                IsSuccess = true
            };
        }
        catch (Exception ex)
        {
            return new RestockProductInStoreResult
            {
                ProductId = productId,
                IsSuccess = false,
                Message = ex.Message
            };
        }
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    private class RestockProductInStoreResult
    {
        public Guid ProductId { get; init; }
        public bool IsSuccess { get; init; }
        public string Message { get; init; }
    }
}