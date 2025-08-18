using System.ComponentModel.DataAnnotations;

public class Customer
{
    public int Id { get; set; }
    [Required, MinLength(1)]
    public string Name { get; set; } = "";
    [Required, EmailAddress]
    public string Email { get; set; } = "";
    [Phone]
    public string? Phone { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Address { get; set; } = "";
}
