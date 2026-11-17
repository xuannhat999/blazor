using blazor_app.Models;

namespace blazor_app.Interfaces;

public interface IProductService
{
    Task<IReadOnlyList<Product>> GetByCategoryAsync(string categoryCode, int take = 12, CancellationToken ct = default);

    Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<Product>> GetRelatedAsync(int productId, int take = 10, CancellationToken ct = default);

    Task<ProductSearchResult> SearchAsync(ProductSearchCriteria criteria, CancellationToken ct = default);

    Task<ProductFilterOptions> GetFilterOptionsAsync(CancellationToken ct = default);
}
