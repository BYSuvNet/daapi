// Models/Order.cs
using System.ComponentModel.DataAnnotations;

public class Order
{
    public int Id { get; set; }
    public DateTime OrderDateUtc { get; set; } = DateTime.UtcNow;

    public int? CustomerId { get; set; }     // intern användare (om inloggad)
    public string? ClientId { get; set; }   // first-party cookie / GA4 user_pseudo_id
    public string? SessionId { get; set; }  // sessions-id för besöket då ordern lades

    public string Currency { get; set; } = "SEK";
    public decimal TotalAmount => Items.Sum(i => i.Price * i.Quantity);
    public List<OrderItem> Items { get; set; } = new();
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public ICollection<OrderAttribution> Attributions { get; set; } = [];
}

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

public class OrderItem
{
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }      // <-- Viktigt för relationen

    [System.Text.Json.Serialization.JsonIgnore]
    public Order? Order { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;

    // Pris låses in vid ordern (kopieras från Product.Price)
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
}
