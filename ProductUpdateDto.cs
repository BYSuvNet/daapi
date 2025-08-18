using System.ComponentModel.DataAnnotations;

public record ProductUpdateDto(
    [property: Required, MinLength(1)] string Name,
    string? Description,
    [property: Range(0, double.MaxValue)] decimal Price
);
