namespace blazor_app.Models;

public class Brand
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Label { get; set; } = null!;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
