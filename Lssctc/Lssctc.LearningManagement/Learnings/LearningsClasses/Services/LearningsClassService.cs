using Lssctc.LearningManagement.Learnings.LearningsClasses.Dtos;
using Lssctc.Share.Common;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.LearningManagement.Learnings.LearningsClasses.Services
{
    public class LearningsClassService : ILearningsClassService
    {
        private readonly IUnitOfWork _uow;
        public LearningsClassService(IUnitOfWork uow)
        {
            _uow = uow;
        }
        public async Task<List<LearningsClassDto>> GetAllClassesByTraineeId(int traineeId)
        {
            var classes = await _uow.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.ClassCode)
                .Include(c => c.ProgramCourse)
                    .ThenInclude(pc => pc.Courses)
                        .ThenInclude(co => co.CourseCode)
                .Where(c => c.ClassMembers.Any(cm => cm.TraineeId == traineeId))
                .ToListAsync();

            return classes.Select(c => MapToLearningsClassDto(c, traineeId)).ToList();
        }

        public async Task<LearningsClassDto> GetClassByClassIdAndTraineeId(int classId, int traineeId)
        {
            var c = await _uow.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.ClassCode)
                .Include(c => c.ProgramCourse)
                    .ThenInclude(pc => pc.Courses)
                        .ThenInclude(co => co.CourseCode)
                .Include(c => c.ClassMembers)
                    .ThenInclude(cm => cm.TrainingProgresses)
                .FirstOrDefaultAsync(c => c.Id == classId && c.ClassMembers.Any(cm => cm.TraineeId == traineeId));

            if (c == null)
                throw new KeyNotFoundException($"Class with ID {classId} for trainee {traineeId} not found.");

            return MapToLearningsClassDto(c, traineeId);
        }



        public async Task<PagedResult<LearningsClassDto>> GetClassesByTraineeIdPaged(int traineeId, int pageIndex, int pageSize)
        {
            var query = _uow.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.ClassCode)
                .Include(c => c.ProgramCourse)
                    .ThenInclude(pc => pc.Courses)
                        .ThenInclude(co => co.CourseCode)
                .Where(c => c.ClassMembers.Any(cm => cm.TraineeId == traineeId));

            var totalCount = await query.CountAsync();

            var classes = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = classes.Select(c => MapToLearningsClassDto(c, traineeId)).ToList();

            return new PagedResult<LearningsClassDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = pageIndex,
                PageSize = pageSize
            };
        }

        private LearningsClassDto MapToLearningsClassDto( Share.Entities.Class c, int traineeId)
        {
            var member = c.ClassMembers.FirstOrDefault(cm => cm.TraineeId == traineeId);
            var course = c.ProgramCourse?.Courses;
            var progress = (decimal?)member?.TrainingProgresses?.FirstOrDefault()?.ProgressPercentage ?? 0;

            string classStatusName = Enum.IsDefined(typeof(ClassStatus), c.Status)
                ? ((ClassStatus)c.Status).ToString()
                : "Unknown";

            return new LearningsClassDto
            {
                ClassId = c.Id,
                ClassName = c.Name,
                ClassStartDate = c.StartDate,
                ClassEndDate = c.EndDate,
                ClassCapacity = c.Capacity,
                ClassCode = c.ClassCode?.Name ?? "LSSCTC Class",
                ProgramCourseId = c.ProgramCourseId,
                CourseId = course?.Id ?? 0,
                CourseCode = course?.CourseCode?.Name ?? "LSSCTC Course Code",
                CourseName = course?.Name ?? "LSSCTC Course Name",
                CourseDescription = course?.Description ?? "LSSCTC Course Description",
                CourseDurationHours = course?.DurationHours ?? 0,
                ClassStatus = classStatusName,
                ClassProgress = progress
            };
        }
    }
}
