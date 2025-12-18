using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.ClassManage.FinalExams.Dtos
{
    public class UpdateClassWeightsDto
    {
        [Required]
        [Range(double.Epsilon, 0.999999, ErrorMessage = "Theory weight must be greater than 0 and less than 1.")]
        public decimal TheoryWeight { get; set; }

        [Required]
        [Range(double.Epsilon, 0.999999, ErrorMessage = "Simulation weight must be greater than 0 and less than 1.")]
        public decimal SimulationWeight { get; set; }

        [Required]
        [Range(double.Epsilon, 0.999999, ErrorMessage = "Practical weight must be greater than 0 and less than 1.")]
        public decimal PracticalWeight { get; set; }
    }
}
