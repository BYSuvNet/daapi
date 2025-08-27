namespace DaApi.AdsAPI;

using System.ComponentModel.DataAnnotations;

public class Campaign
{
    public int Id { get; set; }
    [Required, MaxLength(120)]
    public string Name { get; set; } = "";
    public string Status { get; set; } = "enabled"; // enabled, paused, removed
    public string AdvertisingChannel { get; set; } = "shopping"; //search, pmax, 
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    public int Impressions { get; set; }
    public int Clicks { get; set; }
    public long CostMicros { get; set; }
    public double Conversions { get; set; } //This means: Total number of conversions
    public double ConversionsValue { get; set; }  //This means: Total value of all conversions
}
