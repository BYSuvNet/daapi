using System.ComponentModel.DataAnnotations;

namespace DaApi.AdsAPI;

public class ConversionEvent
{
    public long Id { get; set; }
    [MaxLength(260)] public string? Gclid { get; set; }
    public DateTimeOffset ConversionDate { get; set; }
    public long ValueMicros { get; set; }
    [MaxLength(3)]
    public string CurrencyCode { get; set; } = "SEK";
    [MaxLength(128)]
    public string? OrderId { get; set; }

    //Möjligen ha items-lista med SKU och antal
}

public class ImpressionEvent
{
    public long Id { get; set; }
    public int CampaignId { get; set; }       // FK → Campaign
    public DateTimeOffset OccurredAt { get; set; }
}

public class ClickEvent
{
    public long Id { get; set; }
    [MaxLength(260)] public string? Gclid { get; set; }
    public int CampaignId { get; set; }
    public DateTimeOffset ClickedAt { get; set; }
    public long CpcMicros { get; set; } // Vad kostade klicket i mikroenheter
}