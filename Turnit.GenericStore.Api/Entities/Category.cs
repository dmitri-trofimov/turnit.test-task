using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace Turnit.GenericStore.Api.Entities;

public class Category
{
    public Category()
    {
        Products = new List<Product>();
    }

    public virtual Guid Id { get; set; }
    public virtual string Name { get; set; }

    public virtual IList<Product> Products { get; set; }
}

public class CategoryMap : ClassMap<Category>
{
    public CategoryMap()
    {
        Schema("public");
        Table("category");

        Id(x => x.Id, "id");
        Map(x => x.Name, "name");

        HasManyToMany(x => x.Products)
            .Table("product_category");
    }
}