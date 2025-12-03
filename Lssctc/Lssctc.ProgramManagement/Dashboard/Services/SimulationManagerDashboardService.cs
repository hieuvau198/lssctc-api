using Lssctc.ProgramManagement.Dashboard.Dtos;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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

            // Total Simulation Sessions: Count all practice attempts (simulation sessions)
            var totalSimulationSessions = await _uow.PracticeAttemptRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .CountAsync(pa => pa.IsDeleted != true);

            return new SimulationManagerSummaryDto
            {
                TotalTrainees = totalTrainees,
                TotalPractices = totalPractices,
                TotalTasks = totalTasks,
                TotalSimulationSessions = totalSimulationSessions
            };
        }

        #endregion

        #region Part 2: Charts & Analytics

        public async Task<IEnumerable<MonthlyPracticeCompletionDto>> GetPracticeCompletionDistributionAsync(int simulationManagerId, int year)
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
                .Select(pa => new
                {
                    Month = pa.AttemptDate.Month,
                    IsCompleted = pa.IsPass == true
                })
                .ToListAsync();

            // Group by month
            var monthlyData = attemptsInYear
                .GroupBy(pa => pa.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    CompletedCount = g.Count(pa => pa.IsCompleted),
                    NotCompletedCount = g.Count(pa => !pa.IsCompleted)
                })
                .ToDictionary(x => x.Month);

            // Create result for all 12 months (fill with 0 if no data)
            var result = Enumerable.Range(1, 12)
                .Select(month => new MonthlyPracticeCompletionDto
                {
                    Month = month,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                    CompletedCount = monthlyData.ContainsKey(month) ? monthlyData[month].CompletedCount : 0,
                    NotCompletedCount = monthlyData.ContainsKey(month) ? monthlyData[month].NotCompletedCount : 0
                })
                .ToList();

            return result;
        }

        public async Task<IEnumerable<PracticeDurationDistributionDto>> GetPracticeDurationDistributionAsync(int simulationManagerId)
        {
            // Validate simulation manager exists
            var simulationManagerExists = await _uow.SimulationManagerRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .AnyAsync(sm => sm.Id == simulationManagerId);

            if (!simulationManagerExists)
                throw new KeyNotFoundException($"Simulation Manager with ID {simulationManagerId} not found.");

            // Get all valid practice attempts with their associated practice's estimated duration
            var attempts = await _uow.PracticeAttemptRepository
                .GetAllAsQueryable()
                .AsNoTracking()
                .Where(pa => pa.IsCurrent == true && 
                            pa.PracticeId.HasValue &&
                            (pa.IsDeleted == null || pa.IsDeleted == false))
                .Join(
                    _uow.PracticeRepository.GetAllAsQueryable().AsNoTracking(),
                    pa => pa.PracticeId,
                    p => p.Id,
                    (pa, p) => new
                    {
                        EstimatedDuration = p.EstimatedDurationMinutes
                    })
                .ToListAsync();

            // Calculate duration in minutes for each attempt using estimated duration
            var durationsInMinutes = attempts
                .Where(a => a.EstimatedDuration.HasValue && a.EstimatedDuration.Value > 0)
                .Select(a => a.EstimatedDuration!.Value)
                .ToList();

            // Group into time ranges (buckets)
            var fastCount = durationsInMinutes.Count(d => d < 15);
            var moderateCount = durationsInMinutes.Count(d => d >= 15 && d <= 45);
            var slowCount = durationsInMinutes.Count(d => d > 45);

            var result = new List<PracticeDurationDistributionDto>
            {
                new PracticeDurationDistributionDto { DurationRange = "Fast (< 15 mins)", TraineeCount = fastCount },
                new PracticeDurationDistributionDto { DurationRange = "Moderate (15-45 mins)", TraineeCount = moderateCount },
                new PracticeDurationDistributionDto { DurationRange = "Slow (> 45 mins)", TraineeCount = slowCount }
            };

            return result;
        }

        #endregion
    }
}
