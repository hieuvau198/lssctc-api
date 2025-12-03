using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Programs.Dtos
{
    /// <summary>
    /// DTO to create a complete TrainingProgram with its Courses and Sections in a single request.
    /// 
    /// IMPORTANT NOTES:
    /// 1. At least 1 course is required
    /// 2. Each course can optionally have sections (not required)
    /// 3. CourseOrder will be auto-generated based on position in list (1, 2, 3, ...)
    /// 4. SectionOrder will be auto-generated based on position in list (1, 2, 3, ...)
    /// 5. All operations are transactional - if any part fails, the entire operation is rolled back
    /// 
    /// EXAMPLE JSON:
    /// {
    ///   "name": "Mobile Crane Training Program",
    ///   "description": "Complete training program for mobile crane operation",
    ///   "imageUrl": "https://example.com/image.jpg",
    ///   "courses": [
    ///     {
    ///       "name": "Basic Crane Operations",
    ///       "description": "Introduction to crane operations",
    ///       "categoryId": 1,
    ///       "levelId": 1,
    ///       "durationHours": 40,
    ///       "price": 5000000,
    ///       "imageUrl": "https://example.com/course1.jpg",
    ///       "sections": [
    ///         {
    ///           "sectionTitle": "Safety Procedures",
    ///           "sectionDescription": "Safety guidelines and procedures",
    ///           "estimatedDurationMinutes": 120
    ///         },
    ///         {
    ///           "sectionTitle": "Equipment Overview",
    ///           "sectionDescription": "Understanding crane components",
    ///           "estimatedDurationMinutes": 180
    ///         }
    ///       ]
    ///     },
    ///     {
    ///       "name": "Course Without Sections",
    ///       "description": "This course has no sections yet",
    ///       "categoryId": 1,
    ///       "levelId": 1,
    ///       "durationHours": 20,
    ///       "sections": []
    ///     }
    ///   ]
    /// }
    /// </summary>
    public class CreateProgramWithHierarchyDto
    {
        [Required(ErrorMessage = "Program name is required.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Program name must be between 3 and 200 characters.")]
        public string Name { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        [Url(ErrorMessage = "Image URL must be a valid URL.")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "At least one course is required.")]
        [MinLength(1, ErrorMessage = "At least 1 course is required.")]
        [MaxLength(50, ErrorMessage = "Maximum 50 courses allowed per program.")]
        public List<CreateCourseWithSectionsDto> Courses { get; set; } = new List<CreateCourseWithSectionsDto>();
    }

    /// <summary>
    /// DTO to create a Course with its Sections.
    /// Used as part of CreateProgramWithHierarchyDto.
    /// Sections are now optional - course can be created without any sections.
    /// </summary>
    public class CreateCourseWithSectionsDto
    {
        [Required(ErrorMessage = "Course name is required.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Course name must be between 3 and 200 characters.")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Description is optional for courses in hierarchical creation.
        /// </summary>
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Category ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Category ID must be a positive number.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Level ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Level ID must be a positive number.")]
        public int LevelId { get; set; }

        [Range(0, 1000000000, ErrorMessage = "Price must be between 0 and 1,000,000,000. Default currency is VND")]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Duration in hours is required.")]
        [Range(1, 500, ErrorMessage = "Duration must be between 1 and 500 hours.")]
        public int DurationHours { get; set; }

        [Url(ErrorMessage = "Image URL must be a valid URL.")]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Optional list of sections for this course.
        /// Can be empty or null - course will be created without sections.
        /// </summary>
        [MaxLength(100, ErrorMessage = "Maximum 100 sections allowed per course.")]
        public List<CreateSectionForHierarchyDto>? Sections { get; set; } = new List<CreateSectionForHierarchyDto>();
    }

    /// <summary>
    /// DTO to create a Section as part of the hierarchy.
    /// Simplified version of CreateSectionDto for nested creation.
    /// </summary>
    public class CreateSectionForHierarchyDto
    {
        [Required(ErrorMessage = "Section title is required.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Section title must be between 3 and 200 characters.")]
        public string SectionTitle { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "Section description cannot exceed 1000 characters.")]
        public string? SectionDescription { get; set; }

        [Required(ErrorMessage = "Estimated duration is required.")]
        [Range(1, 1000, ErrorMessage = "Estimated duration must be between 1 and 1000 minutes.")]
        public int EstimatedDurationMinutes { get; set; }
    }
}
