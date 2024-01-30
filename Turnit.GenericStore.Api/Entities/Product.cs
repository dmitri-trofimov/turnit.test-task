using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace Turnit.GenericStore.Api.Entities;

public class Product
{
    public Product()
    {
        ProductStoreLinks = new HashSet<ProductStoreLink>();
        ProductCategoryLinks = new HashSet<ProductCategoryLink>();
    }

    public virtual Guid Id { get; set; }
    public virtual string Name { get; set; }
    public virtual string Description { get; set; }

    public virtual ISet<ProductCategoryLink> ProductCategoryLinks { get; set; }
    public virtual ISet<ProductStoreLink> ProductStoreLinks { get; set; }
}

public class ProductMap : ClassMap<Product>
{
    public ProductMap()
    {
        Schema("public");
        Table("product");

        Id(x => x.Id, "id");
        Map(x => x.Name, "name");
        Map(x => x.Description, "description");

        HasMany(x => x.ProductCategoryLinks)
            .AsSet();

        HasMany(x => x.ProductStoreLinks)
            .AsSet();
    }
}