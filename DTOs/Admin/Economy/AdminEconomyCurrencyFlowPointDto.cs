namespace OceanClean.Api.DTOs.Admin.Economy;

public class AdminEconomyCurrencyFlowPointDto
{
    public string Bucket { get; set; } = string.Empty;

    public int EarnedAmount { get; set; }
    public int SpentAmount { get; set; }
    public int NetAmount { get; set; }

    public uint EarnTransactionCount { get; set; }
    public uint SpendTransactionCount { get; set; }
}