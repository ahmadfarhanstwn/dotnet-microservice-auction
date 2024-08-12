using System;
using AuctionService.DTOs;
using AuctionService.Entities;

namespace AuctionService.Data;

public interface IAuctionRepository
{
    Task<List<AuctionDTO>> GetAllAuctionsAsync(string date);
    Task<AuctionDTO> GetAuctionDTOByIdAsync(Guid id);
    Task<Auction> GetAuctionEntityByIdAsync(Guid id);
    void AddAuction(Auction auction);
    Task<bool> SaveChangesAsync();
    void RemoveAuction(Auction auction);
}
