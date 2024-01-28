﻿using System;
using FluentNHibernate.Mapping;

namespace Turnit.GenericStore.Api.Entities;

public class ProductCategoryLink
{
    public virtual Guid Id { get; set; }
    public virtual Product Product { get; set; }
    public virtual Category Category { get; set; }
}

public class ProductCategoryLinkMap : ClassMap<ProductCategoryLink>
{
    public ProductCategoryLinkMap()
    {
        Schema("public");
        Table("product_category");

        Id(x => x.Id, "id");

        References(x => x.Category, "category_id");
        References(x => x.Product, "product_id");
    }
}