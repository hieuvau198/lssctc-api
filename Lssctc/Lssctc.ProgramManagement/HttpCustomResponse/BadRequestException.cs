

namespace Lssctc.ProgramManagement.HttpCustomResponse
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message) { }
    }
}
