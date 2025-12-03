using Lssctc.ProgramManagement.Programs.Dtos;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.Programs.Services
{
    public class ProgramsService : IProgramsService
    {
        private readonly IUnitOfWork _uow;
        public ProgramsService(IUnitOfWork uow)
        {
            _uow = uow;
        }
        #region Programs
        public async Task<IEnumerable<ProgramDto>> GetAllProgramsAsync()
        {
            var programs = await _uow.ProgramRepository
                .GetAllAsQueryable()
                .Where(p => p.IsDeleted != true)
                .Include(p => p.ProgramCourses)
                    .ThenInclude(pc => pc.Course)
                .ToListAsync();

            return programs.Select(MapToDtoWithCalculations);
        }
        
        public async Task<PagedResult<ProgramDto>> GetProgramsAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            // CHANGED: Perform projection directly in query with Include
            var query = _uow.ProgramRepository
                .GetAllAsQueryable()
                .Where(p => p.IsDeleted != true)
                .Include(p => p.ProgramCourses)
                    .ThenInclude(pc => pc.Course)
                .Select(p => new ProgramDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    IsActive = p.IsActive,
                    ImageUrl = p.ImageUrl,
                    // Calculate TotalCourses: count the number of ProgramCourses
                    TotalCourses = p.ProgramCourses.Count(),
                    // Calculate DurationHours: sum of DurationHours from related Courses
                    DurationHours = p.ProgramCourses
                        .Where(pc => pc.Course != null && pc.Course.DurationHours.HasValue)
                        .Sum(pc => pc.Course.DurationHours!.Value)
                });

            var pagedResult = await query.ToPagedResultAsync(pageNumber, pageSize);

            return pagedResult;
        }

        public async Task<ProgramDto?> GetProgramByIdAsync(int id)
        {
            var program = await _uow.ProgramRepository
                .GetAllAsQueryable()
                .Where(p => p.Id == id && p.IsDeleted != true)
                .Include(p => p.ProgramCourses)
                    .ThenInclude(pc => pc.Course)
                .FirstOrDefaultAsync();

            if (program == null)
            {
                return null;
            }

            return MapToDtoWithCalculations(program);
        }

        public async Task<ProgramDto> CreateProgramAsync(CreateProgramDto createDto)
        {
            var program = new TrainingProgram
            {
                Name = createDto.Name!,
                Description = createDto.Description,
                ImageUrl = createDto.ImageUrl,
                IsActive = true,
                TotalCourses = 0,
                DurationHours = 0,
                IsDeleted = false
            };

            var createdProgram = await _uow.ProgramRepository.CreateAsync(program);
            await _uow.SaveChangesAsync();

            return MapToDto(createdProgram);
        }

        public async Task<ProgramDto> UpdateProgramAsync(int id, UpdateProgramDto updateDto)
        {
            var existingProgram = await _uow.ProgramRepository.GetByIdAsync(id);

            if (existingProgram == null || existingProgram.IsDeleted == true)
            {
                throw new Exception($"Program with ID {id} not found.");
            }

            if (updateDto.Name != null)
            {
                existingProgram.Name = updateDto.Name;
            }

            if (updateDto.Description != null)
            {
                existingProgram.Description = updateDto.Description;
            }

            if (updateDto.ImageUrl != null)
            {
                existingProgram.ImageUrl = updateDto.ImageUrl;
            }

            await _uow.ProgramRepository.UpdateAsync(existingProgram);
            await _uow.SaveChangesAsync();
            
            // Reload with ProgramCourses to recalculate
            var reloadedProgram = await _uow.ProgramRepository
                .GetAllAsQueryable()
                .Where(p => p.Id == id)
                .Include(p => p.ProgramCourses)
                    .ThenInclude(pc => pc.Course)
                .FirstOrDefaultAsync();
            
            return MapToDtoWithCalculations(reloadedProgram!);
        }

        public async Task DeleteProgramAsync(int id)
        {
            var program = await _uow.ProgramRepository
                .GetAllAsQueryable()
                .Where(program => program.Id == id)
                .Include(p => p.ProgramCourses)
                    .ThenInclude(pc => pc.Classes)
                .FirstOrDefaultAsync()
                ;

            if (program == null)
            {
                throw new Exception($"Program with ID {id} not found.");
            }

            if (program.IsDeleted == true)
            {
                return;
            }

            // check if any associated classes exist
            var hasAssociatedClasses = program.ProgramCourses
                .Any(pc => pc.Classes.Any());
            if (hasAssociatedClasses)
            {
                throw new Exception("Cannot delete program with associated classes.");
            }

            // delete its program courses
            
            foreach (var programCourse in program.ProgramCourses)
            {
                await _uow.ProgramCourseRepository.DeleteAsync(programCourse);
            }

            // soft delete program
            program.IsDeleted = true;
            await _uow.ProgramRepository.UpdateAsync(program);

            // save changes at last to ensure all operations are successful
            await _uow.SaveChangesAsync();
        }
        #endregion

        #region Private Mapping Methods

        // Old mapper - only retrieves values from entity (no calculations)
        private static ProgramDto MapToDto(TrainingProgram program)
        {
            return new ProgramDto
            {
                Id = program.Id,
                Name = program.Name,
                Description = program.Description,
                IsActive = program.IsActive,
                DurationHours = program.DurationHours,
                TotalCourses = program.TotalCourses,
                ImageUrl = program.ImageUrl
            };
        }

        // New mapper - calculates TotalCourses and DurationHours from ProgramCourses
        private static ProgramDto MapToDtoWithCalculations(TrainingProgram program)
        {
            // Calculate TotalCourses
            int totalCourses = program.ProgramCourses?.Count() ?? 0;

            // Calculate DurationHours: sum of DurationHours from related Courses
            int durationHours = program.ProgramCourses?
                .Where(pc => pc.Course != null && pc.Course.DurationHours.HasValue)
                .Sum(pc => pc.Course.DurationHours!.Value) ?? 0;

            return new ProgramDto
            {
                Id = program.Id,
                Name = program.Name,
                Description = program.Description,
                IsActive = program.IsActive,
                DurationHours = durationHours,
                TotalCourses = totalCourses,
                ImageUrl = program.ImageUrl
            };
        }

        #endregion
    }
}
