using System;
using System.Collections.Generic;

namespace Lssctc.ProgramManagement.Syllabuses.Dtos
{
    // Syllabus DTOs
    public class SyllabusDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public string CourseCode { get; set; } = null!;
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
        public List<SyllabusSectionDto> SyllabusSections { get; set; } = new List<SyllabusSectionDto>();
    }

    public class CreateSyllabusDto
    {
        public string Name { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public string CourseCode { get; set; } = null!;
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }

    public class UpdateSyllabusDto
    {
        public string? Name { get; set; }
        public string? CourseName { get; set; }
        public string? CourseCode { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }

    // Syllabus Section DTOs
    public class SyllabusSectionDto
    {
        public int Id { get; set; }
        public int SyllabusId { get; set; }
        public string SectionTitle { get; set; } = null!;
        public string? SectionDescription { get; set; }
        public int SectionOrder { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
    }

    public class CreateSyllabusSectionDto
    {
        public int SyllabusId { get; set; }
        public string SectionTitle { get; set; } = null!;
        public string? SectionDescription { get; set; }
        public int SectionOrder { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
    }

    public class UpdateSyllabusSectionDto
    {
        public string? SectionTitle { get; set; }
        public string? SectionDescription { get; set; }
        public int SectionOrder { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
    }
}