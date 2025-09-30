using Lssctc.ProgramManagement.Quizzes.DTOs;
using Lssctc.Share.Common;

namespace Lssctc.ProgramManagement.Quizzes.Services
{
    public interface IQuizService
    {
        Task<PagedResult<QuizDetailDto>> GetDetailQuizzes(int pageIndex, int pageSize);
        Task<QuizDto?> GetQuizById(int id);
        Task<int> CreateQuiz(CreateQuizDto dto);
        Task<bool> UpdateQuizById(int id, UpdateQuizDto dto);
        Task<bool> DeleteQuizById(int id);
       
        Task<QuizDetailDto?> GetQuizDetail(int quizId, CancellationToken ct = default);
        Task<QuizTraineeDetailDto?> GetQuizDetailForTrainee(
    int quizId, CancellationToken ct = default);

        Task<int> CreateQuestionByQuizId(int quizId, CreateQuizQuestionDto dto);
        
        Task<int> CreateOption(int questionId, CreateQuizQuestionOptionDto dto);
        
        Task<IReadOnlyList<QuizDetailQuestionOptionDto>> GetOptionsByQuestionId(
        int questionId, CancellationToken ct = default);
        Task<QuizQuestionOptionDto?> GetOptionById(int optionId);

        // get quizzes by Section Quiz id
        Task<QuizTraineeDetailDto?> GetQuizTraineeDetailBySectionQuizIdAsync(
     int sectionQuizId, CancellationToken ct = default);

        //create question and option question by quiz id
        Task<int> CreateQuestionWithOptionsByQuizId(
    int quizId, CreateQuizQuestionWithOptionsDto dto);


    }
}
