using Lssctc.ProgramManagement.Activities.Dtos;

namespace Lssctc.ProgramManagement.Activities.Services
{
    public interface IActivitySessionService
    {
        Task CreateSessionsOnClassStartAsync(int classId);
        Task CheckActivityAccess(int classId, int activityId);
        Task<ActivitySessionDto> CreateActivitySessionAsync(CreateActivitySessionDto dto);
        Task<ActivitySessionDto> UpdateActivitySessionAsync(int sessionId, UpdateActivitySessionDto dto);
        Task<ActivitySessionDto> GetActivitySessionByIdAsync(int sessionId);
        Task<IEnumerable<ActivitySessionDto>> GetActivitySessionsByClassIdAsync(int classId);
    }
}