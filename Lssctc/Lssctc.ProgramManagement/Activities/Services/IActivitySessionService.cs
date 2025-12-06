using Lssctc.ProgramManagement.Activities.Dtos;

namespace Lssctc.ProgramManagement.Activities.Services
{
    public interface IActivitySessionService
    {
        // Task 1: Tạo session cho tất cả activity khi Class Start
        Task CreateSessionsOnClassStartAsync(int classId);

        // Task 2: Kiểm tra quyền truy cập Activity (sẽ được gọi bởi các API tương tác)
        Task CheckActivityAccess(int classId, int activityId);

        // Task 3: API Create Session (Manual)
        Task<ActivitySessionDto> CreateActivitySessionAsync(CreateActivitySessionDto dto);

        // Task 4: API Edit Session
        Task<ActivitySessionDto> UpdateActivitySessionAsync(int sessionId, UpdateActivitySessionDto dto);

        // Retrieval APIs
        Task<ActivitySessionDto> GetActivitySessionByIdAsync(int sessionId);
        Task<IEnumerable<ActivitySessionDto>> GetActivitySessionsByClassIdAsync(int classId);
    }
}