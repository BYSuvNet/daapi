namespace DaApi.Domain;

public class Return
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public DateTime ReturnDateUtc { get; set; } = DateTime.UtcNow;
    public string Reason { get; set; } = "";
    public decimal Amount { get; set; }  // Belopp som returneras (inkl moms)
    public List<Product> ReturnedProducts { get; set; } = [];
}