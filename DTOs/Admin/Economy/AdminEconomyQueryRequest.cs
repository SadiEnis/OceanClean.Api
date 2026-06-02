namespace OceanClean.Api.DTOs.Admin.Economy;

public class AdminEconomyQueryRequest
{
    public string Range { get; set; } = "all";

    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public int Limit { get; set; } = 10;
}