using System.ComponentModel.DataAnnotations;

namespace StreamingApp.Application.DTOs.Content;

public record CreateEpisodeDto(
    [Required, MaxLength(200)] string Title,
    [MaxLength(2000)] string Description,
    [Range(1, 50)] int SeasonNumber,
    [Range(1, 500)] int EpisodeNumber,
    [Range(1, 600)] int DurationMinutes
);
