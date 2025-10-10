using AutoMapper;
using AutoMapper.QueryableExtensions;
using Lssctc.ProgramManagement.Quizzes.DTOs;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.Quizzes.Services
{
    public class QuizService : IQuizService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public QuizService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

      

        public async Task<QuizDto?> GetQuizById(int id)
        {
            var entity = await _uow.QuizRepository.GetByIdAsync(id); 
            return entity == null ? null : _mapper.Map<QuizDto>(entity);
        }

        public async Task<PagedResult<QuizDetailDto>> GetDetailQuizzes(int pageIndex, int pageSize)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize <= 0 || pageSize > 200) pageSize = 20;

            var query = _uow.QuizRepository.GetAllAsQueryable();

            var total = await query.CountAsync();

            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<QuizDetailDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<QuizDetailDto>
            {
                Items = items,
                TotalCount = total,
                Page = pageIndex,
                PageSize = pageSize
            };
        }

        public async Task<QuizDetailDto?> GetQuizDetail(int quizId, CancellationToken ct = default)
        {
            var quizDetail = await _uow.QuizRepository.GetAllAsQueryable()
            .Where(q => q.Id == quizId)
            .ProjectTo<QuizDetailDto>(_mapper.ConfigurationProvider)
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

            if (quizDetail == null)
            {
                return null;
                throw new KeyNotFoundException($"Quiz {quizId} not found.");
            }

            return quizDetail;
        }

        public async Task<QuizDetailDto?> GetQuizDetailNoAnswer(int quizId, CancellationToken ct = default)
        {
            var quizDetail = await _uow.QuizRepository.GetAllAsQueryable()
            .Where(q => q.Id == quizId)
            .ProjectTo<QuizDetailDto>(_mapper.ConfigurationProvider)
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

            if (quizDetail == null)
            {
                return null;
                throw new KeyNotFoundException($"Quiz {quizId} not found.");
            }

            foreach (var question in quizDetail.Questions)
            {
                foreach (var option in question.Options)
                {
                    option.IsCorrect = false;
                }
            }

            return quizDetail;
        }

        public async Task<QuizTraineeDetailDto?> GetQuizDetailForTrainee(int quizId, CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();

            var quizDetail = await _uow.QuizRepository.GetAllAsQueryable()
       .Where(q => q.Id == quizId)
       .ProjectTo<QuizTraineeDetailDto>(_mapper.ConfigurationProvider)
       .AsNoTracking()
       .FirstOrDefaultAsync(ct);

            if (quizDetail == null)
            {
                return null;
                throw new KeyNotFoundException($"Quiz {quizId} not found.");
            }

            return quizDetail;
        }


        public async Task<int> CreateQuiz(CreateQuizDto dto)
        {
            var entity = _mapper.Map<Quiz>(dto);
            await _uow.QuizRepository.CreateAsync(entity);           
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateQuizById(int id, UpdateQuizDto dto)
        {
            var entity = await _uow.QuizRepository.GetByIdAsync(id);  
            if (entity == null) return false;

            _mapper.Map(dto, entity);
            await _uow.QuizRepository.UpdateAsync(entity);            
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteQuizById(int id)
        {
            var entity = await _uow.QuizRepository.GetByIdAsync(id);  
            if (entity == null) return false;

            await _uow.QuizRepository.DeleteAsync(entity);            
            await _uow.SaveChangesAsync();
            return true;
        }        
        
        



        public async Task<int> CreateQuestionByQuizId(int quizId, CreateQuizQuestionDto dto)
        {
            if (dto == null) throw new ValidationException("Body is required.");

            var quiz = await _uow.QuizRepository.GetByIdAsync(quizId);
            if (quiz is null) throw new KeyNotFoundException($"Quiz {quizId} not found.");

            var rawName = (dto.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(rawName))
                throw new ValidationException("Name is required.");

            if (rawName.Length > 500)
                throw new ValidationException("Name must be at most 500 characters.");

            // Chuẩn hoá khoảng trắng (gộp nhiều khoảng trắng thành 1)
            var normalizedName = string.Join(" ", rawName.Split(' ', StringSplitOptions.RemoveEmptyEntries));

            //  Tên câu hỏi phải duy nhất trong 1 quiz (so sánh không phân biệt hoa thường)
            var nameExists = await _uow.QuizQuestionRepository.ExistsAsync(x =>
                x.QuizId == quizId &&
                x.Name != null &&
                x.Name.ToLower() == normalizedName.ToLower());

            if (nameExists)
                throw new ValidationException("A question with the same name already exists in this quiz.");

            //  Validate Description 
            if (dto.Description != null && dto.Description.Length > 2000)
                throw new ValidationException("Description must be at most 2000 characters.");

            //  Validate QuestionScore (>= 0, làm tròn 2 chữ số)
            decimal? score = dto.QuestionScore;
            if (score.HasValue)
            {
                if (score.Value < 0m)
                    throw new ValidationException("QuestionScore must be greater than or equal to 0.");

                score = Math.Round(score.Value, 2, MidpointRounding.AwayFromZero);

                //  Không vượt tổng điểm quiz (nếu quiz.TotalScore có giá trị)
                if (quiz.TotalScore.HasValue)
                {
                    var used = await _uow.QuizQuestionRepository.GetAllAsQueryable()
                        .Where(q => q.QuizId == quizId && q.QuestionScore != null)
                        .SumAsync(q => q.QuestionScore) ?? 0m;

                    var willBe = used + score.Value;
                    if (willBe > quiz.TotalScore.Value + 0.0001m)
                        throw new ValidationException(
                            $"Total question scores ({willBe}) would exceed quiz.TotalScore ({quiz.TotalScore.Value}).");
                }
            }


            //================== Option ==============

            // Create question
            var entity = _mapper.Map<QuizQuestion>(dto);
            entity.QuizId = quizId;
            entity.Name = normalizedName;
            entity.QuestionScore = score;

         

            await _uow.QuizQuestionRepository.CreateAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> CreateOption(int questionId, CreateQuizQuestionOptionDto dto)
        {
            //  Kiểm tra question tồn tại
            var question = await _uow.QuizQuestionRepository.GetByIdAsync(questionId);
            if (question == null)
                throw new KeyNotFoundException($"Question with ID {questionId} not found.");

            //  Tự động gán DisplayOrder nếu không truyền
            var displayOrder = dto.DisplayOrder ?? await GetNextDisplayOrder();

            //  Kiểm tra trùng DisplayOrder trên toàn bảng
            var isDuplicate = await _uow.QuizQuestionOptionRepository
                .ExistsAsync(x => x.DisplayOrder == displayOrder);

            if (isDuplicate)
            {
                throw new ValidationException($"DisplayOrder {displayOrder} already exists globally in the system.");
            }
            // validate score
            decimal? optionScore = dto.OptionScore;
            if (optionScore.HasValue)
            {
                //Miền giá trị + làm tròn 2 chữ số
                if (optionScore.Value < 0m || optionScore.Value > 999.99m)
                    throw new ValidationException("OptionScore must be between 0 and 999.99.");

                optionScore = Math.Round(optionScore.Value, 2, MidpointRounding.AwayFromZero);

                // Không vượt tổng điểm câu hỏi (nếu câu hỏi có đặt QuestionScore)
                if (question.QuestionScore.HasValue)
                {
                    var used = await _uow.QuizQuestionOptionRepository.GetAllAsQueryable()
                        .Where(o => o.QuizQuestionId == questionId && o.OptionScore != null)
                        .SumAsync(o => o.OptionScore) ?? 0m;

                    var willBe = used + optionScore.Value;
                    if (willBe > question.QuestionScore.Value + 0.0001m)
                        throw new ValidationException(
                            $"Total option scores ({willBe}) would exceed question score ({question.QuestionScore.Value}).");
                }
            }

            //  Tạo mới option
            var option = new QuizQuestionOption
            {
                QuizQuestionId = questionId,
                Name = dto.Name,
                Description = dto.Description,
                IsCorrect = dto.IsCorrect,
                DisplayOrder = displayOrder,
                OptionScore = dto.OptionScore
            };

            //  Lưu
            await _uow.QuizQuestionOptionRepository.CreateAsync(option);
            await _uow.SaveChangesAsync();

            return option.Id;
        }


        // Lấy max DisplayOrder trên toàn bảng và tăng lên 1
        private async Task<int> GetNextDisplayOrder()
        {
            var maxOrder = await _uow.QuizQuestionOptionRepository
                .GetAllAsQueryable()
                .Select(x => x.DisplayOrder)
                .MaxAsync();

            return (maxOrder ?? 0) + 1;
        }







        /// <summary>
        /// Lấy Option theo quizId + questionId + optionId
        /// </summary>
        public async Task<QuizQuestionOptionDto?> GetOptionById(int optionId)
        {
            
            // Trả về DTO (không trừ offset)
            var dto = await _uow.QuizQuestionOptionRepository
                .GetAllAsQueryable()
                .Where(o => o.Id == optionId)
                .Select(o => new QuizQuestionOptionDto
                {
                    Id = o.Id,
                    QuizQuestionId = o.QuizQuestionId,
                    Name = o.Name,
                    Description = o.Description,
                    IsCorrect = o.IsCorrect,
                    OptionScore = o.OptionScore,
                    DisplayOrder = o.DisplayOrder
                })
                .FirstOrDefaultAsync();

            return dto;
        }


        

        //=== get options by questionId
        public async Task<IReadOnlyList<QuizDetailQuestionOptionDto>> GetOptionsByQuestionId(
    int questionId, CancellationToken ct = default)
        {
            if (questionId <= 0) throw new ValidationException("questionId must be > 0.");

            // Đảm bảo question tồn tại
            var exists = await _uow.QuizQuestionRepository.ExistsAsync(q => q.Id == questionId);
            if (!exists) throw new KeyNotFoundException($"Question {questionId} not found.");

            // Lấy toàn bộ options của câu hỏi, sắp theo DisplayOrder (null sẽ đẩy về cuối)
            var items = await _uow.QuizQuestionOptionRepository.GetAllAsQueryable()
                .Where(o => o.QuizQuestionId == questionId)
                .OrderBy(o => o.DisplayOrder) // nếu DisplayOrder có thể null: .OrderBy(o => o.DisplayOrder ?? int.MaxValue)
                .ProjectTo<QuizDetailQuestionOptionDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync(ct);

            if (items.Count == 0)
                throw new ValidationException($"Question {questionId} has no options.");


            return items;
        }



        //==== section quiz ======
        public async Task<QuizTraineeDetailDto?> GetQuizTraineeDetailBySectionQuizIdAsync(
    int sectionQuizId, CancellationToken ct = default)
        {
            return await _uow.SectionQuizRepository.GetAllAsQueryable()
                .Where(sq => sq.Id == sectionQuizId)
                .Select(sq => sq.Quiz)
                .ProjectTo<QuizTraineeDetailDto>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .FirstOrDefaultAsync(ct);
        }



        //==== create question and options by quiz id
   

public async Task<int> CreateQuestionWithOptionsByQuizId(
    int quizId, CreateQuizQuestionWithOptionsDto dto)
    {
        if (dto == null) throw new ValidationException("Body is required.");

        // --- Validate Quiz + Question ---
        var quiz = await _uow.QuizRepository.GetByIdAsync(quizId);
        if (quiz is null) throw new KeyNotFoundException($"Quiz {quizId} not found.");

        var rawName = (dto.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(rawName))
            throw new ValidationException("Name is required.");
        var qName = string.Join(" ", rawName.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        if (qName.Length > 500)
            throw new ValidationException("Name must be at most 500 characters.");

        var dupName = await _uow.QuizQuestionRepository.ExistsAsync(q =>
            q.QuizId == quizId && q.Name != null && q.Name.ToLower() == qName.ToLower());
        if (dupName)
            throw new ValidationException("A question with the same name already exists in this quiz.");

        if (dto.Description != null && dto.Description.Length > 2000)
            throw new ValidationException("Description must be at most 2000 characters.");

        decimal? questionScore = dto.QuestionScore;
        if (questionScore.HasValue)
        {
            if (questionScore.Value < 0m)
                throw new ValidationException("QuestionScore must be >= 0.");
            questionScore = Math.Round(questionScore.Value, 2, MidpointRounding.AwayFromZero);

            if (quiz.TotalScore.HasValue)
            {
                var used = await _uow.QuizQuestionRepository.GetAllAsQueryable()
                    .Where(q => q.QuizId == quizId && q.QuestionScore != null)
                    .SumAsync(q => q.QuestionScore) ?? 0m;

                var willBe = used + questionScore.Value;
                if (willBe > quiz.TotalScore.Value + 0.0001m)
                    throw new ValidationException(
                        $"Total question scores ({willBe}) would exceed quiz.TotalScore ({quiz.TotalScore.Value}).");
            }
        }

        // --- Validate Options ---
        if (dto.Options == null || dto.Options.Count < 2)
            throw new ValidationException("At least 2 options are required.");

        var correctCount = dto.Options.Count(o => o.IsCorrect);
        if (correctCount == 0)
            throw new ValidationException("At least one correct option is required.");
        if (!dto.IsMultipleAnswers && correctCount != 1)
            throw new ValidationException("Exactly 1 correct option is required when IsMultipleAnswers = false.");

        decimal totalOptionScore = 0m;
        var normalizedOptions = new List<(string Name, string? Desc, bool IsCorrect, decimal? Score)>();
        for (int i = 0; i < dto.Options.Count; i++)
        {
            var o = dto.Options[i];
            var rawOptName = (o.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(rawOptName))
                throw new ValidationException($"Option[{i}].Name is required.");
            var optName = string.Join(" ", rawOptName.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (optName.Length > 500)
                throw new ValidationException($"Option[{i}].Name must be at most 500 characters.");
            if (o.Description != null && o.Description.Length > 2000)
                throw new ValidationException($"Option[{i}].Description must be at most 2000 characters.");

            if (o.OptionScore.HasValue)
            {
                var v = o.OptionScore.Value;
                if (v < 0m || v > 999.99m)
                    throw new ValidationException($"Option[{i}].OptionScore must be between 0 and 999.99.");
                totalOptionScore += Math.Round(v, 2, MidpointRounding.AwayFromZero);
            }

            normalizedOptions.Add((optName, o.Description, o.IsCorrect, o.OptionScore));
        }

        if (questionScore.HasValue && totalOptionScore > questionScore.Value + 0.0001m)
            throw new ValidationException(
                $"Sum of option scores ({totalOptionScore}) exceeds QuestionScore ({questionScore.Value}).");

        // --- Create Question trước để có Id ---
        var question = new QuizQuestion
        {
            QuizId = quizId,
            Name = qName,
            Description = dto.Description,
            QuestionScore = questionScore,
            IsMultipleAnswers = dto.IsMultipleAnswers
        };
        await _uow.QuizQuestionRepository.CreateAsync(question);
        await _uow.SaveChangesAsync(); // cần question.Id

        // --- Chuẩn bị Option entities (chưa set DisplayOrder) ---
        var optionEntities = normalizedOptions.Select(o => new QuizQuestionOption
        {
            QuizQuestionId = question.Id,
            Name = o.Name,
            Description = o.Desc,
            IsCorrect = o.IsCorrect,
            OptionScore = o.Score
            // DisplayOrder sẽ set ngay trước khi Save
        }).ToList();

        // --- Optimistic retry: per-question trước, fail thì fallback to global ---
        const int maxRetries = 2;
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            // 1) Tính MAX theo câu hỏi
            var maxPerQuestion = await _uow.QuizQuestionOptionRepository.GetAllAsQueryable()
                .Where(o => o.QuizQuestionId == question.Id)
                .Select(o => o.DisplayOrder)
                .MaxAsync() ?? 0;

            // 2) Gán order 1..n dựa trên per-question
            int next = maxPerQuestion + 1;
            foreach (var e in optionEntities)
                e.DisplayOrder = next++;

            try
            {
                // Add (nếu chưa add) — nếu đã add ở vòng trước, EF vẫn giữ state Added; cứ Save là đủ
                foreach (var e in optionEntities)
                    if (e.Id == 0 && e.DisplayOrder > 0) // heuristic nhẹ
                        await _uow.QuizQuestionOptionRepository.CreateAsync(e);

                await _uow.SaveChangesAsync();
                return question.Id; // OK
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex) && attempt < maxRetries)
            {
                // Fallback: DB có UNIQUE display_order toàn bảng → gán theo GLOBAL MAX và thử lại
                var maxGlobal = await _uow.QuizQuestionOptionRepository.GetAllAsQueryable()
                    .Select(o => o.DisplayOrder)
                    .MaxAsync() ?? 0;

                int gnext = maxGlobal + 1;
                foreach (var e in optionEntities)
                    e.DisplayOrder = gnext++;

                try
                {
                    await _uow.SaveChangesAsync();
                    return question.Id;
                }
                catch (DbUpdateException ex2) when (IsUniqueViolation(ex2) && attempt < maxRetries)
                {
                    // Có thể có tiến trình khác vừa chen; lặp lại vòng for để tính lại MAX mới nhất
                    continue;
                }
            }
        }

        throw new ValidationException("Could not assign DisplayOrder due to concurrent inserts. Please retry.");

        static bool IsUniqueViolation(DbUpdateException ex)
            => ex.InnerException is SqlException sql && (sql.Number == 2627 || sql.Number == 2601);
    }

}
}
