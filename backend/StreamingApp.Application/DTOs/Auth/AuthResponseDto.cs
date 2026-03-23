namespace StreamingApp.Application.DTOs.Auth;

public record AuthResponseDto(
    string AccessToken,
    /// <summary>
    /// Presente apenas em login/register/refresh. Nulo em /me.
    /// O controller é responsável por armazená-lo em cookie HttpOnly — nunca exposto ao JS.
    /// </summary>
    string? RefreshToken,
    string UserId,
    string Email,
    string DisplayName,
    bool IsAdmin,
    string? AvatarUrl
);
