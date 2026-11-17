using blazor_app.Interfaces;
using blazor_app.Models;
using Microsoft.EntityFrameworkCore;

namespace blazor_app.Services;

public class ProductService : IProductService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public ProductService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(string categoryCode, int take = 12, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        return await db.Products
            .AsNoTracking()
            .Where(p => p.Category.Code == categoryCode)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .OrderBy(p => p.Id)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        return await db.Products
            .AsNoTracking()
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Team)
            .Include(p => p.League)
            .Include(p => p.KitType)
            .Include(p => p.Season)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyList<Product>> GetRelatedAsync(int productId, int take = 10, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var baseProduct = await db.Products
            .AsNoTracking()
            .Where(p => p.Id == productId)
            .Select(p => new { p.TeamId, p.LeagueId, p.SeasonId })
            .FirstOrDefaultAsync(ct);

        if (baseProduct is null)
        {
            return Array.Empty<Product>();
        }

        return await db.Products
            .AsNoTracking()
            .Where(p => p.Id != productId
                && (p.TeamId == baseProduct.TeamId
                    || p.LeagueId == baseProduct.LeagueId
                    || p.SeasonId == baseProduct.SeasonId))
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .OrderByDescending(p => p.TeamId == baseProduct.TeamId)
            .ThenByDescending(p => p.LeagueId == baseProduct.LeagueId)
            .ThenByDescending(p => p.SeasonId == baseProduct.SeasonId)
            .ThenBy(p => p.Id)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<ProductSearchResult> SearchAsync(ProductSearchCriteria criteria, CancellationToken ct = default)
    {
        criteria ??= new ProductSearchCriteria();

        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var query = db.Products
            .AsNoTracking()
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .AsQueryable();

        var keyword = criteria.Keyword?.Trim();
        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(p => p.Name.Contains(keyword));
        }

        if (criteria.Brands is { Count: > 0 })
        {
            query = query.Where(p => criteria.Brands.Contains(p.Brand.Code));
        }

        if (criteria.Categories is { Count: > 0 })
        {
            query = query.Where(p => criteria.Categories.Contains(p.Category.Code));
        }

        if (criteria.KitTypes is { Count: > 0 })
        {
            query = query.Where(p => criteria.KitTypes.Contains(p.KitType.Code));
        }

        if (criteria.Leagues is { Count: > 0 })
        {
            query = query.Where(p => criteria.Leagues.Contains(p.League.Code));
        }

        if (criteria.Teams is { Count: > 0 })
        {
            query = query.Where(p => criteria.Teams.Contains(p.Team.Code));
        }

        if (criteria.Seasons is { Count: > 0 })
        {
            query = query.Where(p => criteria.Seasons.Contains(p.Season.Name));
        }

        query = criteria.Sort switch
        {
            "za" => query.OrderByDescending(p => p.Name).ThenBy(p => p.Id),
            "price_asc" => query.OrderBy(p => p.Price).ThenBy(p => p.Id),
            "price_desc" => query.OrderByDescending(p => p.Price).ThenBy(p => p.Id),
            _ => query.OrderBy(p => p.Name).ThenBy(p => p.Id),
        };

        var total = await query.CountAsync(ct);

        var pageSize = Math.Clamp(criteria.PageSize, 1, 100);
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        var page = Math.Clamp(criteria.Page, 1, totalPages);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new ProductSearchResult
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<ProductFilterOptions> GetFilterOptionsAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var brands = await db.Brands.AsNoTracking().OrderBy(b => b.Code).ToListAsync(ct);
        var categories = await db.Categories.AsNoTracking().OrderBy(c => c.Code).ToListAsync(ct);
        var kitTypes = await db.KitTypes.AsNoTracking().OrderBy(k => k.Code).ToListAsync(ct);
        var leagues = await db.Leagues.AsNoTracking().OrderBy(l => l.Code).ToListAsync(ct);
        var teams = await db.Teams.AsNoTracking().OrderBy(t => t.Code).ToListAsync(ct);
        var seasons = await db.Seasons.AsNoTracking().OrderByDescending(s => s.Name).ToListAsync(ct);

        return new ProductFilterOptions
        {
            Brands = brands,
            Categories = categories,
            KitTypes = kitTypes,
            Leagues = leagues,
            Teams = teams,
            Seasons = seasons,
        };
    }
}
