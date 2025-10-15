using Lssctc.LearningManagement.QuizQuestions.DTOs;
using Lssctc.LearningManagement.Quizzes.DTOs;
using Lssctc.Share.Common;

namespace Lssctc.LearningManagement.Quizzes.Services
{
    public interface IQuizService
    {
     
        Task<QuizDto?> GetQuizById(int id);
        Task<int> CreateQuiz(CreateQuizDto dto);
        Task<bool> UpdateQuizById(int id, UpdateQuizDto dto);
        Task<bool> DeleteQuizById(int id);

        Task<PagedResult<QuizQuestionNoOptionsDto>> GetQuestionsByQuizIdPaged(int quizId, int page, int pageSize);
        Task<QuizTraineeDetailDto?> GetQuizDetailForTrainee(
    int quizId, CancellationToken ct = default);

        Task<int> CreateQuestionByQuizId(int quizId, CreateQuizQuestionDto dto);
        
        Task<int> CreateOption(int questionId, CreateQuizQuestionOptionDto dto);
        
        Task<IReadOnlyList<QuizDetailQuestionOptionDto>> GetOptionsByQuestionId(
        int questionId, CancellationToken ct = default);
        Task<QuizQuestionOptionDto?> GetOptionById(int optionId);

        //get all quizzes paged
        Task<PagedResult<QuizDto>> GetAllQuizzesPaged(int page, int pageSize);

        // get quizzes by Section Quiz id
        Task<QuizTraineeDetailDto?> GetQuizTraineeDetailBySectionQuizIdAsync(
     int sectionQuizId, CancellationToken ct = default);

        //create question and option question by quiz id
        Task<int> CreateQuestionWithOptionsByQuizId(
    int quizId, CreateQuizQuestionWithOptionsDto dto);


    }
}
