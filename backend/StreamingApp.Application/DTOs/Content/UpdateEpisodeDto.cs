using System.ComponentModel.DataAnnotations;

namespace StreamingApp.Application.DTOs.Content;

public record UpdateEpisodeDto(
    [Required, MaxLength(200)] string Title,
    [MaxLength(2000)] string Description,
    [Range(1, 600)] int DurationMinutes
);
