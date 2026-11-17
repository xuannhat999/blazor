using blazor_app.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace blazor_app.Data;

public static class DataSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task SeedAsync(IServiceProvider services)
    {
        var factory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var db = await factory.CreateDbContextAsync();

        await db.Database.EnsureCreatedAsync();

        if (await db.Products.AnyAsync())
        {
            return;
        }

        var path = Path.Combine(AppContext.BaseDirectory, "Data", "seed.json");
        if (!File.Exists(path))
        {
            var env = services.GetService<IHostEnvironment>();
            if (env is not null)
            {
                path = Path.Combine(env.ContentRootPath, "Data", "seed.json");
            }
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Seed file not found.", path);
        }

        var json = await File.ReadAllTextAsync(path);
        var data = JsonSerializer.Deserialize<SeedData>(json, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize seed.json.");

        await using var transaction = await db.Database.BeginTransactionAsync();

        var categories = data.Categories.Select(x => new Category { Code = x.Code, Label = x.Label }).ToList();
        var brands = data.Brands.Select(x => new Brand { Code = x.Code, Label = x.Label }).ToList();
        var teams = data.Teams.Select(x => new Team { Code = x.Code, Label = x.Label }).ToList();
        var leagues = data.Leagues.Select(x => new League { Code = x.Code, Label = x.Label }).ToList();
        var kitTypes = data.KitTypes.Select(x => new KitType { Code = x.Code, Label = x.Label }).ToList();
        var seasons = data.Seasons.Select(x => new Season { Name = x.Name }).ToList();

        db.Categories.AddRange(categories);
        db.Brands.AddRange(brands);
        db.Teams.AddRange(teams);
        db.Leagues.AddRange(leagues);
        db.KitTypes.AddRange(kitTypes);
        db.Seasons.AddRange(seasons);
        await db.SaveChangesAsync();

        var categoryIds = categories.ToDictionary(x => x.Code, x => x.Id);
        var brandIds = brands.ToDictionary(x => x.Code, x => x.Id);
        var teamIds = teams.ToDictionary(x => x.Code, x => x.Id);
        var leagueIds = leagues.ToDictionary(x => x.Code, x => x.Id);
        var kitTypeIds = kitTypes.ToDictionary(x => x.Code, x => x.Id);
        var seasonIds = seasons.ToDictionary(x => x.Name, x => x.Id);

        foreach (var dto in data.Products)
        {
            var product = new Product
            {
                Id = dto.Id,
                Name = dto.Name,
                Price = dto.Price,
                OldPrice = dto.OldPrice,
                SourceUrl = dto.SourceUrl,
                BrandId = brandIds[dto.Brand],
                CategoryId = categoryIds[dto.Category],
                TeamId = teamIds[dto.Team],
                LeagueId = leagueIds[dto.League],
                KitTypeId = kitTypeIds[dto.KitType],
                SeasonId = seasonIds[dto.Season]
            };

            for (var i = 0; i < dto.Images.Count; i++)
            {
                product.Images.Add(new ProductImage
                {
                    Url = dto.Images[i],
                    SortOrder = i
                });
            }

            db.Products.Add(product);
        }

        db.Users.AddRange(data.Users.Select(x => new User
        {
            Id = x.Id,
            Name = x.Name,
            Address = x.Address,
            PayMethod = x.PayMethod,
            Email = x.Email,
            Password = x.Password,
            Role = x.Role,
            IsLocked = x.IsLocked
        }));

        await db.SaveChangesAsync();

        foreach (var dto in data.Orders)
        {
            var order = new Order
            {
                Id = dto.Id,
                UserId = dto.UserId,
                Address = dto.Address,
                PayMethod = dto.PayMethod,
                Status = dto.Status,
                CreatedAt = dto.CreatedAt,
                Total = dto.Total
            };

            foreach (var item in dto.Items)
            {
                order.Items.Add(new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Size = item.Size,
                    Price = item.Price
                });
            }

            db.Orders.Add(order);
        }

        db.CartItems.AddRange(data.CartItems.Select(x => new CartItem
        {
            UserId = x.UserId,
            ProductId = x.ProductId,
            Size = x.Size,
            Quantity = x.Quantity
        }));

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private sealed class SeedData
    {
        public List<LookupDto> Categories { get; set; } = new();
        public List<LookupDto> Brands { get; set; } = new();
        public List<LookupDto> Teams { get; set; } = new();
        public List<LookupDto> Leagues { get; set; } = new();
        public List<LookupDto> KitTypes { get; set; } = new();
        public List<SeasonDto> Seasons { get; set; } = new();
        public List<ProductDto> Products { get; set; } = new();
        public List<UserDto> Users { get; set; } = new();
        public List<OrderDto> Orders { get; set; } = new();
        public List<CartItemDto> CartItems { get; set; } = new();
    }

    private sealed class LookupDto
    {
        public string Code { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    private sealed class SeasonDto
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public string SourceUrl { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Team { get; set; } = string.Empty;
        public string League { get; set; } = string.Empty;
        public string KitType { get; set; } = string.Empty;
        public string Season { get; set; } = string.Empty;
        public List<string> Images { get; set; } = new();
    }

    private sealed class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public PayMethod PayMethod { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsLocked { get; set; }
    }

    private sealed class OrderDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Address { get; set; } = string.Empty;
        public string PayMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public decimal Total { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    private sealed class OrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public Size Size { get; set; }
        public decimal Price { get; set; }
    }

    private sealed class CartItemDto
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public Size Size { get; set; }
        public int Quantity { get; set; }
    }
}
