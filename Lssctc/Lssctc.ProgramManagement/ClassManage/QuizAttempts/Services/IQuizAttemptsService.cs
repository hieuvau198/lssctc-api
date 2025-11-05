namespace Lssctc.ProgramManagement.ClassManage.QuizAttempts.Services
{
    public interface IQuizAttemptsService
    {
        // Logic: lasted submitted attempt is the new valid one, using IsCurrent field to mark it   
        // Logic: when submit new attempt, mark all previous attempts IsCurrent = false, and new one IsCurrent = true

        // Get all quiz attempts by trainee id, activity record id
        // Get latest quiz attempt by trainee id, activity record id
        // Get all quiz (one lastest for each trainee inside that class) attempts by activity record id

        // submit quiz attempt by trainee id, activity record id
    }
}
