using blazor_app.Models;

namespace blazor_app.Interfaces;

public interface IUserService
{
    Task<User?> AuthenticateAsync(string email, string password, CancellationToken ct = default);
    Task<bool> RegisterAsync(User user, CancellationToken ct = default);
    Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
}
