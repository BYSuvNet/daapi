using System.ComponentModel.DataAnnotations;
// DTOs för in/ut (renare kontrakt)
public record ProductCreateDto(
    [property: Required, MinLength(1)] string Name,
    string? Description,
    [property: Range(0, double.MaxValue)] decimal Price
);
