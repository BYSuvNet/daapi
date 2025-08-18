// Models/Order.cs
using System.ComponentModel.DataAnnotations;

public class Order
{
    public int Id { get; set; }

    [Required]
    public int CustomerId { get; set; }

    // Denormaliseras för bekvämlighet (lagras vid ordertillfället)
    [Required, MinLength(1)]
    public string CustomerName { get; set; } = "";

    [Required, EmailAddress]
    public string CustomerEmail { get; set; } = "";

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public List<OrderItem> Items { get; set; } = new();

    public decimal TotalAmount => Items.Sum(i => i.Price * i.Quantity);

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
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
