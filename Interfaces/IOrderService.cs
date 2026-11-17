using blazor_app.Models;

namespace blazor_app.Interfaces;

public interface IOrderService
{
    Task<Order?> CreateFromCartAsync(int userId, IReadOnlyList<int> cartItemIds, string address, PayMethod payMethod, CancellationToken ct = default);

    Task<IReadOnlyList<Order>> GetByUserIdAsync(int userId, CancellationToken ct = default);

    Task<Order?> GetByIdAsync(int orderId, int userId, CancellationToken ct = default);
}
