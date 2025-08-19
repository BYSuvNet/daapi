// Dtos/ProductDtos.cs
using System.ComponentModel.DataAnnotations;

public record ProductCreateDto(
    [property: Required, MinLength(2)] string Name,
    [property: Required, MinLength(5)] string? Description,
    [property: Required, Range(0, double.MaxValue)] decimal Price,
    [property: Required, MinLength(1)] string? Brand,
    [property: Required, MinLength(1)] string? Category,
    [property: Url] string? Url,
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
