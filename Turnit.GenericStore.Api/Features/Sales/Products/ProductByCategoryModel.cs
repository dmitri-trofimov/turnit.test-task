using System.Text.Json.Serialization;

namespace Turnit.GenericStore.Api.Features.Sales.Products;

public class ProductByCategoryModel : ProductModel
{
    [JsonIgnore]
    public new ProductCategoryModel[] Categories { get; set; }
}