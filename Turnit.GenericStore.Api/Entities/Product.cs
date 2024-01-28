using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace Turnit.GenericStore.Api.Entities;

public class Product
{
    public Product()
    {
        AvailableInStores = new HashSet<ProductInStore>();
        Categories = new HashSet<Category>();
    }

    public virtual Guid Id { get; set; }
    public virtual string Name { get; set; }
    public virtual string Description { get; set; }

    public virtual ISet<Category> Categories { get; set; }
    public virtual ISet<ProductInStore> AvailableInStores { get; set; }
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

        HasManyToMany(x => x.Categories)
            .Table("product_category")
            .AsSet();

        HasMany(x => x.AvailableInStores)
            .AsSet();
    }
}