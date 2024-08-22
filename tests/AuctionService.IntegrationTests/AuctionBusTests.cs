using System;
using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Utils;
using Contracts;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests;

[Collection("Shared Collection")]
public class AuctionBusTests : IAsyncLifetime
{
    private readonly CustomWebappFactory _factory;
    private readonly HttpClient _httpClient;
    private ITestHarness _testHarness;

    public AuctionBusTests(CustomWebappFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
        _testHarness = factory.Services.GetTestHarness();
    }

    [Fact]
    public async Task CreateAuction_WithValidObject_ShouldPublishAuctionCreated()
    {
        // Arrange
        var auctionDto = GetValidAuctionForCreate();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

        // Act
        var response = await _httpClient.PostAsJsonAsync("api/auctions", auctionDto);
    
        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(await _testHarness.Published.Any<AuctionCreated>());
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDBContext>();
        DBHelper.ReinitDBForTest(db); 
        return Task.CompletedTask;
    }

    private CreateAuctionDTO GetValidAuctionForCreate()
    {
        return new CreateAuctionDTO
        {
            Make = "test",
            Model = "testModel",
            ImageUrl = "testimageurl",
            Color = "testcolor",
            Mileage = 10,
            Year = 10,
            ReservePrice = 10
        };
    }
}
