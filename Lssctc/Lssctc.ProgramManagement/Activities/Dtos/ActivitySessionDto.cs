using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Activities.Dtos
{
    /// <summary>
    /// DTO trả về thông tin Activity Session
    /// </summary>
    public class ActivitySessionDto
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public int ActivityId { get; set; }
        public string ActivityTitle { get; set; } = null!;
        public int ActivityType { get; set; }
        public bool IsActive { get; set; } // Trạng thái kích hoạt (true: truy cập được)
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }

    /// <summary>
    /// DTO tạo Activity Session thủ công (Task 3)
    /// </summary>
    public class CreateActivitySessionDto
    {
        [Required(ErrorMessage = "ClassId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ClassId must be greater than 0.")]
        public int ClassId { get; set; }

        [Required(ErrorMessage = "ActivityId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ActivityId must be greater than 0.")]
        public int ActivityId { get; set; }

        public bool IsActive { get; set; } = false;

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }
    }

    /// <summary>
    /// DTO cập nhật Activity Session (Task 4)
    /// </summary>
    public class UpdateActivitySessionDto
    {
        [Required(ErrorMessage = "IsActive status is required.")]
        public bool IsActive { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }
    }
}