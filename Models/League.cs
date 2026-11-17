namespace blazor_app.Models;

public class League
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Label { get; set; } = null!;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
