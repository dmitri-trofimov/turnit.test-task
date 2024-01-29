using System;

namespace Turnit.GenericStore.Api.Features.Sales.Products;

public class StoreQuantityModel
{
    public Guid StoreId { get; set; }
    public int Quantity { get; set; }
}