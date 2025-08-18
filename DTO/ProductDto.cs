// Dtos/ProductDtos.cs
using System.ComponentModel.DataAnnotations;

public record ProductCreateDto(
    [property: Required, MinLength(1)] string Name,
    string? Description,
    [property: Range(0, double.MaxValue)] decimal Price,
    string? Brand,
    string? Category,
    [property: Url] string? ImageUrl
);

public record ProductUpdateDto(
    [property: Required, MinLength(1)] string Name,
    string? Description,
    [property: Range(0, double.MaxValue)] decimal Price,
    string? Brand,
    string? Category,
    [property: Url] string? ImageUrl
);
