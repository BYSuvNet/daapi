//Denna klass kopplar ihop en order med en touchpoint (via TouchpointId)
public class OrderAttribution
{
    public string OrderId { get; set; } = default!;
    public int TouchpointId { get; set; }
    public string Model { get; set; } = "last_click"; // last_click, first_click, time_decay, mm.
    public decimal Credit { get; set; }               // 0–1 (andel av ordern)
    public DateTime ComputedAtUtc { get; set; }

    public Order Order { get; set; } = null!;
    public Touchpoint Touchpoint { get; set; } = null!;
}

public class Touchpoint
{
    public int TouchpointId { get; set; }
    public DateTime OccurredAt { get; set; }

    public string Source { get; set; } = "";
    public string Medium { get; set; } = "";
    public string Campaign { get; set; } = "";
    public string Term { get; set; } = "";
    public string Content { get; set; } = "";

    public string ReferrerUrl { get; set; } = "";
    public string LandingUrl { get; set; } = "";

    public string ClientId { get; set; } = ""; // anonym besökare
    public string? SessionId { get; set; }
    public string? UserId { get; set; }

    public string? Device { get; set; }
    public string? Os { get; set; }
    public string? Browser { get; set; }
    public string? Country { get; set; }
    public string? ConsentStatus { get; set; }

    public ICollection<TouchpointExternalId> ExternalIds { get; set; } = new List<TouchpointExternalId>();
}

public class TouchpointExternalId
{
    public int TouchpointExternalIdId { get; set; }
    public int TouchpointId { get; set; }
    public Touchpoint Touchpoint { get; set; } = null!;

    public string Provider { get; set; } = ""; // "google_ads", "meta", "bing", "tiktok" …
    public string IdType { get; set; } = "";   // "gclid", "wbraid", "fbclid", "msclkid", "ttclid" …
    public string IdValue { get; set; } = "";
}
