using blazor_app.Interfaces;
using blazor_app.Models;
using Microsoft.EntityFrameworkCore;

namespace blazor_app.Services;

public class UserService : IUserService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public UserService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<User?> AuthenticateAsync(string email, string password, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim();

        if (normalizedEmail.Length == 0 || password.Length == 0)
        {
            return null;
        }

        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        return await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail && u.Password == password, ct);
    }

    public async Task<bool> RegisterAsync(User user, CancellationToken ct = default)
    {
        user.Email = user.Email.Trim();

        if (string.IsNullOrWhiteSpace(user.Name)
            || string.IsNullOrWhiteSpace(user.Email)
            || string.IsNullOrWhiteSpace(user.Password)
            || string.IsNullOrWhiteSpace(user.Address))
        {
            return false;
        }

        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var email = user.Email;
        var exists = await db.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == email, ct);

        if (exists)
        {
            return false;
        }

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        return true;
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        return await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<bool> UpdateProfileAsync(int userId, string name, string address, PayMethod payMethod, CancellationToken ct = default)
    {
        name = name?.Trim() ?? string.Empty;
        address = address?.Trim() ?? string.Empty;

        if (name.Length == 0 || address.Length == 0)
        {
            return false;
        }

        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null)
        {
            return false;
        }

        user.Name = name;
        user.Address = address;
        user.PayMethod = payMethod;

        await db.SaveChangesAsync(ct);

        return true;
    }
}
