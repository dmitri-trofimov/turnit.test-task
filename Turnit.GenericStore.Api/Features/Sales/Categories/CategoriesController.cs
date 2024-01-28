using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using NHibernate.Linq;
using Turnit.GenericStore.Api.Entities;

namespace Turnit.GenericStore.Api.Features.Sales.Categories;

[Route("categories")]
public class CategoriesController : ApiControllerBase
{
    private readonly ISession _session;

    public CategoriesController(ISession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    [HttpGet]
    [Route("")]
    public async Task<CategoryModel[]> AllCategories()
    {
        var categories = await _session.Query<Category>()
            .OrderBy(x => x.Name)
            .ToListAsync();

        var result = categories
            .Select(x => new CategoryModel
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToArray();

        return result;
    }
}