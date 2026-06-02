namespace OceanClean.Api.DTOs.Admin.Economy;

public class AdminEconomyCurrencyFlowResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public string Range { get; set; } = string.Empty;
    public string BucketType { get; set; } = string.Empty;

    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public List<AdminEconomyCurrencyFlowPointDto> Points { get; set; } = new();
}