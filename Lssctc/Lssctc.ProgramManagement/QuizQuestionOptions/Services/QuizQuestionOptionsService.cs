using Lssctc.ProgramManagement.QuizQuestionOptions.DTOs;
using Lssctc.Share.Entities;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Lssctc.ProgramManagement.QuizQuestionOptions.Services
{
    public class QuizQuestionOptionsService : IQuizQuestionOptionsService
    {

        private readonly IUnitOfWork _uow;
        public QuizQuestionOptionsService(IUnitOfWork uow)
        {
            _uow = uow;
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
                .OrderBy(o => o.DisplayOrder ?? int.MaxValue) // null sẽ đẩy về cuối
                .Select(o => new QuizDetailQuestionOptionDto
                {
                    Id = o.Id,
                    QuizQuestionId = o.QuizQuestionId,
                    Name = o.Name,
                    Description = o.Description,
                    IsCorrect = o.IsCorrect,
                    DisplayOrder = o.DisplayOrder,
                    OptionScore = o.OptionScore
                })
                .AsNoTracking()
                .ToListAsync(ct);

            if (items.Count == 0)
                throw new ValidationException($"Question {questionId} has no options.");

            return items;
        }


    }
}
