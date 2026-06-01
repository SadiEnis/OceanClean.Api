using Microsoft.Extensions.Options;
using OceanClean.Api.DTOs.Admin.Auth;
using OceanClean.Api.Repositories;

namespace OceanClean.Api.Security;

public class AdminAuthService
{
    private readonly AdminAuthRepository _adminAuthRepository;
    private readonly JwtTokenService _jwtTokenService;
    private readonly RefreshTokenService _refreshTokenService;
    private readonly JwtSettings _jwtSettings;

    public AdminAuthService(
        AdminAuthRepository adminAuthRepository,
        JwtTokenService jwtTokenService,
        RefreshTokenService refreshTokenService,
        IOptions<JwtSettings> jwtOptions)
    {
        _adminAuthRepository = adminAuthRepository;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<AdminLoginResponse> LoginAsync(
        AdminLoginRequest request,
        string? ipAddress,
        string? userAgent)
    {
        string? validationError = ValidateLoginRequest(request);

        if (validationError != null)
        {
            return new AdminLoginResponse
            {
                Success = false,
                Message = validationError
            };
        }

        var admin = await _adminAuthRepository.GetAdminByUsernameAsync(request.Username.Trim());

        if (admin == null)
        {
            return new AdminLoginResponse
            {
                Success = false,
                Message = "Invalid username or password."
            };
        }

        if (!string.Equals(admin.AdminStatus, "active", StringComparison.OrdinalIgnoreCase))
        {
            return new AdminLoginResponse
            {
                Success = false,
                Message = "Admin account is not active."
            };
        }

        bool passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash);

        if (!passwordValid)
        {
            return new AdminLoginResponse
            {
                Success = false,
                Message = "Invalid username or password."
            };
        }

        string accessToken = _jwtTokenService.GenerateAdminAccessToken(admin);

        string refreshToken = _refreshTokenService.GenerateRefreshToken();
        string refreshTokenHash = _refreshTokenService.HashRefreshToken(refreshToken);

        DateTime refreshTokenExpiresAt = DateTime.UtcNow.AddDays(
            _jwtSettings.RefreshTokenExpirationDays
        );

        await _adminAuthRepository.SaveRefreshTokenAsync(
            admin.AdminUserId,
            refreshTokenHash,
            refreshTokenExpiresAt,
            ipAddress,
            userAgent
        );

        await _adminAuthRepository.UpdateLastLoginAsync(admin.AdminUserId);

        await _adminAuthRepository.InsertAdminAuditLogAsync(
            admin.AdminUserId,
            actionType: "admin_login",
            targetType: "admin_user",
            targetId: admin.AdminUserId,
            oldValue: null,
            newValue: "login_success",
            ipAddress: ipAddress,
            userAgent: userAgent
        );

        return new AdminLoginResponse
        {
            Success = true,
            Message = "Admin login successful.",
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Admin = new AdminUserDto
            {
                AdminUserId = admin.AdminUserId,
                Username = admin.Username,
                Role = admin.AdminRole
            }
        };
    }

    private static string? ValidateLoginRequest(AdminLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            return "Username is required.";

        if (string.IsNullOrWhiteSpace(request.Password))
            return "Password is required.";

        if (request.Username.Length > 50)
            return "Username cannot be longer than 50 characters.";

        return null;
    }

    public async Task<AdminRefreshTokenResponse> RefreshAsync(AdminRefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return new AdminRefreshTokenResponse
            {
                Success = false,
                Message = "Refresh token is required."
            };
        }

        string tokenHash = _refreshTokenService.HashRefreshToken(request.RefreshToken);

        var refreshTokenRecord = await _adminAuthRepository.GetRefreshTokenByHashAsync(tokenHash);

        if (refreshTokenRecord == null)
        {
            return new AdminRefreshTokenResponse
            {
                Success = false,
                Message = "Invalid refresh token."
            };
        }

        if (refreshTokenRecord.RevokedAt != null)
        {
            return new AdminRefreshTokenResponse
            {
                Success = false,
                Message = "Refresh token has been revoked."
            };
        }

        if (refreshTokenRecord.ExpiresAt <= DateTime.UtcNow)
        {
            return new AdminRefreshTokenResponse
            {
                Success = false,
                Message = "Refresh token has expired."
            };
        }

        var admin = await _adminAuthRepository.GetAdminByIdAsync(refreshTokenRecord.AdminUserId);

        if (admin == null)
        {
            return new AdminRefreshTokenResponse
            {
                Success = false,
                Message = "Admin user not found."
            };
        }

        if (!string.Equals(admin.AdminStatus, "active", StringComparison.OrdinalIgnoreCase))
        {
            return new AdminRefreshTokenResponse
            {
                Success = false,
                Message = "Admin account is not active."
            };
        }

        string accessToken = _jwtTokenService.GenerateAdminAccessToken(admin);

        return new AdminRefreshTokenResponse
        {
            Success = true,
            Message = "Access token refreshed successfully.",
            AccessToken = accessToken,
            Admin = new AdminUserDto
            {
                AdminUserId = admin.AdminUserId,
                Username = admin.Username,
                Role = admin.AdminRole
            }
        };
    }
    
    public async Task<AdminLogoutResponse> LogoutAsync(
        AdminLogoutRequest request,
        string? ipAddress,
        string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return new AdminLogoutResponse
            {
                Success = false,
                Message = "Refresh token is required."
            };
        }

        string tokenHash = _refreshTokenService.HashRefreshToken(request.RefreshToken);

        var refreshTokenRecord = await _adminAuthRepository.GetRefreshTokenByHashAsync(tokenHash);

        if (refreshTokenRecord == null)
        {
            return new AdminLogoutResponse
            {
                Success = false,
                Message = "Invalid refresh token."
            };
        }

        if (refreshTokenRecord.RevokedAt != null)
        {
            return new AdminLogoutResponse
            {
                Success = true,
                Message = "Already logged out."
            };
        }

        await _adminAuthRepository.RevokeRefreshTokenAsync(
            refreshTokenRecord.RefreshTokenId,
            ipAddress
        );

        await _adminAuthRepository.InsertAdminAuditLogAsync(
            refreshTokenRecord.AdminUserId,
            actionType: "admin_logout",
            targetType: "admin_user",
            targetId: refreshTokenRecord.AdminUserId,
            oldValue: "active_session",
            newValue: "logged_out",
            ipAddress: ipAddress,
            userAgent: userAgent
        );

        return new AdminLogoutResponse
        {
            Success = true,
            Message = "Admin logout successful."
        };
    }
}