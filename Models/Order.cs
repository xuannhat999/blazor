namespace blazor_app.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Address { get; set; } = null!;
    public string PayMethod { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public decimal Total { get; set; }

    public User User { get; set; } = null!;
    public ICollection<OrderDetail> Items { get; set; } = new List<OrderDetail>();
}
