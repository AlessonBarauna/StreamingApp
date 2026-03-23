using System.ComponentModel.DataAnnotations;

namespace StreamingApp.Application.DTOs.Stream;

public record SaveProgressDto(
    [Required] int Seconds,
    [Required] int Total
);

public record ProgressResponseDto(
    int ProgressSeconds,
    int TotalSeconds,
    double PercentComplete,
    bool IsCompleted
);
