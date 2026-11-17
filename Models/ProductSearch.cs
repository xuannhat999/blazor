namespace blazor_app.Models;

public class ProductSearchCriteria
{
    public string? Keyword { get; set; }
    public string Sort { get; set; } = "az";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public List<string> Brands { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public List<string> KitTypes { get; set; } = new();
    public List<string> Leagues { get; set; } = new();
    public List<string> Teams { get; set; } = new();
    public List<string> Seasons { get; set; } = new();
}

public class ProductSearchResult
{
    public IReadOnlyList<Product> Items { get; set; } = Array.Empty<Product>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(Total / (double)PageSize));
}

public class ProductFilterOptions
{
    public IReadOnlyList<Brand> Brands { get; set; } = Array.Empty<Brand>();
    public IReadOnlyList<Category> Categories { get; set; } = Array.Empty<Category>();
    public IReadOnlyList<KitType> KitTypes { get; set; } = Array.Empty<KitType>();
    public IReadOnlyList<League> Leagues { get; set; } = Array.Empty<League>();
    public IReadOnlyList<Team> Teams { get; set; } = Array.Empty<Team>();
    public IReadOnlyList<Season> Seasons { get; set; } = Array.Empty<Season>();
}
