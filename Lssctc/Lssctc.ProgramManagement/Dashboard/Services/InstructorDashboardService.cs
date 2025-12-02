using Lssctc.ProgramManagement.Dashboard.Dtos;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.Dashboard.Services
{
    public class InstructorDashboardService : IInstructorDashboardService
    {
        private readonly IUnitOfWork _uow;

        public InstructorDashboardService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        #region Part 1: Overview (Summary Cards)

        public async Task<InstructorSummaryDto> GetInstructorSummaryAsync(int instructorId)
        {
            // Validate instructor exists
            var instructorExists = await _uow.InstructorRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .AnyAsync(i => i.Id == instructorId && i.IsDeleted == false);

            if (!instructorExists)
                throw new KeyNotFoundException($"Instructor with ID {instructorId} not found or has been deleted.");

            // Total Assigned Trainees: Count unique Trainees enrolled in classes assigned to this instructor
            // Path: ClassInstructor -> Class -> Enrollment -> Count distinct TraineeId
            var totalAssignedTrainees = await _uow.ClassInstructorRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(ci => ci.InstructorId == instructorId)
                .Include(ci => ci.Class)
                    .ThenInclude(c => c.Enrollments)
                .Where(ci => ci.Class != null)
                .SelectMany(ci => ci.Class.Enrollments)
                .Where(e => e.IsDeleted == false)
                .Select(e => e.TraineeId)
                .Distinct()
                .CountAsync();

            // Total Assigned Classes: Count classes where this instructor is assigned
            var totalAssignedClasses = await _uow.ClassInstructorRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Include(ci => ci.Class)
                .Where(ci => ci.InstructorId == instructorId && 
                            ci.Class != null)
                .Select(ci => ci.ClassId)
                .Distinct()
                .CountAsync();

            // Total Materials Created: Count learning materials authored by this instructor
            var totalMaterialsCreated = await _uow.MaterialAuthorRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(ma => ma.InstructorId == instructorId)
                .CountAsync();

            // Total Quizzes Created: Count quizzes authored by this instructor
            var totalQuizzesCreated = await _uow.QuizAuthorRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(qa => qa.InstructorId == instructorId)
                .CountAsync();

            return new InstructorSummaryDto
            {
                TotalAssignedTrainees = totalAssignedTrainees,
                TotalAssignedClasses = totalAssignedClasses,
                TotalMaterialsCreated = totalMaterialsCreated,
                TotalQuizzesCreated = totalQuizzesCreated
            };
        }

        #endregion

        #region Part 2: Charts & Analytics

        public async Task<IEnumerable<ClassTraineeCountDto>> GetTopClassesByTraineeCountAsync(int instructorId, int topCount = 5)
        {
            // Validate instructor exists
            var instructorExists = await _uow.InstructorRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .AnyAsync(i => i.Id == instructorId && i.IsDeleted == false);

            if (!instructorExists)
                throw new KeyNotFoundException($"Instructor with ID {instructorId} not found or has been deleted.");

            // Get classes assigned to the instructor, order by number of Enrollments (descending), take top N
            var topClasses = await _uow.ClassInstructorRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(ci => ci.InstructorId == instructorId)
                .Include(ci => ci.Class)
                    .ThenInclude(c => c.ClassCode)
                .Include(ci => ci.Class)
                    .ThenInclude(c => c.Enrollments)
                .Where(ci => ci.Class != null)
                .Select(ci => new
                {
                    ClassId = ci.ClassId,
                    ClassName = ci.Class.Name,
                    ClassCode = ci.Class.ClassCode != null ? ci.Class.ClassCode.Name : "N/A",
                    // Count enrollments that are not deleted
                    TraineeCount = ci.Class.Enrollments.Count(e => e.IsDeleted == false)
                })
                .OrderByDescending(x => x.TraineeCount)
                .Take(topCount)
                .ToListAsync();

            return topClasses.Select(x => new ClassTraineeCountDto
            {
                ClassName = x.ClassName,
                ClassCode = x.ClassCode,
                TraineeCount = x.TraineeCount
            });
        }

        public async Task<IEnumerable<ClassAverageScoreDto>> GetAverageScorePerClassAsync(int instructorId)
        {
            // Validate instructor exists
            var instructorExists = await _uow.InstructorRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .AnyAsync(i => i.Id == instructorId && i.IsDeleted == false);

            if (!instructorExists)
                throw new KeyNotFoundException($"Instructor with ID {instructorId} not found or has been deleted.");

            // For each class assigned to the instructor, calculate the average FinalScore
            // Path: ClassInstructor -> Class -> Enrollment -> LearningProgress
            var classesWithScores = await _uow.ClassInstructorRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(ci => ci.InstructorId == instructorId)
                .Include(ci => ci.Class)
                    .ThenInclude(c => c.ClassCode)
                .Include(ci => ci.Class)
                    .ThenInclude(c => c.Enrollments)
                        .ThenInclude(e => e.LearningProgresses)
                .Where(ci => ci.Class != null)
                .Select(ci => new
                {
                    ClassId = ci.ClassId,
                    ClassName = ci.Class.Name,
                    ClassCode = ci.Class.ClassCode != null ? ci.Class.ClassCode.Name : "N/A",
                    Enrollments = ci.Class.Enrollments
                        .Where(e => e.IsDeleted == false)
                        .ToList()
                })
                .ToListAsync();

            // Calculate average scores for each class
            var result = classesWithScores.Select(c =>
            {
                // Get all final scores from learning progresses
                var scores = c.Enrollments
                    .SelectMany(e => e.LearningProgresses)
                    .Where(lp => lp.FinalScore.HasValue && lp.FinalScore.Value > 0)
                    .Select(lp => lp.FinalScore!.Value)
                    .ToList();

                // Calculate average if there are any scores
                decimal? averageScore = scores.Any() ? scores.Average() : null;

                return new ClassAverageScoreDto
                {
                    ClassName = c.ClassName,
                    ClassCode = c.ClassCode,
                    AverageScore = averageScore
                };
            }).ToList();

            return result;
        }

        #endregion
    }
}
