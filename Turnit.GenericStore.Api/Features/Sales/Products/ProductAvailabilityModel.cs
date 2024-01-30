using System;

namespace Turnit.GenericStore.Api.Features.Sales.Products;

public class ProductAvailabilityModel
{
    public Guid StoreId { get; set; }
    public string StoreName { get; set; }
    public int AvailableCount { get; set; }
}