using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;

namespace SearchService;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Item>>> SearchItems(string searchTerm, int pageNumber = 1, int pageSize = 5)
    {
        var query = DB.PagedSearch<Item>();

        query.Sort(x => x.Ascending(a => a.Make));

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query.Match(Search.Full, searchTerm).SortByTextScore();
        }

        query.PageNumber(pageNumber);
        query.PageSize(pageSize);

        var result = await query.ExecuteAsync();

        return Ok(new
            {
                result = result.Results,
                pageCount = result.PageCount,
                totalCount = result.TotalCount
            }
        );
    }
}
