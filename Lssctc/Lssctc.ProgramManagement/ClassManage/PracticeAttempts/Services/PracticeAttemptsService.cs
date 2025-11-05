using Lssctc.Share.Interfaces;

namespace Lssctc.ProgramManagement.ClassManage.PracticeAttempts.Services
{
    public class PracticeAttemptsService : IPracticeAttemptsService
    {
        private readonly IUnitOfWork _uow;
        public PracticeAttemptsService(IUnitOfWork uow)
        {
            _uow = uow;
        }

    }
}
