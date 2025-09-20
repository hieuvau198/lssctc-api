using AutoMapper;
using AutoMapper.QueryableExtensions;
using Lssctc.LearningManagement.Quizzes.DTOs;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Lssctc.LearningManagement.Quizzes.Services
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

        public async Task<(IReadOnlyList<QuizDetailDto> Items, int Total)> GetDetailQuizzes(
      int pageIndex, int pageSize, string? search)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize <= 0 || pageSize > 200) pageSize = 20;

            var query = _uow.QuizRepository.GetAllAsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(q =>
                    (q.Name != null && q.Name.Contains(s)) ||
                    (q.Description != null && q.Description.Contains(s)));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(q => q.UpdatedAt)
                .ThenByDescending(q => q.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<QuizDetailDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return (items, total);
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
            //  Validate question input data
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
                        .SumAsync(q => (decimal?)q.QuestionScore) ?? 0m;

                    var willBe = used + score.Value;
                    if (willBe > quiz.TotalScore.Value + 0.0001m)
                        throw new ValidationException(
                            $"Total question scores ({willBe}) would exceed quiz.TotalScore ({quiz.TotalScore.Value}).");
                }
            }

            // Create question
            var entity = _mapper.Map<QuizQuestion>(dto);
            entity.QuizId = quizId;
            entity.Name = normalizedName;
            entity.QuestionScore = score;

         

            await _uow.QuizQuestionRepository.CreateAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }


        /// <summary>
        /// Tạo Option theo quizId + questionId (validate đủ: tên/điểm/thứ tự/single-multi)
        /// </summary>
        public async Task<int> CreateOption(int quizId, int questionId, CreateQuizQuestionOptionDto dto)
        {
            if (dto == null) throw new ValidationException("Body is required.");

            // 1) Kiểm tra câu hỏi thuộc quiz
            var question = await _uow.QuizQuestionRepository
                .GetAllAsQueryable()
                .Where(q => q.Id == questionId && q.QuizId == quizId)
                .Select(q => new
                {
                    q.Id,
                    q.QuizId,
                    q.QuestionScore,
                    q.IsMultipleAnswers
                })
                .FirstOrDefaultAsync();

            if (question is null)
                throw new KeyNotFoundException($"Question {questionId} not found in Quiz {quizId}.");

            // 2) Chuẩn hóa & validate Name
            var raw = dto.Name?.Trim();
            if (string.IsNullOrWhiteSpace(raw))
                throw new ValidationException("Name is required.");

            var name = string.Join(" ", raw.Split(' ', System.StringSplitOptions.RemoveEmptyEntries));
            if (name.Length > 100)
                throw new ValidationException("Name must be at most 100 characters.");

            // Không trùng trong cùng câu hỏi (case-insensitive)
            var nameDup = await _uow.QuizQuestionOptionRepository.ExistsAsync(o =>
                o.QuizQuestionId == questionId &&
                o.Name != null &&
                o.Name.ToLower() == name.ToLower());
            if (nameDup)
                throw new ValidationException("Option name already exists in this question.");

            // 3) DisplayOrder: >=1, không trùng trong cùng câu hỏi
            if (dto.DisplayOrder < 1)
                throw new ValidationException("DisplayOrder must be >= 1.");

            var orderDup = await _uow.QuizQuestionOptionRepository.ExistsAsync(o =>
                o.QuizQuestionId == questionId && o.DisplayOrder == dto.DisplayOrder);
            if (orderDup)
                throw new ValidationException($"DisplayOrder {dto.DisplayOrder} already exists in this question.");

            // 4) Single/multi answers
            if (!question.IsMultipleAnswers && dto.IsCorrect)
            {
                var existedTrue = await _uow.QuizQuestionOptionRepository.ExistsAsync(o =>
                    o.QuizQuestionId == questionId && o.IsCorrect);
                if (existedTrue)
                    throw new ValidationException("This question allows only one correct option.");
            }

            // 5) OptionScore: miền giá trị + tổng không vượt quá QuestionScore
            if (dto.OptionScore is < 0 or > 999.99m)
                throw new ValidationException("OptionScore must be between 0 and 999.99.");

            if (question.QuestionScore is not null && dto.OptionScore is not null)
            {
                var currentSum = await _uow.QuizQuestionOptionRepository.GetAllAsQueryable()
                    .Where(o => o.QuizQuestionId == questionId && o.OptionScore != null)
                    .SumAsync(o => (decimal?)o.OptionScore) ?? 0m;

                if (currentSum + dto.OptionScore > question.QuestionScore)
                    throw new ValidationException($"Sum of OptionScore would exceed question score {question.QuestionScore}.");
            }

            // 6) Lưu
            var entity = new QuizQuestionOption
            {
                QuizQuestionId = questionId,
                Name = name,
                Description = dto.Description,
                IsCorrect = dto.IsCorrect,       // match DB default = 1
                DisplayOrder = dto.DisplayOrder, // KHÔNG dùng offset
                OptionScore = dto.OptionScore
            };

            await _uow.QuizQuestionOptionRepository.CreateAsync(entity);
            await _uow.SaveChangesAsync();

            return entity.Id;
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


        //====== get quiz with questions and options for teacher
        public async Task<QuizDetailDto?> GetQuizDetail(int quizId, CancellationToken ct = default)
        {
            //CancellationToken dùng để huỷ truy vấn bất đồng bộ nếu client hủy request
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


        //====== get quiz with questions and options for trainee
        public async Task<QuizTraineeDetailDto?> GetQuizDetailForTrainee(
    int quizId, CancellationToken ct = default)
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
    }
}
