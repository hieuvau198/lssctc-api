using Lssctc.ProgramManagement.Dashboard.Dtos;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.Dashboard.Services
{
    public class SimulationManagerDashboardService : ISimulationManagerDashboardService
    {
        private readonly IUnitOfWork _uow;

        public SimulationManagerDashboardService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        #region Part 1: Overview (Summary Cards)

        public async Task<SimulationManagerSummaryDto> GetManagerSummaryAsync(int simulationManagerId)
        {
            // Validate simulation manager exists
            var simulationManagerExists = await _uow.SimulationManagerRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .AnyAsync(sm => sm.Id == simulationManagerId);

            if (!simulationManagerExists)
                throw new KeyNotFoundException($"Simulation Manager with ID {simulationManagerId} not found.");

            // Total Trainees: Count all non-deleted trainees
            var totalTrainees = await _uow.TraineeRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .CountAsync(t => t.IsDeleted == false);

            // Total Practices: Count all non-deleted practices
            var totalPractices = await _uow.PracticeRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .CountAsync(p => p.IsDeleted == false);

            // Total Tasks: Count all non-deleted simulation tasks
            var totalTasks = await _uow.SimTaskRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .CountAsync(st => st.IsDeleted == false);

            // Total Active Classes With Simulations:
            // Count distinct classes that are:
            // 1. Status is Open (1) or InProgress (2)
            // 2. Linked to at least one non-deleted Practice via the section structure
            // Path: Class -> ProgramCourse -> Course -> CourseSection -> Section -> SectionActivity -> Activity -> ActivityPractice -> Practice
            var totalActiveClassesWithSimulations = await _uow.ClassRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(c => c.Status == 1 || c.Status == 2) // Open or InProgress
                .Include(c => c.ProgramCourse)
                    .ThenInclude(pc => pc.Course)
                        .ThenInclude(course => course.CourseSections)
                            .ThenInclude(cs => cs.Section)
                                .ThenInclude(s => s.SectionActivities)
                                    .ThenInclude(sa => sa.Activity)
                                        .ThenInclude(a => a.ActivityPractices)
                                            .ThenInclude(ap => ap.Practice)
                .Where(c => c.ProgramCourse.Course.CourseSections
                    .Any(cs => cs.Section.SectionActivities
                        .Any(sa => sa.Activity.ActivityPractices
                            .Any(ap => ap.Practice.IsDeleted == false))))
                .Select(c => c.Id)
                .Distinct()
                .CountAsync();

            return new SimulationManagerSummaryDto
            {
                TotalTrainees = totalTrainees,
                TotalPractices = totalPractices,
                TotalTasks = totalTasks,
                TotalActiveClassesWithSimulations = totalActiveClassesWithSimulations
            };
        }

        #endregion

        #region Part 2: Charts & Analytics

        public async Task<IEnumerable<PracticeCompletionDistributionDto>> GetPracticeCompletionDistributionAsync(int simulationManagerId, int year)
        {
            // Validate simulation manager exists
            var simulationManagerExists = await _uow.SimulationManagerRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .AnyAsync(sm => sm.Id == simulationManagerId);

            if (!simulationManagerExists)
                throw new KeyNotFoundException($"Simulation Manager with ID {simulationManagerId} not found.");

            // Get all current practice attempts for the specified year
            var attemptsInYear = await _uow.PracticeAttemptRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(pa => pa.IsCurrent == true && 
                            pa.AttemptDate.Year == year &&
                            (pa.IsDeleted == null || pa.IsDeleted == false))
                .ToListAsync();

            // Group by completion status
            var completedCount = attemptsInYear.Count(pa => pa.IsPass == true);
            var notCompletedCount = attemptsInYear.Count(pa => pa.IsPass == false || pa.IsPass == null);

            // Always return both statuses, even if count is zero
            var result = new List<PracticeCompletionDistributionDto>
            {
                new PracticeCompletionDistributionDto
                {
                    CompletionStatus = "Completed",
                    TraineeCount = completedCount
                },
                new PracticeCompletionDistributionDto
                {
                    CompletionStatus = "NotCompleted",
                    TraineeCount = notCompletedCount
                }
            };

            return result;
        }

        public async Task<IEnumerable<PracticeAverageScoreDto>> GetAverageScorePerPracticeAsync(int simulationManagerId)
        {
            // Validate simulation manager exists
            var simulationManagerExists = await _uow.SimulationManagerRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .AnyAsync(sm => sm.Id == simulationManagerId);

            if (!simulationManagerExists)
                throw new KeyNotFoundException($"Simulation Manager with ID {simulationManagerId} not found.");

            // Get all practices that have at least one current attempt
            var practicesWithAttempts = await _uow.PracticeRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(p => p.IsDeleted == false)
                .Select(p => new
                {
                    PracticeId = p.Id,
                    PracticeName = p.PracticeName,
                    PracticeCode = p.PracticeCode,
                    EstimatedDurationMinutes = p.EstimatedDurationMinutes,
                    CurrentAttempts = p.ActivityPractices
                        .SelectMany(ap => ap.Activity.SectionActivities)
                        .SelectMany(sa => sa.Section.CourseSections)
                        .SelectMany(cs => cs.Course.ProgramCourses)
                        .SelectMany(pc => pc.Classes)
                        .SelectMany(c => c.Enrollments)
                        .SelectMany(e => e.LearningProgresses)
                        .SelectMany(lp => lp.SectionRecords)
                        .SelectMany(sr => sr.ActivityRecords)
                        .SelectMany(ar => ar.PracticeAttempts)
                        .Where(pa => pa.IsCurrent == true && 
                                    pa.PracticeId == p.Id &&
                                    (pa.IsDeleted == null || pa.IsDeleted == false))
                        .ToList()
                })
                .Where(x => x.CurrentAttempts.Any())
                .ToListAsync();

            // Calculate average score for each practice
            var result = practicesWithAttempts.Select(p =>
            {
                var scores = p.CurrentAttempts
                    .Where(pa => pa.Score.HasValue)
                    .Select(pa => pa.Score!.Value)
                    .ToList();

                decimal? averageScore = scores.Any() ? scores.Average() : null;

                return new PracticeAverageScoreDto
                {
                    PracticeName = p.PracticeName ?? "Unknown",
                    PracticeCode = p.PracticeCode ?? "N/A",
                    AverageScore = averageScore,
                    TotalAttempts = p.CurrentAttempts.Count,
                    EstimatedDurationMinutes = p.EstimatedDurationMinutes
                };
            }).ToList();

            return result;
        }

        #endregion
    }
}
