using OceanClean.Api.DTOs.Auth;
using OceanClean.Api.Repositories;

namespace OceanClean.Api.Services;

public class AuthService
{
    private readonly UserRepository _userRepository;

    public AuthService(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        string? ipAddress,
        string? userAgent)
    {
        var username = request.Username.Trim().ToLowerInvariant();
        var displayName = request.DisplayName.Trim();
        var password = request.Password;

        if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Username must be at least 3 characters."
            };
        }

        if (string.IsNullOrWhiteSpace(displayName) || displayName.Length < 3)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Display name must be at least 3 characters."
            };
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Password must be at least 6 characters."
            };
        }

        if (await _userRepository.UsernameExistAsync(username))
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Username is already taken."
            };
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var userId = await _userRepository.CreateUserWithProfileAsync(
            username,
            passwordHash,
            displayName
        );

        await _userRepository.UpdateLastLoginAsync(userId);

        await _userRepository.InsertPlayerLoginLogAsync(
            userId,
            ipAddress,
            userAgent
        );

        return new AuthResponse
        {
            Success = true,
            Message = "User registered successfully.",
            UserId = userId,
            Username = username,
            DisplayName = displayName
        };
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        string? userAgent)
    {
        var username = request.Username.Trim().ToLowerInvariant();

        var user = await _userRepository.GetUserByUsernameAsync(username);

        if (user == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid username or password."
            };
        }

        if (user.Status != "active")
        {
            return new AuthResponse
            {
                Success = false,
                Message = "This account is not active."
            };
        }

        var passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!passwordValid)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid username or password."
            };
        }

        await _userRepository.UpdateLastLoginAsync(user.UserId);

        await _userRepository.InsertPlayerLoginLogAsync(
            user.UserId,
            ipAddress,
            userAgent
        );

        return new AuthResponse
        {
            Success = true,
            Message = "Login successful.",
            UserId = user.UserId,
            Username = user.Username,
            DisplayName = user.DisplayName
        };
    }
}