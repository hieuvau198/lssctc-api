using ExcelDataReader;
using Lssctc.ProgramManagement.Programs.Dtos;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using System.Data;

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
            // Ensure encoding for ExcelDataReader is registered (required for .NET Core)
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            if (file == null || file.Length == 0)
                throw new ArgumentException("No file uploaded.");

            using var stream = file.OpenReadStream();
            using var reader = ExcelReaderFactory.CreateReader(stream);

            // Read as DataSet (assuming first row is HEADER)
            var result = reader.AsDataSet(new ExcelDataSetConfiguration()
            {
                ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                {
                    UseHeaderRow = true
                }
            });

            if (result.Tables.Count == 0 || result.Tables[0].Rows.Count == 0)
                throw new ArgumentException("The uploaded Excel file is empty.");

            var dataTable = result.Tables[0];

            // --- 1. Extract Program Details (from the first data row) ---
            var firstRow = dataTable.Rows[0];
            string programName = firstRow[0]?.ToString()?.Trim();
            string programDesc = firstRow[1]?.ToString()?.Trim();

            if (string.IsNullOrEmpty(programName))
                throw new ArgumentException("Program Name is missing (Column 1).");

            var trainingProgram = new TrainingProgram
            {
                Name = programName,
                Description = programDesc,
                IsDeleted = false,
                IsActive = true,
                ImageUrl = null // Can be set later
            };

            // Save Program first to get ID
            await _uow.ProgramRepository.CreateAsync(trainingProgram);
            await _uow.SaveChangesAsync();

            // --- 2. Parse Courses and Sections ---
            // Structure: List of objects to hold data before saving
            // Use a Dictionary to group sections by Course Name to ensure uniqueness
            var courseMap = new Dictionary<string, (Course CourseEntity, List<Section> Sections)>();
            var courseOrderList = new List<string>(); // To maintain the order of courses as they appear in Excel

            foreach (DataRow row in dataTable.Rows)
            {
                // Columns:
                // 0: Program Name (Ignored after first row)
                // 1: Program Desc (Ignored after first row)
                // 2: Course Name
                // 3: Course Description
                // 4: Section Title
                // 5: Section Description
                // 6: Duration (Minutes)

                string courseName = row[2]?.ToString()?.Trim();
                string courseDesc = row[3]?.ToString()?.Trim();
                string sectionTitle = row[4]?.ToString()?.Trim();
                string sectionDesc = row[5]?.ToString()?.Trim();
                string durationStr = row[6]?.ToString()?.Trim();

                if (string.IsNullOrEmpty(courseName)) continue; // Skip rows without a course

                // Check if we've already seen this course
                if (!courseMap.ContainsKey(courseName))
                {
                    var newCourse = new Course
                    {
                        Name = courseName,
                        Description = courseDesc,
                        IsActive = true,
                        IsDeleted = false,
                        // Defaults
                        DurationHours = 0
                    };
                    courseMap[courseName] = (newCourse, new List<Section>());
                    courseOrderList.Add(courseName);
                }

                // If this row has section data, add it to the course
                if (!string.IsNullOrEmpty(sectionTitle))
                {
                    int.TryParse(durationStr, out int duration);
                    var newSection = new Section
                    {
                        SectionTitle = sectionTitle,
                        SectionDescription = sectionDesc,
                        EstimatedDurationMinutes = duration > 0 ? duration : 0,
                        IsDeleted = false
                    };
                    courseMap[courseName].Sections.Add(newSection);
                }
            }

            // --- 3. Save Courses and Links ---
            int programTotalDurationHours = 0;
            int programTotalCourses = 0;

            for (int i = 0; i < courseOrderList.Count; i++)
            {
                var courseName = courseOrderList[i];
                var data = courseMap[courseName];
                var courseEntity = data.CourseEntity;
                var sections = data.Sections;

                // Calculate Course Duration
                int totalMinutes = sections.Sum(s => s.EstimatedDurationMinutes ?? 0);
                courseEntity.DurationHours = (int)Math.Ceiling(totalMinutes / 60.0);
                programTotalDurationHours += courseEntity.DurationHours ?? 0;

                // Save Course
                await _uow.CourseRepository.CreateAsync(courseEntity);
                await _uow.SaveChangesAsync(); // Need ID for links

                // Link Course to Program
                var programCourse = new ProgramCourse
                {
                    ProgramId = trainingProgram.Id,
                    CourseId = courseEntity.Id,
                    CourseOrder = i + 1,
                    Name = courseEntity.Name, // Snapshot name
                    Description = courseEntity.Description
                };
                await _uow.ProgramCourseRepository.CreateAsync(programCourse);

                // --- 4. Save Sections and Links ---
                for (int j = 0; j < sections.Count; j++)
                {
                    var sectionEntity = sections[j];

                    // Save Section
                    await _uow.SectionRepository.CreateAsync(sectionEntity);
                    await _uow.SaveChangesAsync(); // Need ID

                    // Link Section to Course
                    var courseSection = new CourseSection
                    {
                        CourseId = courseEntity.Id,
                        SectionId = sectionEntity.Id,
                        SectionOrder = j + 1
                    };
                    await _uow.CourseSectionRepository.CreateAsync(courseSection);
                }

                programTotalCourses++;
            }

            // --- 5. Update Program Totals ---
            trainingProgram.DurationHours = programTotalDurationHours;
            trainingProgram.TotalCourses = programTotalCourses;

            await _uow.ProgramRepository.UpdateAsync(trainingProgram);
            await _uow.SaveChangesAsync();

            // Return basic DTO
            return new ProgramDto
            {
                Id = trainingProgram.Id,
                Name = trainingProgram.Name,
                Description = trainingProgram.Description,
                DurationHours = trainingProgram.DurationHours,
                TotalCourses = trainingProgram.TotalCourses
            };
        }
    }
}
