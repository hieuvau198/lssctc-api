using Lssctc.ProgramManagement.ClassManage.Timeslots.Dtos;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.ClassManage.Timeslots.Services
{
    public interface ITimeslotService
    {
        
        Task CreateAttendanceForClassAsync(int classId);
        Task<TimeslotDto> CreateTimeslotAsync(CreateTimeslotDto dto, int creatorId);
        Task<IEnumerable<TimeslotDto>> CreateListTimeslotAsync(CreateListTimeslotDto dto, int creatorId);
        Task<TimeslotDto> UpdateTimeslotAsync(int timeslotId, UpdateTimeslotDto dto, int updaterId);
        Task<IEnumerable<ImportTimeslotRecordDto>> ImportTimeslotsAsync(int classId, IFormFile file, int creatorId);
        Task<IEnumerable<TimeslotDto>> GetTimeslotsByClassAndInstructorAsync(int classId, int instructorId);
        // API 1b: Xem tất cả slot dạy trong tuần
        Task<IEnumerable<TimeslotDto>> GetTimeslotsByInstructorForWeekAsync(int instructorId, DateTime weekStart);
        // API 2: Lấy danh sách học viên cần điểm danh cho 1 slot
        Task<TimeslotAttendanceDto> GetAttendanceListForTimeslotAsync(int timeslotId, int instructorId);
        // API 3: Submit danh sách điểm danh cho 1 slot
        Task<TimeslotAttendanceDto> SubmitAttendanceForTimeslotAsync(int timeslotId, int instructorId, SubmitAttendanceDto dto);

        // === Trainee APIs ===
        // API 4: Học viên xem danh sách slot cho 1 lớp
        Task<IEnumerable<TimeslotDto>> GetTimeslotsByClassAndTraineeAsync(int classId, int traineeId);
        // API 5: Học viên xem danh sách tất cả slot cho 1 tuần
        Task<IEnumerable<TimeslotDto>> GetTimeslotsByTraineeForWeekAsync(int traineeId, DateTime weekStart);
        Task<IEnumerable<TraineeAttendanceRecordDto>> GetTraineeAttendanceHistoryAsync(int classId, int traineeId);
    }

}