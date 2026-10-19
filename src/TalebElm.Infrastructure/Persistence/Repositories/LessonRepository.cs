
using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Interfaces;
using TalebElm.Infrastructure.Persistence;

namespace TalebElm.Infrastructure.Persistence.Repositories;

public class LessonRepository(AppDbContext context) : ILessonRepository
{
    public async Task<Lesson?> GetByIdAsync(Guid id)
    {
        return await context.Lessons.FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IReadOnlyList<Lesson>> GetAllAsync()
    {
        return await context.Lessons.AsNoTracking().ToListAsync();
    }

    public async Task AddAsync(Lesson entity)
    {
        await context.Lessons.AddAsync(entity);
    }

    public async Task<IReadOnlyList<Lesson>> GetByModuleIdAsync(Guid moduleId)
    {
        return await context.Lessons
            .Where(l => l.ModuleId == moduleId)
            .OrderBy(l => l.Order)
            .AsNoTracking()
            .ToListAsync();
    }
}
