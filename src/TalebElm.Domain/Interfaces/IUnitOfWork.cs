namespace TalebElm.Domain.Interfaces;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    ITrackRepository Tracks { get; }
    IExamRepository Exams { get; }
    IUserProgressRepository UserProgresses { get; }
    IModuleRepository Modules { get; }
    ILessonRepository Lessons { get; }
    Task<int> SaveChangesAsync();
}
