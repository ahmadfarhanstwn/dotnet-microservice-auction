using MongoDB.Entities;

namespace SearchService;

public class AuctionSrvClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AuctionSrvClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<List<Item>> GetItemsForSearchDB()
    {
        var lastUpdated = await DB.Find<Item, string>()
                            .Sort(x => x.Descending(x => x.UpdatedAt))
                            .Project(x => x.UpdatedAt.ToString())
                            .ExecuteFirstAsync();

        return await _httpClient.GetFromJsonAsync<List<Item>>(_configuration["AuctionServiceURL"] + "/api/auctions?date=" + lastUpdated);
    }
}
