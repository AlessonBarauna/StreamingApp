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

        var token = await GenerateAccessTokenAsync(user);
        return Result<AuthResponseDto>.Success(new AuthResponseDto(token, user.Id, user.Email!, user.DisplayName, user.IsAdmin, user.AvatarUrl));
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Result<AuthResponseDto>.Unauthorized("Email ou senha inválidos.");

        var token = await GenerateAccessTokenAsync(user);
        return Result<AuthResponseDto>.Success(new AuthResponseDto(token, user.Id, user.Email!, user.DisplayName, user.IsAdmin, user.AvatarUrl));
    }

    public async Task<Result<AuthResponseDto>> GetMeAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Result<AuthResponseDto>.NotFound();

        var token = await GenerateAccessTokenAsync(user);
        return Result<AuthResponseDto>.Success(new AuthResponseDto(token, user.Id, user.Email!, user.DisplayName, user.IsAdmin, user.AvatarUrl));
    }

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
}
