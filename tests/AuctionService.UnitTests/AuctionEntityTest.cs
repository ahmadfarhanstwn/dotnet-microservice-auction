using AuctionService.Entities;

namespace AuctionService.UnitTests;

public class AuctionEntityTest
{
    [Fact]
    public void HasReservePrice_ReservePriceGTZero_True()
    {
        //arrange
        var auction = new Auction{Id = Guid.NewGuid(), ReservePrice = 12};
        
        //act
        var result = auction.HasReservePrice();
        
        //assert
        Assert.True(result);
    }

    [Fact]
    public void HasReservePrice_ReservePriceGTZero_False()
    {
        //arrange
        var auction = new Auction{Id = Guid.NewGuid(), ReservePrice = 0};
        
        //act
        var result = auction.HasReservePrice();
        
        //assert
        Assert.False(result);
    }
}