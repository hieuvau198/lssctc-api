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
                .ToListAsync();

            return programs.Select(MapToDto);
        }
        
        public async Task<PagedResult<ProgramDto>> GetProgramsAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _uow.ProgramRepository
                .GetAllAsQueryable()
                .Where(p => p.IsDeleted != true)
                .Select(p => MapToDto(p));

            var pagedResult = await query.ToPagedResultAsync(pageNumber, pageSize);

            return pagedResult;
        }

        public async Task<ProgramDto?> GetProgramByIdAsync(int id)
        {
            var program = await _uow.ProgramRepository.GetByIdAsync(id);

            if (program == null || program.IsDeleted == true)
            {
                return null;
            }

            return MapToDto(program);
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
            return MapToDto(existingProgram);
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
                .Any(pc => pc.Classes != null);
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

        private ProgramDto MapToDto(TrainingProgram program)
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

        #endregion
    }
}
