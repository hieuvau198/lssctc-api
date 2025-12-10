namespace Lssctc.ProgramManagement.Materials.Dtos
{
    public class TraineeMaterialResponseDto
    {
        public IEnumerable<ActivityMaterialDto> Materials { get; set; }
        public TraineeSessionStatusDto SessionStatus { get; set; }
    }

    public class TraineeSessionStatusDto
    {
        public bool IsOpen { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Message { get; set; }
    }
}
