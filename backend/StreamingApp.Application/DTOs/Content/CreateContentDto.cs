using System.ComponentModel.DataAnnotations;

namespace StreamingApp.Application.DTOs.Content;

public record CreateContentDto(
    [Required] string Title,
    [Required] string Description,
    [Required] string Type,
    int ReleaseYear,
    int? DurationMinutes,
    string AgeRating,
    [Required] Guid CategoryId,
    bool IsFeatured = false
);
