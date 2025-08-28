using System.ComponentModel.DataAnnotations;

using DaApi.Domain;

public record OrderItemCreateDto(
    [property: Required] int ProductId,
    [property: Range(1, int.MaxValue)] int Quantity
);

public record OrderCreateDto(
    [property: Required] int CustomerId,
    [property: MinLength(1)] List<OrderItemCreateDto> Items
);

public record OrderStatusUpdateDto(
    [property: Required] OrderStatus Status
);
