using Lssctc.Share.Interfaces;

namespace Lssctc.ProgramManagement.ClassManage.QuizAttempts.Services
{
    public class QuizAttemptsService : IQuizAttemptsService
    {
        private readonly IUnitOfWork _uow;
        public QuizAttemptsService(IUnitOfWork uow)
        {
            _uow = uow;
        }
    }
}
