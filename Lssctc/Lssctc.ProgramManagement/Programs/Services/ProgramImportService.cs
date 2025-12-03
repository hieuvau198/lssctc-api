using ExcelDataReader;
using Lssctc.ProgramManagement.Programs.Dtos;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Lssctc.ProgramManagement.Programs.Services
{
    public class ProgramImportService : IProgramImportService
    {
        private readonly IUnitOfWork _uow;

        public ProgramImportService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<ProgramDto> ImportProgramFromExcelAsync(IFormFile file)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            if (file == null || file.Length == 0) throw new ArgumentException("No file uploaded.");

            using var stream = file.OpenReadStream();
            using var reader = ExcelReaderFactory.CreateReader(stream);

            var result = reader.AsDataSet(new ExcelDataSetConfiguration()
            {
                ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
            });

            if (result.Tables.Count == 0 || result.Tables[0].Rows.Count == 0)
                throw new ArgumentException("The uploaded Excel file is empty.");

            var dataTable = result.Tables[0];
            var firstRow = dataTable.Rows[0];

            // 1. Create Program
            string programName = firstRow[0]?.ToString()?.Trim();
            if (string.IsNullOrEmpty(programName)) throw new ArgumentException("Program Name is missing.");

            var trainingProgram = new TrainingProgram
            {
                Name = programName,
                Description = firstRow[1]?.ToString()?.Trim(),
                IsDeleted = false,
                IsActive = true,
                ImageUrl = null
            };

            await _uow.ProgramRepository.CreateAsync(trainingProgram);
            await _uow.SaveChangesAsync(); // Save to get ID

            // 2. Process Courses & Sections
            // Map: CourseName -> (CourseEntity, List<Section>, CategoryName, LevelName)
            var courseMap = new Dictionary<string, (Course Course, List<Section> Sections, string Cat, string Lvl)>();
            var courseOrder = new List<string>();

            foreach (DataRow row in dataTable.Rows)
            {
                // Columns:
                // 0:ProgName, 1:ProgDesc
                // 2:CourseName, 3:CourseDesc, 4:Category, 5:Level, 6:Price, 7:ImgUrl
                // 8:SecTitle, 9:SecDesc, 10:Duration

                string cName = row[2]?.ToString()?.Trim();
                if (string.IsNullOrEmpty(cName)) continue;

                if (!courseMap.ContainsKey(cName))
                {
                    string cDesc = row[3]?.ToString()?.Trim();
                    string catName = row[4]?.ToString()?.Trim();
                    string lvlName = row[5]?.ToString()?.Trim();
                    string priceStr = row[6]?.ToString()?.Trim();
                    string imgUrl = row[7]?.ToString()?.Trim();

                    // Defaults
                    decimal.TryParse(priceStr, out decimal price);
                    if (string.IsNullOrEmpty(catName)) catName = "General";
                    if (string.IsNullOrEmpty(lvlName)) lvlName = "Beginner";

                    var newCourse = new Course
                    {
                        Name = cName,
                        Description = cDesc,
                        Price = price,
                        ImageUrl = imgUrl, // Save URL
                        IsActive = true,   // Always true
                        IsDeleted = false,
                        DurationHours = 0
                    };
                    courseMap[cName] = (newCourse, new List<Section>(), catName, lvlName);
                    courseOrder.Add(cName);
                }

                // Section Data
                string sTitle = row[8]?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(sTitle))
                {
                    string sDesc = row[9]?.ToString()?.Trim();
                    string durStr = row[10]?.ToString()?.Trim();
                    int.TryParse(durStr, out int duration);

                    var newSection = new Section
                    {
                        SectionTitle = sTitle,
                        SectionDescription = sDesc,
                        EstimatedDurationMinutes = duration > 0 ? duration : 0,
                        IsDeleted = false
                    };
                    courseMap[cName].Sections.Add(newSection);
                }
            }

            // 3. Save Data
            int progDuration = 0;
            int progCount = 0;

            for (int i = 0; i < courseOrder.Count; i++)
            {
                var cName = courseOrder[i];
                var data = courseMap[cName];
                var course = data.Course;
                var sections = data.Sections;

                // A. Handle Category (Get or Create)
                var category = await GetOrCreateCategoryAsync(data.Cat);
                course.CategoryId = category.Id;

                // B. Handle Level (Get or Create)
                var level = await GetOrCreateLevelAsync(data.Lvl);
                course.LevelId = level.Id;

                // C. Duration
                int totalMin = sections.Sum(s => s.EstimatedDurationMinutes ?? 0);
                course.DurationHours = (int)Math.Ceiling(totalMin / 60.0);
                progDuration += course.DurationHours ?? 0;

                // D. Save Course
                await _uow.CourseRepository.CreateAsync(course);
                await _uow.SaveChangesAsync();

                // E. Link to Program
                await _uow.ProgramCourseRepository.CreateAsync(new ProgramCourse
                {
                    ProgramId = trainingProgram.Id,
                    CourseId = course.Id,
                    CourseOrder = i + 1,
                    Name = course.Name,
                    Description = course.Description
                });

                // F. Save Sections & Links
                for (int j = 0; j < sections.Count; j++)
                {
                    await _uow.SectionRepository.CreateAsync(sections[j]);
                    await _uow.SaveChangesAsync();

                    await _uow.CourseSectionRepository.CreateAsync(new CourseSection
                    {
                        CourseId = course.Id,
                        SectionId = sections[j].Id,
                        SectionOrder = j + 1
                    });
                }
                progCount++;
            }

            // 4. Update Program Stats
            trainingProgram.DurationHours = progDuration;
            trainingProgram.TotalCourses = progCount;
            await _uow.ProgramRepository.UpdateAsync(trainingProgram);
            await _uow.SaveChangesAsync();

            return new ProgramDto { Id = trainingProgram.Id, Name = trainingProgram.Name, TotalCourses = progCount };
        }

        // --- Helper Methods ---
        private async Task<CourseCategory> GetOrCreateCategoryAsync(string name)
        {
            var cat = await _uow.CourseCategoryRepository.GetAllAsQueryable()
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());

            if (cat != null) return cat;

            cat = new CourseCategory { Name = name, Description = $"Auto-created for {name}" };
            await _uow.CourseCategoryRepository.CreateAsync(cat);
            await _uow.SaveChangesAsync();
            return cat;
        }

        private async Task<CourseLevel> GetOrCreateLevelAsync(string name)
        {
            var lvl = await _uow.CourseLevelRepository.GetAllAsQueryable()
                .FirstOrDefaultAsync(l => l.Name.ToLower() == name.ToLower());

            if (lvl != null) return lvl;

            lvl = new CourseLevel { Name = name, Description = $"Auto-created level {name}"};
            await _uow.CourseLevelRepository.CreateAsync(lvl);
            await _uow.SaveChangesAsync();
            return lvl;
        }

        // --- DELETE REVERT LOGIC ---
        public async Task DeleteImportedProgramAsync(int programId)
        {
            var program = await _uow.ProgramRepository.GetAllAsQueryable()
                .Include(p => p.ProgramCourses)
                    .ThenInclude(pc => pc.Course)
                        .ThenInclude(c => c.CourseSections)
                            .ThenInclude(cs => cs.Section)
                .FirstOrDefaultAsync(p => p.Id == programId);

            if (program == null) throw new KeyNotFoundException("Program not found.");

            // Gather IDs to check/delete
            var coursesToDelete = new List<Course>();
            var sectionsToDelete = new List<Section>();

            foreach (var pc in program.ProgramCourses)
            {
                var course = pc.Course;
                if (course == null) continue;

                // Check if Course is used by ANY OTHER Program
                bool isSharedCourse = await _uow.ProgramCourseRepository.GetAllAsQueryable()
                    .AnyAsync(x => x.CourseId == course.Id && x.ProgramId != programId);

                if (!isSharedCourse)
                {
                    coursesToDelete.Add(course);

                    // Check Sections
                    foreach (var cs in course.CourseSections)
                    {
                        var section = cs.Section;
                        if (section == null) continue;

                        // Check if Section is used by ANY OTHER Course
                        bool isSharedSection = await _uow.CourseSectionRepository.GetAllAsQueryable()
                            .AnyAsync(x => x.SectionId == section.Id && x.CourseId != course.Id);

                        if (!isSharedSection)
                        {
                            sectionsToDelete.Add(section);
                        }
                    }
                }
            }

            // Execute Deletes (Bottom-Up)

            // 1. Delete Links first (Handled by Cascading usually, but manual for safety)
            // Note: EF Core might need explicit removal if Cascade not set in DB
            // Removing Program will remove ProgramCourses.
            // We need to remove CourseSections for the courses we are deleting.

            // 2. Delete Sections
            foreach (var s in sectionsToDelete.Distinct())
            {
                // Remove all CourseSection links for this section (should be only current course)
                var links = await _uow.CourseSectionRepository.GetAllAsQueryable().Where(x => x.SectionId == s.Id).ToListAsync();
                foreach (var l in links) await _uow.CourseSectionRepository.DeleteAsync(l);

                await _uow.SectionRepository.DeleteAsync(s);
            }

            // 3. Delete Courses
            foreach (var c in coursesToDelete.Distinct())
            {
                var links = await _uow.ProgramCourseRepository.GetAllAsQueryable().Where(x => x.CourseId == c.Id).ToListAsync();
                foreach (var l in links) await _uow.ProgramCourseRepository.DeleteAsync(l);

                await _uow.CourseRepository.DeleteAsync(c);
            }

            // 4. Delete Program
            await _uow.ProgramRepository.DeleteAsync(program);

            await _uow.SaveChangesAsync();
        }
    }
}