using MassTransit;
using Contracts;
using AutoMapper;
using MongoDB.Entities;

namespace SearchService;

public class AuctionCreatedCustomer : IConsumer<AuctionCreated>
{
    private readonly IMapper _mapper;

    public AuctionCreatedCustomer(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine("Consuming auction created: " + context.Message.Id);

        var item = _mapper.Map<Item>(context.Message);

        await item.SaveAsync();
    }
}
