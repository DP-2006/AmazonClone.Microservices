using System.Text.Json;
using Basket.Domain.Entities;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Basket.Infrastructure.Repositories;

public sealed class RedisBasketRepository : IBasketRepository
{
    private readonly IDatabase _db;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public RedisBasketRepository(IConnectionMultiplexer redis) => _db = redis.GetDatabase();

    private static string Key(Guid userId) => $"basket:{userId}";

    public async Task<ShoppingBasket?> GetAsync(Guid userId, CancellationToken ct = default)
    {
        var data = await _db.StringGetAsync(Key(userId));
        return data.IsNullOrEmpty ? null : JsonSerializer.Deserialize<ShoppingBasket>(data!, JsonOpts);
    }

    public async Task<ShoppingBasket> UpdateAsync(ShoppingBasket basket, CancellationToken ct = default)
    {
        await _db.StringSetAsync(Key(basket.UserId), JsonSerializer.Serialize(basket, JsonOpts), TimeSpan.FromDays(7));
        return basket;
    }

    public async Task DeleteAsync(Guid userId, CancellationToken ct = default)
    {
        await _db.KeyDeleteAsync(Key(userId));
    }
}
