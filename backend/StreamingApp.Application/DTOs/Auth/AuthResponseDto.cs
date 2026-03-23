namespace StreamingApp.Application.DTOs.Auth;

public record AuthResponseDto(
    string AccessToken,
    string UserId,
    string Email,
    string DisplayName,
    bool IsAdmin,
    string? AvatarUrl
);
