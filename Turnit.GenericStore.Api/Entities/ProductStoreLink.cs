using System;
using FluentNHibernate.Mapping;

namespace Turnit.GenericStore.Api.Entities;

public class ProductStoreLink
{
    public virtual Guid Id { get; set; }
    public virtual Product Product { get; set; }
    public virtual Store Store { get; set; }
    public virtual int AvailableCount { get; set; }
}

public class ProductStoreLinkMap : ClassMap<ProductStoreLink>
{
    public ProductStoreLinkMap()
    {
        Schema("public");
        Table("product_availability");

        Id(x => x.Id, "id");
        Map(x => x.AvailableCount, "availability");

        References(x => x.Store, "store_id");
        References(x => x.Product, "product_id");
    }
}