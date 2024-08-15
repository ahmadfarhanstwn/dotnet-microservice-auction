using System;
using System.Xml.Linq;
using AuctionService.Controllers;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.RequestHelpers;
using AuctionService.UnitTests.Utils;
using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuctionService.UnitTests;

public class AuctionControllerTests
{
    private readonly Mock<IAuctionRepository> _auctionRepository;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly Fixture _fixture;
    private readonly AuctionsController _auctionsController;
    private readonly IMapper _mapper;

    public AuctionControllerTests()
    {
        _fixture = new Fixture();
        _auctionRepository = new Mock<IAuctionRepository>();
        _publishEndpoint = new Mock<IPublishEndpoint>();

        var mockMapper = new MapperConfiguration(mc => 
        {
            mc.AddMaps(typeof(MappingProfiles).Assembly);
        }).CreateMapper().ConfigurationProvider;
        _mapper = new Mapper(mockMapper);
        _auctionsController = new AuctionsController(_auctionRepository.Object, _mapper, _publishEndpoint.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext{User = Helpers.GetClaimsPrincipal()}
            }
        };
    }

    [Fact]
    public async Task GetAuctions_WithNoParams_Return10Auctions()
    {
        //arrange
        var auctions = _fixture.CreateMany<AuctionDTO>(10).ToList();
        _auctionRepository.Setup(repo => repo.GetAllAuctionsAsync(null)).ReturnsAsync(auctions);

        //act
        var result = await _auctionsController.GetAllAuctions(null);

        //assert
        Assert.Equal(10, result.Value.Count);
        Assert.IsType<ActionResult<List<AuctionDTO>>>(result);
    }

    [Fact]
    public async Task GetAuctionById_WithValidGuid_ReturnAuction()
    {
        //arrange
        var auction = _fixture.Create<AuctionDTO>();
        _auctionRepository.Setup(repo => repo.GetAuctionDTOByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);

        //act
        var result = await _auctionsController.GetAuctionById(auction.Id);

        //assert
        Assert.Equal(auction.Make, result.Value.Make);
        Assert.IsType<ActionResult<AuctionDTO>>(result);
    }

    [Fact]
    public async Task GetAuctionById_WithInValidGuid_ReturnAuction()
    {
        //arrange
        _auctionRepository.Setup(repo => repo.GetAuctionDTOByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value : null);

        //act
        var result = await _auctionsController.GetAuctionById(Guid.NewGuid());

        //assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateAuction_WithValidActionDto_ReturnCreatedAtAction()
    {
        //arrange
        var auctionDto = _fixture.Create<CreateAuctionDTO>();
        _auctionRepository.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
        _auctionRepository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        //act
        var result = await _auctionsController.CreateAuction(auctionDto);
        var createdResult = result.Result as CreatedAtActionResult;

        //assert
        Assert.NotNull(createdResult);
        Assert.Equal("GetAuctionById", createdResult.ActionName);
        Assert.IsType<AuctionDTO>(createdResult.Value);
    }

    [Fact]
    public async Task CreateAuction_FailedSave_Returns400BadRequest()
    {
        //arrange
        var auctionDto = _fixture.Create<CreateAuctionDTO>();
        _auctionRepository.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
        _auctionRepository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);

        //act
        var result = await _auctionsController.CreateAuction(auctionDto);

        //assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateAuction_WithUpdateAuctionDto_ReturnsOkResponse()
    {
        //arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
        auction.Seller = _auctionsController.User.Identity.Name;
        var updateAuctionDTO = _fixture.Create<UpdateAuctionDTO>();
        _auctionRepository.Setup(repo => repo.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        //act
        var result = await _auctionsController.UpdateAuction(Guid.NewGuid(), updateAuctionDTO);

        //assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidUser_Returns403Forbid()
    {
        //arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
        var updateAuctionDTO = _fixture.Create<UpdateAuctionDTO>();
        _auctionRepository.Setup(repo => repo.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        //act
        var result = await _auctionsController.UpdateAuction(Guid.NewGuid(), updateAuctionDTO);

        //assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidGuid_ReturnsNotFound()
    {
        var updateAuctionDTO = _fixture.Create<UpdateAuctionDTO>();
        _auctionRepository.Setup(repo => repo.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value: null);
        _auctionRepository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        //act
        var result = await _auctionsController.UpdateAuction(Guid.NewGuid(), updateAuctionDTO);

        //assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithValidUser_ReturnsOkResponse()
    {
        //arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
        auction.Seller = _auctionsController.User.Identity.Name;
        _auctionRepository.Setup(repo => repo.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepository.Setup(repo => repo.RemoveAuction(auction));
        _auctionRepository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        //act
        var result = await _auctionsController.DeleteAuction(Guid.NewGuid());

        //assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidGuid_Returns404Response()
    {
        //arrange
        _auctionRepository.Setup(repo => repo.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value : null);

        //act
        var result = await _auctionsController.DeleteAuction(Guid.NewGuid());

        //assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidUser_Returns403Response()
    {
        //arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
        _auctionRepository.Setup(repo => repo.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);

        //act
        var result = await _auctionsController.DeleteAuction(Guid.NewGuid());

        //assert
        Assert.IsType<ForbidResult>(result);
    }
}
