namespace blazor_app.Models;

public class Product
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public decimal Price { get; set; }
  public decimal? OldPrice { get; set; }
  public string SourceUrl { get; set; } = null!;

  public int BrandId { get; set; }
  public int CategoryId { get; set; }
  public int TeamId { get; set; }
  public int LeagueId { get; set; }
  public int KitTypeId { get; set; }
  public int SeasonId { get; set; }

  public Brand Brand { get; set; } = null!;
  public Category Category { get; set; } = null!;
  public Team Team { get; set; } = null!;
  public League League { get; set; } = null!;
  public KitType KitType { get; set; } = null!;
  public Season Season { get; set; } = null!;

  public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
}
