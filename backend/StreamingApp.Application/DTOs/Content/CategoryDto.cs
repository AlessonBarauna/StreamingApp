namespace StreamingApp.Application.DTOs.Content;

public record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string IconName
);
