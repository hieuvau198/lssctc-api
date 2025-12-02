namespace Lssctc.ProgramManagement.Dashboard.Dtos
{
    public class ActiveTraineeDto
    {
        public string TraineeName { get; set; } = null!;
        public string TraineeCode { get; set; } = null!;
        public int EnrolledCourseCount { get; set; }
    }
}
