using Basket.Domain.Entities;

namespace Basket.Infrastructure.Repositories;

public interface IBasketRepository
{
    Task<ShoppingBasket?> GetAsync(Guid userId, CancellationToken ct = default);
    Task<ShoppingBasket> UpdateAsync(ShoppingBasket basket, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, CancellationToken ct = default);
}
