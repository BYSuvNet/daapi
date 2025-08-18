using System.ComponentModel.DataAnnotations;

public record CustomerCreateDto(
    [property: Required, MinLength(1)] string Name,
    [property: Required, EmailAddress] string Email,
    [property: Phone] string? Phone,
    string Address
);

public record CustomerUpdateDto(
    [property: Required, MinLength(1)] string Name,
    [property: Required, EmailAddress] string Email,
    [property: Phone] string? Phone,
    string Address
);