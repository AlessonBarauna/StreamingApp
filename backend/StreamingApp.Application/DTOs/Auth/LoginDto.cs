using System.ComponentModel.DataAnnotations;

namespace StreamingApp.Application.DTOs.Auth;

public record LoginDto(
    [Required, EmailAddress] string Email,
    [Required] string Password
);
