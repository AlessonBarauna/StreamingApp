using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamingApp.Application.DTOs.Auth;
using StreamingApp.Application.Services;
using System.Security.Claims;

namespace StreamingApp.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string RefreshTokenCookie = "refresh_token";
    private readonly AuthService _authService;

    public AuthController(AuthService authService) => _authService = authService;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto, CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(dto, ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });

        SetRefreshCookie(result.Value!.RefreshToken!);
        return Ok(ToClientResponse(result.Value));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(dto, ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });

        SetRefreshCookie(result.Value!.RefreshToken!);
        return Ok(ToClientResponse(result.Value));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _authService.GetMeAsync(userId, ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });
        return Ok(ToClientResponse(result.Value!));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        var cookieValue = Request.Cookies[RefreshTokenCookie];
        if (string.IsNullOrEmpty(cookieValue))
            return Unauthorized(new { error = "Refresh token ausente." });

        var result = await _authService.RefreshAsync(cookieValue, ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });

        SetRefreshCookie(result.Value!.RefreshToken!);
        return Ok(ToClientResponse(result.Value));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _authService.LogoutAsync(userId, ct);

        Response.Cookies.Delete(RefreshTokenCookie);
        return Ok(new { message = "Logged out" });
    }

    // --- Helpers ---

    private void SetRefreshCookie(string rawRefreshToken)
    {
        Response.Cookies.Append(RefreshTokenCookie, rawRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            MaxAge = TimeSpan.FromDays(7)
        });
    }

    /// <summary>
    /// Remove o RefreshToken do response antes de enviar ao cliente.
    /// O refresh token vai apenas no cookie HttpOnly, nunca no corpo da resposta.
    /// </summary>
    private static object ToClientResponse(AuthResponseDto dto) => new
    {
        dto.AccessToken,
        dto.UserId,
        dto.Email,
        dto.DisplayName,
        dto.IsAdmin,
        dto.AvatarUrl
    };
}
