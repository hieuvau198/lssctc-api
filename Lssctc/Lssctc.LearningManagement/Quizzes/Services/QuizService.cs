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

      

        public async Task<QuizDto?> GetByIdAsync(int id)
        {
            var entity = await _uow.QuizRepository.GetByIdAsync(id); 
            return entity == null ? null : _mapper.Map<QuizDto>(entity);
        }

        public async Task<(IReadOnlyList<QuizDto> Items, int Total)> GetPagedAsync(int pageIndex, int pageSize, string? search)
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
                .OrderByDescending(q => q.UpdatedAt)   // ưu tiên bản cập nhật mới
                .ThenByDescending(q => q.Id)           // ổn định thứ tự
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<QuizDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return (items, total);
        }

        public async Task<int> CreateAsync(CreateQuizDto dto)
        {
            var entity = _mapper.Map<Quiz>(dto);
            await _uow.QuizRepository.CreateAsync(entity);           
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(int id, UpdateQuizDto dto)
        {
            var entity = await _uow.QuizRepository.GetByIdAsync(id);  
            if (entity == null) return false;

            _mapper.Map(dto, entity);
            await _uow.QuizRepository.UpdateAsync(entity);            
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _uow.QuizRepository.GetByIdAsync(id);  
            if (entity == null) return false;

            await _uow.QuizRepository.DeleteAsync(entity);            
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<int> CreateQuestionsAsync(int quizId, CreateQuizQuestionDto dto)
        {
            //  Body bắt buộc
            if (dto == null) throw new ValidationException("Body is required.");

            //  Kiểm tra quiz tồn tại
            var quiz = await _uow.QuizRepository.GetByIdAsync(quizId);
            if (quiz is null) throw new KeyNotFoundException($"Quiz {quizId} not found.");

            //  Validate & chuẩn hoá Name
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

            // Tạo entity
            var entity = _mapper.Map<QuizQuestion>(dto);
            entity.QuizId = quizId;
            entity.Name = normalizedName;
            entity.QuestionScore = score;

         

            await _uow.QuizQuestionRepository.CreateAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }


        //=========== create question option =============


        // ---- SINGLE ----
        public async Task<int> CreateQuizQuestionOptionAsync(int questionId, CreateQuizQuestionOptionDto dto)
        {
            if (dto == null) throw new ValidationException("Body is required.");

            var qq = await _uow.QuizQuestionRepository.GetAllAsQueryable()
                .Where(x => x.Id == questionId)
                .Select(x => new { x.Id, x.QuizId, x.QuestionScore })
                .FirstOrDefaultAsync();
            if (qq is null) throw new KeyNotFoundException($"Question {questionId} not found.");
            if (await _uow.QuizRepository.GetByIdAsync(qq.QuizId) is null)
                throw new KeyNotFoundException($"Quiz {qq.QuizId} not found.");

            // Validate name/desc
            var name = dto.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name)) throw new ValidationException("Name is required.");
            name = string.Join(" ", name.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (name.Length > 100) throw new ValidationException("Name must be at most 100 characters.");
            if (!string.IsNullOrEmpty(dto.Description) && dto.Description.Length > 2000)
                throw new ValidationException("Description must be at most 2000 characters.");

            // Tránh trùng tên trong cùng question
            var dup = await _uow.QuizQuestionOptionRepository.ExistsAsync(o =>
                o.QuizQuestionId == questionId &&
                o.Name != null && o.Name.ToLower() == name.ToLower());
            if (dup) throw new ValidationException("An option with the same name already exists in this question.");

            // Điểm
            decimal? score = dto.OptionScore;
            if (score.HasValue)
            {
                if (score.Value < 0m) throw new ValidationException("OptionScore must be >= 0.");
                score = Math.Round(score.Value, 2, MidpointRounding.AwayFromZero);
            }

            // BẮT BUỘC: display_order phải đúng số tiếp theo trong phạm vi question
            var currentMax = await _uow.QuizQuestionOptionRepository.GetAllAsQueryable()
                .Where(o => o.QuizQuestionId == questionId)
                .Select(o => (int?)o.DisplayOrder)
                .MaxAsync() ?? 0;

            var expected = currentMax + 1;
            if (dto.DisplayOrder != expected)
                throw new ValidationException($"DisplayOrder must be exactly {expected} for this question.");

            // (Tuỳ yêu cầu) Nếu bạn đang enforce tổng OptionScore = QuestionScore:
            if (qq.QuestionScore.HasValue)
            {
                if (!score.HasValue)
                    throw new ValidationException("OptionScore is required because total must equal QuestionScore.");

                var sumExisting = await _uow.QuizQuestionOptionRepository.GetAllAsQueryable()
                    .Where(o => o.QuizQuestionId == questionId)
                    .Select(o => (decimal?)o.OptionScore)
                    .SumAsync() ?? 0m;

                var sumFinal = Math.Round(sumExisting + score.Value, 2, MidpointRounding.AwayFromZero);
                var target = Math.Round(qq.QuestionScore.Value, 2, MidpointRounding.AwayFromZero);
                if (sumFinal != target)
                    throw new ValidationException($"Total OptionScore must equal {target}. Current after add: {sumFinal}.");
            }

            var entity = new QuizQuestionOption
            {
                QuizQuestionId = questionId,
                Name = name,
                Description = dto.Description,
                IsCorrect = dto.IsCorrect,
                OptionScore = score,
                DisplayOrder = dto.DisplayOrder // lưu đúng số client gửi (đã check)
            };

            await _uow.QuizQuestionOptionRepository.CreateAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }


        // ---- BULK ----
        public async Task<IReadOnlyList<int>> CreateQuizQuestionOptionsBulkAsync(int questionId, CreateQuizQuestionOptionBulkDto dto)
        {
            if (dto == null || dto.Items == null || dto.Items.Count == 0)
                throw new ValidationException("Danh sách option rỗng.");

            // 1) Lấy QuizId + QuestionScore, validate tồn tại
            var qq = await _uow.QuizQuestionRepository.GetAllAsQueryable()
                .Where(x => x.Id == questionId)
                .Select(x => new { x.Id, x.QuizId, x.QuestionScore })
                .FirstOrDefaultAsync();
            if (qq is null) throw new KeyNotFoundException($"Question {questionId} not found.");
            if (await _uow.QuizRepository.GetByIdAsync(qq.QuizId) is null)
                throw new KeyNotFoundException($"Quiz {qq.QuizId} not found.");

            // 2) Chuẩn hóa & validate từng item
            for (int idx = 0; idx < dto.Items.Count; idx++)
            {
                var it = dto.Items[idx];

                it.Name = it.Name?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(it.Name))
                    throw new ValidationException($"Tên option (item #{idx + 1}) không được rỗng.");

                it.Name = string.Join(" ", it.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries));
                if (it.Name.Length > 100)
                    throw new ValidationException($"Tên option (item #{idx + 1}) tối đa 100 ký tự.");

                if (!string.IsNullOrEmpty(it.Description) && it.Description.Length > 2000)
                    throw new ValidationException($"Description (item #{idx + 1}) tối đa 2000 ký tự.");

                if (it.OptionScore.HasValue)
                {
                    if (it.OptionScore.Value < 0m)
                        throw new ValidationException($"OptionScore (item #{idx + 1}) phải >= 0.");
                    it.OptionScore = R2(it.OptionScore.Value); // làm tròn 2 số
                }
            }

            // 3) Trùng tên trong batch
            var namesLower = dto.Items.Select(x => x.Name.ToLower()).ToList();
            var dupInBatch = namesLower.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (dupInBatch.Any())
                throw new ValidationException($"Duplicated names in request: {string.Join(", ", dupInBatch)}");

            // 4) Trùng tên với dữ liệu hiện có (trên cùng question)
            var existedNames = await _uow.QuizQuestionOptionRepository.GetAllAsQueryable()
                .Where(o => o.QuizQuestionId == questionId && o.Name != null)
                .Select(o => o.Name!.ToLower())
                .ToListAsync();
            var dupWithDb = namesLower.Intersect(existedNames).ToList();
            if (dupWithDb.Any())
                throw new ValidationException($"Option name duplicated with existing: {string.Join(", ", dupWithDb)}");

            // 5) Validate tổng điểm: (existing + new) = QuestionScore
            if (qq.QuestionScore.HasValue)
            {
                var missingScoreIdx = dto.Items
                    .Select((x, i) => (x, i))
                    .Where(t => !t.x.OptionScore.HasValue)
                    .Select(t => t.i + 1)
                    .FirstOrDefault();
                if (missingScoreIdx > 0)
                    throw new ValidationException($"OptionScore (item #{missingScoreIdx}) is required because this question enforces total OptionScore = QuestionScore.");

                var sumExisting = await _uow.QuizQuestionOptionRepository.GetAllAsQueryable()
                    .Where(o => o.QuizQuestionId == questionId)
                    .Select(o => (decimal?)o.OptionScore)
                    .SumAsync() ?? 0m;

                var sumNew = dto.Items.Sum(x => R2(x.OptionScore!.Value));
                var sumFinal = R2(sumExisting + sumNew);
                var target = R2(qq.QuestionScore.Value);

                if (sumFinal != target)
                    throw new ValidationException($"Tổng điểm option sau khi thêm phải = {target}. Hiện tại: {sumFinal}.");
            }

            // 6) Lấy currentMax của questionId
            var currentMax = await _uow.QuizQuestionOptionRepository.GetAllAsQueryable()
                .Where(o => o.QuizQuestionId == questionId)
                .Select(o => (int?)o.DisplayOrder)
                .MaxAsync() ?? 0;

            // 7) Giữ nguyên thứ tự mảng payload (KHÔNG dùng dto.DisplayOrder)
            var ordered = dto.Items; // theo đúng thứ tự người dùng gửi

            // 8) Tạo entity: DisplayOrder = currentMax + 1..n
            var displayCursor = currentMax;
            var entities = new List<QuizQuestionOption>(ordered.Count);

            foreach (var it in ordered)
            {
                var entity = new QuizQuestionOption
                {
                    QuizQuestionId = questionId,
                    Name = it.Name,
                    Description = it.Description,
                    IsCorrect = it.IsCorrect,
                    OptionScore = it.OptionScore,
                    DisplayOrder = ++displayCursor
                };
                await _uow.QuizQuestionOptionRepository.CreateAsync(entity);
                entities.Add(entity);
            }

            // 9) Lưu 1 lần, sau đó mới đọc Id
            await _uow.SaveChangesAsync();

            var createdIds = entities.Select(e => e.Id).ToList();
            return createdIds;
        }

        // Helper làm tròn 2 chữ số
        private static decimal R2(decimal v)
            => Math.Round(v, 2, MidpointRounding.AwayFromZero);



        //========= end create question option =========
    }
}
