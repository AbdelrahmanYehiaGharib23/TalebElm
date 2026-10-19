
namespace TalebElm.Application.DTOs;

public record LessonResourceResponse(
    Guid Id,
    string Title,
    string Kind,
    string? Url,
    string? Citation,
    int Order
);
