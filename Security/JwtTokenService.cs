using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OceanClean.Api.Models.Admin;

namespace OceanClean.Api.Security;

public class JwtTokenService
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public string GenerateAdminAccessToken(AdminUserRecord admin)
    {
        if (string.IsNullOrWhiteSpace(_jwtSettings.SecretKey))
            throw new InvalidOperationException("JWT SecretKey is missing.");

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, admin.AdminUserId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, admin.Username),
            new Claim(ClaimTypes.NameIdentifier, admin.AdminUserId.ToString()),
            new Claim(ClaimTypes.Name, admin.Username),
            new Claim(ClaimTypes.Role, admin.AdminRole),
            new Claim("admin_user_id", admin.AdminUserId.ToString()),
            new Claim("admin_role", admin.AdminRole)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)
        );

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var expires = DateTime.UtcNow.AddMinutes(
            _jwtSettings.AccessTokenExpirationMinutes
        );

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}