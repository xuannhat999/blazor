using blazor_app.Interfaces;
using blazor_app.Models;
using Microsoft.EntityFrameworkCore;

namespace blazor_app.Services;

public class CartService : ICartService
{
    private const int MaxQuantity = 99;

    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public CartService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IReadOnlyList<CartItem>> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        return await db.CartItems
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .Include(c => c.Product).ThenInclude(p => p.Images.OrderBy(i => i.SortOrder))
            .Include(c => c.Product).ThenInclude(p => p.Brand)
            .Include(c => c.Product).ThenInclude(p => p.Team)
            .OrderBy(c => c.Id)
            .ToListAsync(ct);
    }

    public async Task AddAsync(int userId, int productId, Size size, int quantity, CancellationToken ct = default)
    {
        var safeQuantity = Math.Clamp(quantity, 1, MaxQuantity);

        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var existing = await db.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId
                && c.ProductId == productId
                && c.Size == size, ct);

        if (existing is not null)
        {
            existing.Quantity = Math.Clamp(existing.Quantity + safeQuantity, 1, MaxQuantity);
        }
        else
        {
            db.CartItems.Add(new CartItem
            {
                UserId = userId,
                ProductId = productId,
                Size = size,
                Quantity = safeQuantity
            });
        }

        await db.SaveChangesAsync(ct);
    }
}
