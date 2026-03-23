namespace StreamingApp.Application.DTOs.Content;

public record PagedResultDto<T>(
    IEnumerable<T> Items,
    int Total,
    int Page,
    int Limit
);
