
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Application.Services.Concrete;

public class LessonService(IUnitOfWork unitOfWork) : ILessonService
{
    public async Task<IReadOnlyList<LessonResponse>> GetByModuleAsync(Guid moduleId)
    {
        var lessons = await unitOfWork.Lessons.GetByModuleIdAsync(moduleId);

        return lessons
            .Select(lesson => new LessonResponse(
                lesson.Id,
                lesson.Title,
                lesson.ModuleId))
            .ToList();
    }

    public async Task<LessonDetailResponse> GetByIdAsync(Guid id)
    {
        var lesson = await unitOfWork.Lessons.GetByIdAsync(id);

        if (lesson is null)
        {
            throw new NotFoundException(
                $"Lesson with id '{id}' was not found.");
        }

        return new LessonDetailResponse(
            lesson.Id,
            lesson.ModuleId,
            lesson.Title,
            lesson.Content,
            lesson.Order,
            Array.Empty<LessonResourceResponse>());
    }
}
