using System;

namespace Turnit.GenericStore.Api.Features.Sales.Products;

public class ProductModel
{
    public Guid Id { get; init; }
    public string Name { get; set; }
    public ProductCategoryModel[] Categories { get; set; }
    public ProductAvailabilityModel[] AvailableInStores { get; set; }
}