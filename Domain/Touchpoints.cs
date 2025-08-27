namespace DaApi.Domain;

public class OrderAttribution
{
    public string OrderId { get; set; } = default!;
    public Order Order { get; set; } = null!;

    public int TouchpointId { get; set; }
    public string Model { get; set; } = "last_click"; // last_click, first_click, time_decay, mm.
    public decimal Credit { get; set; }               // 0–1 (andel av ordern)
    public DateTime ComputedAtUtc { get; set; }

    public Touchpoint Touchpoint { get; set; } = null!;
}

public class Touchpoint
{
    public int TouchpointId { get; set; }
    public DateTime OccurredAt { get; set; }

    public string Source { get; set; } = "";
    public string Medium { get; set; } = "";
    public string Campaign { get; set; } = "";
    public string Content { get; set; } = "";

    public string ClientId { get; set; } = ""; // anonym besökare
    public string? SessionId { get; set; }
    public string? UserId { get; set; }

    public string Clid { get; set; } = ""; // click id, använd source för att avgöra om det är gclid, fbclid, etc.

    public string? Device { get; set; }
    public string? Os { get; set; }
    public string? Browser { get; set; }
    public string? Country { get; set; }
}
