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

    [Required]
    public Address Address { get; set; } = new Address();

    public DateTime? BirthDate { get; set; } = null;
}

public class Address
{
    [Required, MinLength(1)]
    public string Street { get; set; } = "";

    [Required]
    public string PostalNumber { get; set; } = "";

    [Required, MinLength(1)]
    public string City { get; set; } = "";

    [Required, MinLength(1)]
    public string Country { get; set; } = "";
}
