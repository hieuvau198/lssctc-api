namespace Lssctc.ProgramManagement.ClassManage.SectionRecords.Dtos
{
    public class TraineeSectionDto
    {
        /// <summary>
        /// The ID of the specific trainee's SectionRecord.
        /// Will be 0 if no record exists (e.g., scaffolding failed).
        /// </summary>
        public int SectionRecordId { get; set; }

        /// <summary>
        /// The ID of the template Section.
        /// </summary>
        public int SectionId { get; set; }

        /// <summary>
        /// The title of the section.
        /// </summary>
        public string? SectionTitle { get; set; }

        /// <summary>
        /// The description of the section.
        /// </summary>
        public string? SectionDescription { get; set; }

        /// <summary>
        /// The display order of the section within the course.
        /// </summary>
        public int SectionOrder { get; set; }

        /// <summary>
        /// The estimated duration of the section in minutes.
        /// </summary>
        public int? EstimatedDurationMinutes { get; set; }

        /// <summary>
        /// The trainee's completion status for this section.
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// The trainee's progress percentage for this section.
        /// </summary>
        public decimal? Progress { get; set; }
    }
}
