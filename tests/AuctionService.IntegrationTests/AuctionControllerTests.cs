using System;
using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests;

public class AuctionControllerTests : IClassFixture<CustomWebappFactory>, IAsyncLifetime
{
    private readonly CustomWebappFactory _factory;
    private readonly HttpClient _httpClient;
    private const string GT_ID = "afbee524-5972-4075-8800-7d1f9d7b0a0c";

    public AuctionControllerTests(CustomWebappFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllAuctions_ShouldReturn3Auctions()
    {
        // Arrange ?
    
        // Act
        var response = await _httpClient.GetFromJsonAsync<List<AuctionDTO>>("api/auctions");
    
        // Assert
        Assert.Equal(3, response.Count);
    }

    [Fact]
    public async Task GetAuctionByID_WithValidGUID_ShouldReturnAuction()
    {
        // Arrange ?
    
        // Act
        var response = await _httpClient.GetFromJsonAsync<AuctionDTO>($"api/auctions/{GT_ID}");
    
        // Assert
        Assert.Equal("GT", response.Model);
    }

    [Fact]
    public async Task GetAuctionByID_WithInvalidID_ShouldReturnNotFound()
    {
        // Arrange ?
    
        // Act
        var response = await _httpClient.GetAsync($"api/auctions/{Guid.NewGuid()}");
    
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAuctionByID_WithInvalidGUID_ShouldReturnBadRequest()
    {
        // Arrange ?
    
        // Act
        var response = await _httpClient.GetAsync($"api/auctions/notaguid");
    
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDBContext>();
        DBHelper.ReinitDBForTest(db); 
        return Task.CompletedTask;
    }
}
