using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StreamingApp.Application.DTOs.Auth;
using StreamingApp.Domain.Common;
using StreamingApp.Domain.Entities;

namespace StreamingApp.Application.Services;

public class AuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _config;

    public AuthService(UserManager<User> userManager, IConfiguration config)
    {
        _userManager = userManager;
        _config = config;
    }

    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
    {
        if (await _userManager.FindByEmailAsync(dto.Email) != null)
            return Result<AuthResponseDto>.Failure("Email já cadastrado.");

        var user = new User
        {
            UserName = dto.Email,
            Email = dto.Email,
            DisplayName = dto.DisplayName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return Result<AuthResponseDto>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, "User");

        var accessToken = await GenerateAccessTokenAsync(user);
        var refreshToken = await SetRefreshTokenAsync(user);

        return Result<AuthResponseDto>.Success(new AuthResponseDto(
            accessToken, refreshToken, user.Id, user.Email!, user.DisplayName, user.IsAdmin, user.AvatarUrl));
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Result<AuthResponseDto>.Unauthorized("Email ou senha inválidos.");

        var accessToken = await GenerateAccessTokenAsync(user);
        var refreshToken = await SetRefreshTokenAsync(user);

        return Result<AuthResponseDto>.Success(new AuthResponseDto(
            accessToken, refreshToken, user.Id, user.Email!, user.DisplayName, user.IsAdmin, user.AvatarUrl));
    }

    public async Task<Result<AuthResponseDto>> GetMeAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return Result<AuthResponseDto>.NotFound();

        var accessToken = await GenerateAccessTokenAsync(user);
        return Result<AuthResponseDto>.Success(new AuthResponseDto(
            accessToken, null, user.Id, user.Email!, user.DisplayName, user.IsAdmin, user.AvatarUrl));
    }

    /// <summary>
    /// Valida o refresh token do cookie, rotaciona e retorna um novo par de tokens.
    /// Cookie value format: "{userId}:{rawRefreshToken}"
    /// </summary>
    public async Task<Result<AuthResponseDto>> RefreshAsync(string cookieValue, CancellationToken ct = default)
    {
        var parts = cookieValue.Split(':', 2);
        if (parts.Length != 2)
            return Result<AuthResponseDto>.Unauthorized("Refresh token inválido.");

        var userId = parts[0];
        var rawToken = parts[1];

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null || user.RefreshTokenHash is null || user.RefreshTokenExpiry is null)
            return Result<AuthResponseDto>.Unauthorized("Refresh token inválido.");

        if (user.RefreshTokenExpiry < DateTime.UtcNow)
            return Result<AuthResponseDto>.Unauthorized("Refresh token expirado.");

        if (!VerifyTokenHash(rawToken, user.RefreshTokenHash))
            return Result<AuthResponseDto>.Unauthorized("Refresh token inválido.");

        var accessToken = await GenerateAccessTokenAsync(user);
        var newRefreshToken = await SetRefreshTokenAsync(user);

        return Result<AuthResponseDto>.Success(new AuthResponseDto(
            accessToken, newRefreshToken, user.Id, user.Email!, user.DisplayName, user.IsAdmin, user.AvatarUrl));
    }

    public async Task<Result<AuthResponseDto>> UpdateProfileAsync(string userId, UpdateProfileDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return Result<AuthResponseDto>.NotFound();

        user.DisplayName = dto.DisplayName;
        user.AvatarUrl = dto.AvatarUrl;
        await _userManager.UpdateAsync(user);

        var accessToken = await GenerateAccessTokenAsync(user);
        return Result<AuthResponseDto>.Success(new AuthResponseDto(
            accessToken, null, user.Id, user.Email!, user.DisplayName, user.IsAdmin, user.AvatarUrl));
    }

    public async Task<Result> LogoutAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return Result.NotFound();

        user.RefreshTokenHash = null;
        user.RefreshTokenExpiry = null;
        await _userManager.UpdateAsync(user);
        return Result.Success();
    }

    // --- Private helpers ---

    private async Task<string> GenerateAccessTokenAsync(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.DisplayName),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Gera, armazena (hashed) e retorna o refresh token raw no formato "{userId}:{rawToken}".
    /// </summary>
    private async Task<string> SetRefreshTokenAsync(User user)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        user.RefreshTokenHash = HashToken(rawToken);
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);
        return $"{user.Id}:{rawToken}";
    }

    private static string HashToken(string rawToken) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

    private static bool VerifyTokenHash(string rawToken, string storedHash) =>
        HashToken(rawToken) == storedHash;
}
