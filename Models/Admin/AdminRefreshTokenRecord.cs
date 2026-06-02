namespace OceanClean.Api.Models.Admin;

public class AdminRefreshTokenRecord
{
    public ulong RefreshTokenId { get; set; }
    public ulong AdminUserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}