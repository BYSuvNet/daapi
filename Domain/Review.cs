using System.ComponentModel.DataAnnotations;
using DaApi.Domain;

public class ProductReview
{
    public int Id { get; set; }

    [Required]
    public int ProductId { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public Product? Product { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required, Range(1, 5)]
    public int Rating { get; set; } = 1;

    [MaxLength(1000)]
    public string? Comment { get; set; }

    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
}