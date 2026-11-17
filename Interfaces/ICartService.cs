using blazor_app.Models;

namespace blazor_app.Interfaces;

public interface ICartService
{
    Task AddAsync(int userId, int productId, Size size, int quantity, CancellationToken ct = default);

    Task<IReadOnlyList<CartItem>> GetByUserIdAsync(int userId, CancellationToken ct = default);
}
