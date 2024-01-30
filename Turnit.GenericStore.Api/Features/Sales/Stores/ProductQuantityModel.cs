using System;

namespace Turnit.GenericStore.Api.Features.Sales.Stores;

public class ProductQuantityModel
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}