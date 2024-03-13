using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using WebHost.Customization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetValue<string>("CacheSettings:ConnectionString");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/carts/{userId}", async (string userId, IDistributedCache redis) => 
{
    var data = await redis.GetStringAsync(userId);

    if (string.IsNullOrEmpty(data)) return null;

    var cart = JsonSerializer.Deserialize<Cart>(data, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = false
    });

    return cart;
});

app.MapPost("/carts", async (Cart cart, IDistributedCache redis) =>
{
    await redis.SetStringAsync(cart.UserId, JsonSerializer.Serialize(cart));
    return true;
});

app.Run();

record Cart(string UserId, List<Product> Product);
record Product(string Name, int Amount, decimal UnitPrice);
