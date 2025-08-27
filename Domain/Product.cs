namespace DaApi.Domain;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public decimal ListPriceExVat { get; set; }
    public decimal CostPrice { get; set; }
    public string? Brand { get; set; }
    public string? Category { get; set; }
}
