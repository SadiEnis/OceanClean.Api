namespace OceanClean.Api.DTOs.Admin.Economy;

public class AdminEconomyItemSummaryResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public string Range { get; set; } = string.Empty;
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public List<AdminEconomyItemSummaryDto> TopPurchasedItems { get; set; } = new();
    public List<AdminEconomyItemSummaryDto> TopUsedItems { get; set; } = new();
}