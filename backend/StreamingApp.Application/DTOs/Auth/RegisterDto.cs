using System.ComponentModel.DataAnnotations;

namespace StreamingApp.Application.DTOs.Auth;

public record RegisterDto(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    [Required] string DisplayName
);
