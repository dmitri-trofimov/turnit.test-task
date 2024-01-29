using System;
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

    [HttpPost]
    [Route("{storeId:guid}/products/{productId:guid}/restock")]
    public async Task<IActionResult> RestockProductInStore(
        Guid storeId,
        Guid productId,
        [FromBody] ProductRestockModel model)
    {
        if (model.Quantity < 1)
            throw new Exception("Restock quantity must be greater than zero.");

        var store = _session.Get<Store>(storeId) ?? throw new Exception("Store not found.");
        var product = _session.Get<Product>(productId) ?? throw new Exception("Product not found.");

        var productStoreLink = await _session.Query<ProductStoreLink>()
            .SingleOrDefaultAsync(x => x.Store.Id == store.Id && x.Product.Id == product.Id);

        if (productStoreLink == null)
            throw new Exception("Product is not presented in store.");

        using (var transaction = _session.BeginTransaction())
        {
            productStoreLink.AvailableCount += model.Quantity;
            await _session.SaveOrUpdateAsync(productStoreLink);

            await transaction.CommitAsync();
        }

        return Ok();
    }
}