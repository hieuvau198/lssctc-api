using Lssctc.ProgramManagement.Programs.Dtos;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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

        /// <summary>
        /// Creates a complete TrainingProgram with its Courses and Sections in a single transactional operation.
        /// Follows the same pattern as CreateQuizWithQuestions.
        /// </summary>
        public async Task<int> CreateProgramWithHierarchyAsync(CreateProgramWithHierarchyDto dto)
        {
            if (dto == null) throw new ValidationException("Body is required.");

            try
            {
                // === VALIDATION PHASE ===
                
                // Validate courses
                if (dto.Courses == null || dto.Courses.Count == 0)
                    throw new ValidationException("At least one course is required.");

                if (dto.Courses.Count > 50)
                    throw new ValidationException("A program cannot contain more than 50 courses.");

                int courseIndex = 0;
                foreach (var courseDto in dto.Courses)
                {
                    courseIndex++;

                    // Validate course name
                    if (string.IsNullOrWhiteSpace(courseDto.Name))
                        throw new ValidationException($"Course #{courseIndex}: Name cannot be empty.");

                    var normalizedCourseName = string.Join(' ', courseDto.Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
                    
                    if (normalizedCourseName.Length < 3 || normalizedCourseName.Length > 200)
                        throw new ValidationException($"Course #{courseIndex}: Name must be between 3 and 200 characters (actual: {normalizedCourseName.Length} chars).");

                    // Validate category exists
                    var category = await _uow.CourseCategoryRepository.GetByIdAsync(courseDto.CategoryId);
                    if (category == null)
                        throw new ValidationException($"Course #{courseIndex} ('{TruncateText(normalizedCourseName, 50)}'): Category with ID {courseDto.CategoryId} not found.");

                    // Validate level exists
                    var level = await _uow.CourseLevelRepository.GetByIdAsync(courseDto.LevelId);
                    if (level == null)
                        throw new ValidationException($"Course #{courseIndex} ('{TruncateText(normalizedCourseName, 50)}'): Level with ID {courseDto.LevelId} not found.");

                    // Validate sections only if provided
                    if (courseDto.Sections != null && courseDto.Sections.Count > 0)
                    {
                        if (courseDto.Sections.Count > 100)
                            throw new ValidationException($"Course #{courseIndex} ('{TruncateText(normalizedCourseName, 50)}'): Cannot have more than 100 sections.");

                        int sectionIndex = 0;
                        foreach (var sectionDto in courseDto.Sections)
                        {
                            sectionIndex++;

                            if (string.IsNullOrWhiteSpace(sectionDto.SectionTitle))
                                throw new ValidationException($"Course #{courseIndex}, Section #{sectionIndex}: Title cannot be empty.");

                            var normalizedSectionTitle = string.Join(' ', sectionDto.SectionTitle.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
                            
                            if (normalizedSectionTitle.Length < 3 || normalizedSectionTitle.Length > 200)
                                throw new ValidationException($"Course #{courseIndex}, Section #{sectionIndex}: Title must be between 3 and 200 characters (actual: {normalizedSectionTitle.Length} chars).");
                        }
                    }
                }

                // === CREATION PHASE ===

                // Step 1: Create the TrainingProgram
                var normalizedProgramName = string.Join(' ', dto.Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
                
                var program = new TrainingProgram
                {
                    Name = normalizedProgramName,
                    Description = dto.Description,
                    ImageUrl = dto.ImageUrl,
                    IsActive = true,
                    TotalCourses = 0,
                    DurationHours = 0,
                    IsDeleted = false
                };

                await _uow.ProgramRepository.CreateAsync(program);
                await _uow.SaveChangesAsync();
                var programId = program.Id;

                // Step 2: Create Courses and their Sections
                int courseOrder = 0;
                courseIndex = 0;
                
                foreach (var courseDto in dto.Courses)
                {
                    courseIndex++;
                    courseOrder++;

                    try
                    {
                        // Create Course
                        var normalizedCourseName = string.Join(' ', courseDto.Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
                        
                        var course = new Course
                        {
                            Name = normalizedCourseName,
                            Description = courseDto.Description,
                            CategoryId = courseDto.CategoryId,
                            LevelId = courseDto.LevelId,
                            Price = courseDto.Price,
                            DurationHours = courseDto.DurationHours,
                            ImageUrl = courseDto.ImageUrl ?? "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS2sUcEWdSaINXf8E4hmy7obh3B1w0-l_T8Tw&s",
                            IsActive = true,
                            IsDeleted = false
                        };

                        await _uow.CourseRepository.CreateAsync(course);
                        await _uow.SaveChangesAsync();
                        var courseId = course.Id;

                        // Create ProgramCourse link
                        var programCourse = new ProgramCourse
                        {
                            ProgramId = programId,
                            CourseId = courseId,
                            CourseOrder = courseOrder,
                            Name = normalizedCourseName,
                            Description = courseDto.Description
                        };

                        await _uow.ProgramCourseRepository.CreateAsync(programCourse);
                        await _uow.SaveChangesAsync();

                        // Step 3: Create Sections for this Course (only if sections are provided)
                        if (courseDto.Sections != null && courseDto.Sections.Count > 0)
                        {
                            int sectionOrder = 0;
                            int sectionIndex = 0;

                            foreach (var sectionDto in courseDto.Sections)
                            {
                                sectionIndex++;
                                sectionOrder++;

                                try
                                {
                                    var normalizedSectionTitle = string.Join(' ', sectionDto.SectionTitle.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
                                    
                                    // Create Section
                                    var section = new Section
                                    {
                                        SectionTitle = normalizedSectionTitle,
                                        SectionDescription = string.IsNullOrWhiteSpace(sectionDto.SectionDescription) 
                                            ? null 
                                            : string.Join(' ', sectionDto.SectionDescription.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)),
                                        EstimatedDurationMinutes = sectionDto.EstimatedDurationMinutes,
                                        IsDeleted = false
                                    };

                                    await _uow.SectionRepository.CreateAsync(section);
                                    await _uow.SaveChangesAsync();

                                    // Create CourseSection link
                                    var courseSection = new CourseSection
                                    {
                                        CourseId = courseId,
                                        SectionId = section.Id,
                                        SectionOrder = sectionOrder
                                    };

                                    await _uow.CourseSectionRepository.CreateAsync(courseSection);
                                    await _uow.SaveChangesAsync();
                                }
                                catch (Exception ex) when (ex is not ValidationException)
                                {
                                    throw new ValidationException($"Course #{courseIndex}, Section #{sectionIndex}: {ex.Message}", ex);
                                }
                            }
                        }
                    }
                    catch (Exception ex) when (ex is not ValidationException)
                    {
                        throw new ValidationException($"Course #{courseIndex} ('{TruncateText(courseDto.Name, 50)}'): {ex.Message}", ex);
                    }
                }

                return programId;
            }
            catch (DbUpdateException dbEx)
            {
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                throw new ValidationException($"Database error while creating program: {innerMessage}. Please check that all field mappings are correct.", dbEx);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ValidationException($"Unexpected error while creating program: {ex.Message}", ex);
            }
        }

        #endregion

        #region Private Helper Methods

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text.Length <= maxLength ? text : text.Substring(0, maxLength) + "...";
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
