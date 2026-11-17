using blazor_app.Interfaces;
using blazor_app.Models;
using Microsoft.EntityFrameworkCore;

namespace blazor_app.Services;

public class OrderService : IOrderService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public OrderService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<Order?> CreateFromCartAsync(int userId, IReadOnlyList<int> cartItemIds, string address, PayMethod payMethod, CancellationToken ct = default)
    {
        var normalizedAddress = address.Trim();

        if (normalizedAddress.Length == 0 || cartItemIds.Count == 0)
        {
            return null;
        }

        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var ids = cartItemIds.Distinct().ToList();

        var cartItems = await db.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId && ids.Contains(c.Id))
            .ToListAsync(ct);

        if (cartItems.Count == 0)
        {
            return null;
        }

        var order = new Order
        {
            UserId = userId,
            Address = normalizedAddress,
            PayMethod = payMethod == PayMethod.Online ? "online" : "cash",
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
            Total = cartItems.Sum(c => c.Product.Price * c.Quantity)
        };

        foreach (var cartItem in cartItems)
        {
            order.Items.Add(new OrderDetail
            {
                ProductId = cartItem.ProductId,
                Quantity = cartItem.Quantity,
                Size = cartItem.Size,
                Price = cartItem.Product.Price
            });
        }

        db.Orders.Add(order);
        db.CartItems.RemoveRange(cartItems);

        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return order;
    }

    public async Task<IReadOnlyList<Order>> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        return await db.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ThenByDescending(o => o.Id)
            .ToListAsync(ct);
    }

    public async Task<Order?> GetByIdAsync(int orderId, int userId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        return await db.Orders
            .AsNoTracking()
            .Include(o => o.Items.OrderBy(i => i.Id))
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Images.OrderBy(img => img.SortOrder))
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, ct);
    }
}
