using System.ComponentModel.DataAnnotations;
// Modell
public class Product
{
    public int Id { get; set; }

    [Required, MinLength(1)]
    public string Name { get; set; } = "";

    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
}
