using System.ComponentModel.DataAnnotations;

namespace StreamingApp.Application.DTOs.Auth;

public record UpdateProfileDto(
    [Required, MaxLength(100)] string DisplayName,
    [MaxLength(500)] string? AvatarUrl
);
