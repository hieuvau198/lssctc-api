using Lssctc.Share.Interfaces;

namespace Lssctc.ProgramManagement.ClassManage.Helpers
{
    public class ProgressHelper
    {
        private readonly IUnitOfWork _uow;
        public ProgressHelper(IUnitOfWork uow)
        {
            _uow = uow;
        }
        #region BR


        // each trainee can have multiple attempts for each practice, or each quiz
        // Logic: when submit new attempt, mark all previous attempts IsCurrent = false, and new one IsCurrent = true
        // each time when calculate progress, only consider attempts with IsCurrent = true
        // 

        #endregion
    }
}
