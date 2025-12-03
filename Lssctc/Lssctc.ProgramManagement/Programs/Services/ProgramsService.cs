using Lssctc.ProgramManagement.Programs.Dtos;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OfficeOpenXml;
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
        
        public async Task<object> ImportProgramFromExcelAsync(IFormFile file)
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;
            
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];
            
            if (worksheet == null || worksheet.Dimension == null)
                throw new ValidationException("Excel file is empty or invalid");

            var programs = new List<CreateProgramWithHierarchyDto>();
            var rowCount = worksheet.Dimension.End.Row;

            // Group rows by program
            var programGroups = new Dictionary<string, List<ExcelRowData>>();
            
            for (int row = 2; row <= rowCount; row++)
            {
                var rowData = ReadExcelRow(worksheet, row);
                
                if (string.IsNullOrWhiteSpace(rowData.ProgramName))
                    continue;

                if (!programGroups.ContainsKey(rowData.ProgramName))
                    programGroups[rowData.ProgramName] = new List<ExcelRowData>();
                
                programGroups[rowData.ProgramName].Add(rowData);
            }

            // Build DTO hierarchy
            foreach (var programGroup in programGroups)
            {
                var firstRow = programGroup.Value.First();
                var programDto = new CreateProgramWithHierarchyDto
                {
                    Name = firstRow.ProgramName,
                    Description = firstRow.ProgramDescription,
                    ImageUrl = firstRow.ProgramImageUrl,
                    Courses = new List<CreateCourseWithSectionsDto>()
                };

                // Group by course within this program
                var courseGroups = programGroup.Value
                    .Where(r => !string.IsNullOrWhiteSpace(r.CourseName))
                    .GroupBy(r => new { r.CourseName, r.CourseDescription, r.CategoryId, r.LevelId, r.DurationHours, r.Price, r.CourseImageUrl });

                foreach (var courseGroup in courseGroups)
                {
                    var courseDto = new CreateCourseWithSectionsDto
                    {
                        Name = courseGroup.Key.CourseName,
                        Description = courseGroup.Key.CourseDescription,
                        CategoryId = courseGroup.Key.CategoryId,
                        LevelId = courseGroup.Key.LevelId,
                        DurationHours = courseGroup.Key.DurationHours,
                        Price = courseGroup.Key.Price,
                        ImageUrl = courseGroup.Key.CourseImageUrl,
                        Sections = new List<CreateSectionForHierarchyDto>()
                    };

                    // Add sections for this course
                    foreach (var row in courseGroup)
                    {
                        if (!string.IsNullOrWhiteSpace(row.SectionTitle))
                        {
                            courseDto.Sections.Add(new CreateSectionForHierarchyDto
                            {
                                SectionTitle = row.SectionTitle,
                                SectionDescription = row.SectionDescription,
                                EstimatedDurationMinutes = row.DurationMinutes ?? 0
                            });
                        }
                    }

                    programDto.Courses.Add(courseDto);
                }

                programs.Add(programDto);
            }

            // Create all programs
            var createdProgramIds = new List<int>();
            var errors = new List<string>();

            foreach (var programDto in programs)
            {
                try
                {
                    var programId = await CreateProgramWithHierarchyAsync(programDto);
                    createdProgramIds.Add(programId);
                }
                catch (Exception ex)
                {
                    errors.Add($"Program '{programDto.Name}': {ex.Message}");
                }
            }

            return new
            {
                totalProgramsInFile = programs.Count,
                successfullyCreated = createdProgramIds.Count,
                failed = errors.Count,
                createdProgramIds,
                errors
            };
        }

        private ExcelRowData ReadExcelRow(ExcelWorksheet worksheet, int row)
        {
            return new ExcelRowData
            {
                ProgramName = worksheet.Cells[row, 1].Text?.Trim(),
                ProgramDescription = worksheet.Cells[row, 2].Text?.Trim(),
                ProgramImageUrl = worksheet.Cells[row, 3].Text?.Trim(),
                CourseName = worksheet.Cells[row, 4].Text?.Trim(),
                CourseDescription = worksheet.Cells[row, 5].Text?.Trim(),
                CategoryId = int.TryParse(worksheet.Cells[row, 6].Text, out var catId) ? catId : 0,
                LevelId = int.TryParse(worksheet.Cells[row, 7].Text, out var lvlId) ? lvlId : 0,
                DurationHours = int.TryParse(worksheet.Cells[row, 8].Text, out var durHrs) ? durHrs : 0,
                Price = decimal.TryParse(worksheet.Cells[row, 9].Text, out var price) ? price : (decimal?)null,
                CourseImageUrl = worksheet.Cells[row, 10].Text?.Trim(),
                SectionTitle = worksheet.Cells[row, 11].Text?.Trim(),
                SectionDescription = worksheet.Cells[row, 12].Text?.Trim(),
                DurationMinutes = int.TryParse(worksheet.Cells[row, 13].Text, out var durMin) ? durMin : (int?)null
            };
        }

        private class ExcelRowData
        {
            public string ProgramName { get; set; }
            public string ProgramDescription { get; set; }
            public string ProgramImageUrl { get; set; }
            public string CourseName { get; set; }
            public string CourseDescription { get; set; }
            public int CategoryId { get; set; }
            public int LevelId { get; set; }
            public int DurationHours { get; set; }
            public decimal? Price { get; set; }
            public string CourseImageUrl { get; set; }
            public string SectionTitle { get; set; }
            public string SectionDescription { get; set; }
            public int? DurationMinutes { get; set; }
        }

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

        #region System Data Cleanup

        /// <summary>
        /// Complete cleanup of a TrainingProgram and all its dependencies including Courses.
        /// This is a system cleanup feature that performs deep deletion in a transaction.
        /// </summary>
        public async Task CleanupProgramDataAsync(int id)
        {
            // Get DbContext for transaction management
            var dbContext = _uow.GetDbContext();
            IDbContextTransaction? transaction = null;

            try
            {
                // Begin transaction
                transaction = await dbContext.Database.BeginTransactionAsync();

                // 1. Retrieve the TrainingProgram (read-only to get IDs)
                var program = await _uow.ProgramRepository
                    .GetAllAsQueryable()
                    .Where(p => p.Id == id)
                    .AsNoTracking()
                    .Include(p => p.ProgramCourses)
                    .FirstOrDefaultAsync();

                if (program == null)
                {
                    throw new KeyNotFoundException($"Program with ID {id} not found.");
                }

                // 2. Collect IDs for cleanup
                var programCourseIds = program.ProgramCourses.Select(pc => pc.Id).ToList();
                var courseIds = program.ProgramCourses.Select(pc => pc.CourseId).Distinct().ToList();

                // 3. Delete TraineeCertificates linked to enrollments in this program
                var enrollmentIds = await _uow.EnrollmentRepository
                    .GetAllAsQueryable()
                    .Where(e => programCourseIds.Contains(e.Class.ProgramCourseId))
                    .Select(e => e.Id)
                    .ToListAsync();

                if (enrollmentIds.Any())
                {
                    // Delete trainee certificates
                    await _uow.TraineeCertificateRepository
                        .GetAllAsQueryable()
                        .Where(tc => enrollmentIds.Contains(tc.EnrollmentId))
                        .ExecuteDeleteAsync();

                    // Delete enrollments
                    await _uow.EnrollmentRepository
                        .GetAllAsQueryable()
                        .Where(e => enrollmentIds.Contains(e.Id))
                        .ExecuteDeleteAsync();
                }

                // 4. Delete ProgramCourse mappings
                await _uow.ProgramCourseRepository
                    .GetAllAsQueryable()
                    .Where(pc => programCourseIds.Contains(pc.Id))
                    .ExecuteDeleteAsync();

                // 5. Delete Courses and their children (Deep Delete)
                foreach (var courseId in courseIds)
                {
                    await DeleteCourseDeepAsync(courseId);
                }

                // 6. Finally, delete the Program itself
                await _uow.ProgramRepository
                    .GetAllAsQueryable()
                    .Where(p => p.Id == id)
                    .ExecuteDeleteAsync();

                // Commit transaction
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                // Rollback transaction on error
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                }
                throw;
            }
            finally
            {
                // Dispose transaction
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }

        /// <summary>
        /// Deep deletion of a Course including all its Sections, Activities, and related entities.
        /// </summary>
        private async Task DeleteCourseDeepAsync(int courseId)
        {
            // Get section IDs for this course
            var sectionIds = await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .Where(cs => cs.CourseId == courseId)
                .Select(cs => cs.SectionId)
                .Distinct()
                .ToListAsync();

            // Delete CourseSection mappings
            await _uow.CourseSectionRepository
                .GetAllAsQueryable()
                .Where(cs => cs.CourseId == courseId)
                .ExecuteDeleteAsync();

            // Delete Sections and their Activities
            foreach (var sectionId in sectionIds)
            {
                await DeleteSectionDeepAsync(sectionId);
            }

            // Delete the Course entity itself
            await _uow.CourseRepository
                .GetAllAsQueryable()
                .Where(c => c.Id == courseId)
                .ExecuteDeleteAsync();
        }

        /// <summary>
        /// Deep deletion of a Section including all its Activities.
        /// </summary>
        private async Task DeleteSectionDeepAsync(int sectionId)
        {
            // Get activity IDs for this section
            var activityIds = await _uow.SectionActivityRepository
                .GetAllAsQueryable()
                .Where(sa => sa.SectionId == sectionId)
                .Select(sa => sa.ActivityId)
                .Distinct()
                .ToListAsync();

            // Delete SectionActivity mappings
            await _uow.SectionActivityRepository
                .GetAllAsQueryable()
                .Where(sa => sa.SectionId == sectionId)
                .ExecuteDeleteAsync();

            // Delete Activities (including their related entities)
            // First delete ActivityMaterials, ActivityPractices, ActivityQuizzes
            if (activityIds.Any())
            {
                await _uow.ActivityMaterialRepository
                    .GetAllAsQueryable()
                    .Where(am => activityIds.Contains(am.ActivityId))
                    .ExecuteDeleteAsync();

                await _uow.ActivityPracticeRepository
                    .GetAllAsQueryable()
                    .Where(ap => activityIds.Contains(ap.ActivityId))
                    .ExecuteDeleteAsync();

                await _uow.ActivityQuizRepository
                    .GetAllAsQueryable()
                    .Where(aq => activityIds.Contains(aq.ActivityId))
                    .ExecuteDeleteAsync();

                // Delete Activities themselves
                await _uow.ActivityRepository
                    .GetAllAsQueryable()
                    .Where(a => activityIds.Contains(a.Id))
                    .ExecuteDeleteAsync();
            }

            // Delete Section
            await _uow.SectionRepository
                .GetAllAsQueryable()
                .Where(s => s.Id == sectionId)
                .ExecuteDeleteAsync();
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
