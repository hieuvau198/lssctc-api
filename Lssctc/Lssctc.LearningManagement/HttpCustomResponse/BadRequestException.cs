namespace Lssctc.LearningManagement.HttpCustomResponse
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message) { }
    }
}
