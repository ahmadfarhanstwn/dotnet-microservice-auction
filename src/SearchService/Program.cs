using MongoDB.Driver;
using MongoDB.Entities;
using SearchService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

try {
    await DBInitializer.InitDB(app);
} catch(Exception e)
{
    Console.WriteLine(e);
}

app.Run();