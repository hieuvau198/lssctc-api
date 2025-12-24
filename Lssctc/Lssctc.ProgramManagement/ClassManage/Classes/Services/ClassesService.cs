using Lssctc.ProgramManagement.Accounts.Authens.Services;
using Lssctc.ProgramManagement.Activities.Services;
using Lssctc.ProgramManagement.Certificates.Services;
using Lssctc.ProgramManagement.ClassManage.Classes.Dtos;
using Lssctc.ProgramManagement.ClassManage.Enrollments.Services;
using Lssctc.ProgramManagement.ClassManage.FinalExams.Services;
using Lssctc.ProgramManagement.ClassManage.Helpers;
using Lssctc.ProgramManagement.ClassManage.Timeslots.Services;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.ProgramManagement.ClassManage.Classes.Services
{
    public class ClassesService : IClassesService
    {
        private readonly IUnitOfWork _uow;
        private readonly IClassQueryService _queryService;
        private readonly ClassManageHandler _handler;
        private readonly ClassImportHandler _importHandler;
        private readonly ClassCleanupHandler _cleanupHandler;
        private readonly ITimeslotService _timeslotService;
        private readonly IActivitySessionService _activitySessionService;
        private readonly IFEService _finalExamsService;
        private readonly IEnrollmentsService _enrollmentsService;
        private readonly ITraineeCertificatesService _traineeCertificatesService;

        public ClassesService(
            IUnitOfWork uow,
            IClassQueryService queryService,
            IMailService mailService,
            ITimeslotService timeslotService,
            IActivitySessionService activitySessionService,
            IFEService finalExamsService,
            IEnrollmentsService enrollmentsService,
            ITraineeCertificatesService traineeCertificatesService)
        {
            _uow = uow;
            _queryService = queryService;
            _timeslotService = timeslotService;
            _activitySessionService = activitySessionService;
            _finalExamsService = finalExamsService;
            _enrollmentsService = enrollmentsService;
            _traineeCertificatesService = traineeCertificatesService;

            _handler = new ClassManageHandler(uow);
            _importHandler = new ClassImportHandler(uow, mailService);
            _cleanupHandler = new ClassCleanupHandler(uow);
        }

        #region Classes (Queries Delegated to QueryService)

        public async Task<IEnumerable<ClassDto>> GetAllClassesAsync()
        {
            return await _queryService.GetAllClassesAsync();
        }

        public async Task<PagedResult<ClassDto>> GetClassesAsync(int pageNumber, int pageSize, string? searchTerm = null, string? sortBy = null, string? sortDirection = null, string? status = null)
        {
            return await _queryService.GetClassesAsync(pageNumber, pageSize, searchTerm, sortBy, sortDirection, status);
        }

        public async Task<ClassDto?> GetClassByIdAsync(int id)
        {
            return await _queryService.GetClassByIdAsync(id);
        }

        #endregion

        #region Commands (Write Operations)

        public async Task<ClassDto> CreateClassAsync(CreateClassDto dto)
        {
            // 1. Fetch Course with Hierarchy for Validation
            var course = await _uow.CourseRepository.GetAllAsQueryable()
                .Where(c => c.Id == dto.CourseId)
                .Include(c => c.CourseCertificates)
                .Include(c => c.CourseSections)
                    .ThenInclude(cs => cs.Section)
                        .ThenInclude(s => s.SectionActivities)
                            .ThenInclude(sa => sa.Activity)
                                .ThenInclude(a => a.ActivityMaterials)
                .Include(c => c.CourseSections)
                    .ThenInclude(cs => cs.Section)
                        .ThenInclude(s => s.SectionActivities)
                            .ThenInclude(sa => sa.Activity)
                                .ThenInclude(a => a.ActivityQuizzes)
                .Include(c => c.CourseSections)
                    .ThenInclude(cs => cs.Section)
                        .ThenInclude(s => s.SectionActivities)
                            .ThenInclude(sa => sa.Activity)
                                .ThenInclude(a => a.ActivityPractices)
                .FirstOrDefaultAsync();

            if (course == null) throw new KeyNotFoundException("Không tìm thấy khóa học.");

            // 2. Validate Course Structure
            if (!course.CourseCertificates.Any())
            {
                throw new InvalidOperationException("Không thể tạo lớp học: Khóa học đã chọn phải có ít nhất một chứng chỉ.");
            }

            if (!course.CourseSections.Any())
            {
                throw new InvalidOperationException("Không thể tạo lớp học: Khóa học đã chọn phải có ít nhất một chương học.");
            }

            // Iterate through all sections to ensure they have activities, and those activities have content
            foreach (var courseSection in course.CourseSections)
            {
                var section = courseSection.Section;
                if (!section.SectionActivities.Any())
                {
                    throw new InvalidOperationException($"Không thể tạo lớp học: Chương '{section.SectionTitle}' chưa được gán hoạt động nào.");
                }

                foreach (var sectionActivity in section.SectionActivities)
                {
                    var activity = sectionActivity.Activity;
                    if (!activity.ActivityType.HasValue)
                    {
                        throw new InvalidOperationException($"Không thể tạo lớp học: Hoạt động '{activity.ActivityTitle}' không có loại hợp lệ.");
                    }

                    var type = (ActivityType)activity.ActivityType.Value;
                    bool hasContent = false;

                    switch (type)
                    {
                        case ActivityType.Material:
                            hasContent = activity.ActivityMaterials.Any();
                            break;
                        case ActivityType.Quiz:
                            hasContent = activity.ActivityQuizzes.Any();
                            break;
                        case ActivityType.Practice:
                            hasContent = activity.ActivityPractices.Any();
                            break;
                        default:
                            break;
                    }

                    if (!hasContent)
                    {
                        throw new InvalidOperationException($"Không thể tạo lớp học: Hoạt động '{activity.ActivityTitle}' (Loại: {type}) trong chương '{section.SectionTitle}' chưa được gán nội dung.");
                    }
                }
            }

            // 3. Prepare Dates
            var startDateUtc = dto.StartDate;
            var endDateUtc = dto.EndDate;

            if (startDateUtc < DateTime.UtcNow.AddDays(-30)) throw new InvalidOperationException("Ngày bắt đầu không được quá 30 ngày trong quá khứ.");
            if (!endDateUtc.HasValue || endDateUtc <= startDateUtc.AddDays(2)) throw new InvalidOperationException("Ngày kết thúc phải sau ngày bắt đầu ít nhất 3 ngày.");

            // 4. Validate Class Code Unique
            var existingClassCode = await _uow.ClassCodeRepository.GetAllAsQueryable()
                .FirstOrDefaultAsync(cc => cc.Name.ToLower() == dto.ClassCode.Trim().ToLower());
            if (existingClassCode != null) throw new InvalidOperationException($"Mã lớp '{existingClassCode.Name}' đã tồn tại.");

            // 5. Validate ProgramCourse link
            var programCourse = await _uow.ProgramCourseRepository.GetAllAsQueryable()
                .FirstOrDefaultAsync(pc => pc.ProgramId == dto.ProgramId && pc.CourseId == dto.CourseId);
            if (programCourse == null) throw new KeyNotFoundException("Không tìm thấy chương trình học phù hợp.");

            // 6. Create Entities
            var classCodeEntity = new ClassCode { Name = dto.ClassCode.Trim() };
            await _uow.ClassCodeRepository.CreateAsync(classCodeEntity);
            await _uow.SaveChangesAsync();

            var newClass = new Class
            {
                Name = dto.Name.Trim(),
                Capacity = dto.Capacity,
                ProgramCourseId = programCourse.Id,
                ClassCodeId = classCodeEntity.Id,
                Description = dto.Description?.Trim() ?? string.Empty,
                StartDate = startDateUtc,
                EndDate = endDateUtc,
                Status = (int)ClassStatusEnum.Draft,
                BackgroundImageUrl = dto.BackgroundImageUrl ?? "https://templates.framework-y.com/lightwire/images/wide-1.jpg"
            };

            await _uow.ClassRepository.CreateAsync(newClass);
            await _uow.SaveChangesAsync();
            await _finalExamsService.AutoCreateFinalExamsForClassAsync(newClass.Id);

            return (await GetClassByIdAsync(newClass.Id))!;
        }

        public async Task<ClassDto> UpdateClassAsync(int id, UpdateClassDto dto)
        {
            var existing = await _uow.ClassRepository.GetAllAsQueryable()
                 .Include(c => c.ProgramCourse).ThenInclude(pc => pc.Course)
                 .Include(c => c.ClassCode)
                 .FirstOrDefaultAsync(c => c.Id == id);

            if (existing == null) throw new KeyNotFoundException($"Không tìm thấy lớp học với ID {id}.");
            if (existing.Status != (int)ClassStatusEnum.Draft) throw new InvalidOperationException("Chỉ có thể cập nhật các lớp học ở trạng thái 'Nháp'.");

            var startDateUtc = dto.StartDate;
            var endDateUtc = dto.EndDate;

            if (startDateUtc < DateTime.UtcNow.AddDays(-30)) throw new InvalidOperationException("Ngày bắt đầu không được quá 30 ngày trong quá khứ.");
            if (!endDateUtc.HasValue || endDateUtc <= startDateUtc.AddDays(2)) throw new InvalidOperationException("Ngày kết thúc phải sau ngày bắt đầu ít nhất 3 ngày.");

            existing.Name = dto.Name.Trim();
            existing.Capacity = dto.Capacity;
            existing.Description = dto.Description?.Trim() ?? existing.Description;
            existing.StartDate = startDateUtc;
            existing.EndDate = endDateUtc;
            if (dto.BackgroundImageUrl != null) existing.BackgroundImageUrl = dto.BackgroundImageUrl;

            await _uow.ClassRepository.UpdateAsync(existing);
            await _uow.SaveChangesAsync();

            return MapToDto(existing);
        }

        public async Task OpenClassAsync(int id)
        {
            var existing = await _uow.ClassRepository.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"Không tìm thấy lớp học với ID {id}.");
            if (existing.Status != (int)ClassStatusEnum.Draft) throw new InvalidOperationException("Chỉ có thể mở các lớp học ở trạng thái 'Nháp'.");

            var hasInstructors = await _uow.ClassInstructorRepository.GetAllAsQueryable()
                .AnyAsync(ci => ci.ClassId == id);
            if (!hasInstructors) throw new InvalidOperationException("Không thể mở lớp: Phải có ít nhất một giảng viên được phân công.");

            var hasTimeslots = await _uow.TimeslotRepository.GetAllAsQueryable()
                .AnyAsync(t => t.ClassId == id);
            if (!hasTimeslots) throw new InvalidOperationException("Không thể mở lớp: Phải cấu hình lịch học (Timeslots).");

            // Validate Final Exam Config (Same as Start Class)
            await ValidateFinalExamConfigurationAsync(id, "mở lớp học");

            existing.Status = (int)ClassStatusEnum.Open;
            await _uow.ClassRepository.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
        }

        public async Task StartClassAsync(int id)
        {
            var existing = await _uow.ClassRepository.GetAllAsQueryable()
                .Include(c => c.ClassInstructors).Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existing == null) throw new KeyNotFoundException($"Không tìm thấy lớp học với ID {id}.");
            if (existing.Status != (int)ClassStatusEnum.Draft && existing.Status != (int)ClassStatusEnum.Open) throw new InvalidOperationException("Chỉ có thể bắt đầu các lớp học ở trạng thái 'Nháp' hoặc 'Mở'.");
            if (existing.ClassInstructors == null || !existing.ClassInstructors.Any()) throw new InvalidOperationException("Không thể bắt đầu lớp học nếu chưa có giảng viên.");
            if (existing.Enrollments == null || !existing.Enrollments.Any(e => e.Status == (int)EnrollmentStatusEnum.Enrolled)) throw new InvalidOperationException("Không thể bắt đầu lớp học nếu chưa có ít nhất một học viên ghi danh.");

            // Validate Final Exam Config
            await ValidateFinalExamConfigurationAsync(id, "bắt đầu lớp học");

            foreach (var enrollment in existing.Enrollments)
            {
                if (enrollment.Status == (int)EnrollmentStatusEnum.Enrolled)
                {
                    enrollment.Status = (int)EnrollmentStatusEnum.Inprogress;
                    await _uow.EnrollmentRepository.UpdateAsync(enrollment);
                }
                else if (enrollment.Status == (int)EnrollmentStatusEnum.Pending)
                {
                    enrollment.Status = (int)EnrollmentStatusEnum.Rejected;
                    await _uow.EnrollmentRepository.UpdateAsync(enrollment);
                }
            }

            existing.Status = (int)ClassStatusEnum.Inprogress;
            await _uow.ClassRepository.UpdateAsync(existing);
            await _uow.SaveChangesAsync();

            await _handler.EnsureProgressScaffoldingForClassAsync(id);
            await _timeslotService.CreateAttendanceForClassAsync(id);
            await _activitySessionService.CreateSessionsOnClassStartAsync(id);
            await _finalExamsService.AutoCreateFinalExamsForClassAsync(id);
        }

        public async Task CompleteClassAsync(int id)
        {
            var existingClass = await _uow.ClassRepository.GetByIdAsync(id);
            if (existingClass == null)
                throw new KeyNotFoundException($"Không tìm thấy lớp học với ID {id}.");
            if (existingClass.Status != (int)ClassStatusEnum.Inprogress)
                throw new InvalidOperationException("Chỉ có thể hoàn thành các lớp học đang diễn ra (Inprogress).");

            var finalExams = await _uow.FinalExamRepository.GetAllAsQueryable()
                .Include(fe => fe.Enrollment)
                .Where(fe => fe.Enrollment.ClassId == id)
                .ToListAsync();

            bool allExamsResolved = finalExams.All(fe =>
                fe.Status == (int)FinalExamStatusEnum.Completed ||
                fe.Status == (int)FinalExamStatusEnum.Cancelled);

            if (!allExamsResolved)
                throw new InvalidOperationException("Không thể hoàn thành lớp học. Tất cả các bài kiểm tra cuối khóa phải ở trạng thái Hoàn thành hoặc Đã hủy.");

            var enrollments = await _uow.EnrollmentRepository.GetAllAsQueryable()
                .Where(e => e.ClassId == id)
                .Include(e => e.FinalExams)
                .ToListAsync();

            var enrollmentsToUpdate = new List<Enrollment>();

            foreach (var enrollment in enrollments)
            {
                if (enrollment.Status == (int)EnrollmentStatusEnum.Inprogress)
                {
                    var finalExam = enrollment.FinalExams
                        .OrderByDescending(fe => fe.Id)
                        .FirstOrDefault();

                    if (finalExam != null && finalExam.IsPass == true)
                    {
                        enrollment.Status = (int)EnrollmentStatusEnum.Completed;
                    }
                    else
                    {
                        enrollment.Status = (int)EnrollmentStatusEnum.Failed;
                    }
                    enrollmentsToUpdate.Add(enrollment);
                }
                else if (enrollment.Status == (int)EnrollmentStatusEnum.Cancelled ||
                         enrollment.Status == (int)EnrollmentStatusEnum.Rejected ||
                         enrollment.Status == (int)EnrollmentStatusEnum.Completed ||
                         enrollment.Status == (int)EnrollmentStatusEnum.Failed)
                {
                    continue;
                }
                else
                {
                    enrollment.Status = (int)EnrollmentStatusEnum.Cancelled;
                    enrollmentsToUpdate.Add(enrollment);
                }
            }

            if (enrollmentsToUpdate.Any())
            {
                await _enrollmentsService.UpdateEnrollmentsAsync(enrollmentsToUpdate);
            }

            existingClass.Status = (int)ClassStatusEnum.Completed;
            await _uow.ClassRepository.UpdateAsync(existingClass);
            await _uow.SaveChangesAsync();

            await _traineeCertificatesService.CreateTraineeCertificatesForCompleteClass(id);
        }

        public async Task CancelClassAsync(int id)
        {
            var existing = await _uow.ClassRepository.GetAllAsQueryable().Include(c => c.Enrollments).FirstOrDefaultAsync(c => c.Id == id);
            if (existing == null) throw new KeyNotFoundException($"Không tìm thấy lớp học với ID {id}.");
            if (existing.Enrollments != null && existing.Enrollments.Any()) throw new InvalidOperationException("Không thể hủy lớp học đã có học viên ghi danh.");
            if (existing.Status == (int)ClassStatusEnum.Inprogress || existing.Status == (int)ClassStatusEnum.Completed || existing.Status == (int)ClassStatusEnum.Cancelled)
                throw new InvalidOperationException("Không thể hủy lớp học đang diễn ra, đã hoàn thành hoặc đã bị hủy.");

            existing.Status = (int)ClassStatusEnum.Cancelled;
            await _uow.ClassRepository.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
        }

        public async Task DeleteClassDataRecursiveAsync(int classId)
        {
            await _cleanupHandler.DeleteClassDataRecursiveAsync(classId);
        }

        public async Task<string> ImportTraineesToClassAsync(int classId, IFormFile file)
        {
            return await _importHandler.ImportTraineesToClassAsync(classId, file);
        }

        #endregion

        #region Read Operations (Queries Delegated to QueryService)

        public async Task<IEnumerable<ClassDto>> GetClassesByProgramAndCourseAsync(int programId, int courseId)
        {
            return await _queryService.GetClassesByProgramAndCourseAsync(programId, courseId);
        }

        public async Task<IEnumerable<ClassDto>> GetClassesByCourseAsync(int courseId)
        {
            return await _queryService.GetClassesByCourseAsync(courseId);
        }

        public async Task<IEnumerable<ClassDto>> GetClassesByCourseIdForTrainee(int courseId)
        {
            return await _queryService.GetClassesByCourseIdForTrainee(courseId);
        }

        public async Task<IEnumerable<ClassDto>> GetClassesByInstructorAsync(int instructorId)
        {
            return await _queryService.GetClassesByInstructorAsync(instructorId);
        }

        public async Task<IEnumerable<ClassDto>> GetAllClassesByTraineeAsync(int traineeId)
        {
            return await _queryService.GetAllClassesByTraineeAsync(traineeId);
        }

        public async Task<PagedResult<ClassDto>> GetPagedClassesByTraineeAsync(int traineeId, int pageNumber, int pageSize)
        {
            return await _queryService.GetPagedClassesByTraineeAsync(traineeId, pageNumber, pageSize);
        }

        public async Task<ClassDto?> GetClassByIdAndTraineeAsync(int classId, int traineeId)
        {
            return await _queryService.GetClassByIdAndTraineeAsync(classId, traineeId);
        }

        public async Task<IEnumerable<ClassWithEnrollmentDto>> GetAvailableClassesByProgramCourseForTraineeAsync(int programId, int courseId, int? traineeId)
        {
            return await _queryService.GetAvailableClassesByProgramCourseForTraineeAsync(programId, courseId, traineeId);
        }

        #endregion

        #region Helpers

        private async Task ValidateFinalExamConfigurationAsync(int classId, string actionName)
        {
            var feTemplate = await _uow.FinalExamTemplateRepository.GetAllAsQueryable()
                .Include(t => t.FinalExamPartialsTemplates)
                .FirstOrDefaultAsync(t => t.ClassId == classId);

            if (feTemplate == null || !feTemplate.FinalExamPartialsTemplates.Any())
                throw new InvalidOperationException($"Không thể {actionName}: Cấu hình bài thi cuối khóa chưa được tạo.");

            var exampleExam = await _uow.FinalExamRepository.GetAllAsQueryable()
                .Include(fe => fe.FinalExamPartials).ThenInclude(p => p.FeTheories)
                .Include(fe => fe.FinalExamPartials).ThenInclude(p => p.FeSimulations)
                .Include(fe => fe.FinalExamPartials).ThenInclude(p => p.PeChecklists)
                .FirstOrDefaultAsync(fe => fe.Enrollment.ClassId == classId && fe.Enrollment.IsDeleted != true);

            if (exampleExam == null)
                throw new InvalidOperationException($"Không thể {actionName}: Dữ liệu bài thi cuối khóa chưa được khởi tạo. Vui lòng lưu cấu hình bài thi trước.");

            foreach (var templatePartial in feTemplate.FinalExamPartialsTemplates)
            {
                var partial = exampleExam.FinalExamPartials.FirstOrDefault(p => p.Type == templatePartial.Type);

                if (partial == null)
                    throw new InvalidOperationException($"Không thể {actionName}: Phần thi {GetExamTypeName(templatePartial.Type)} được yêu cầu nhưng chưa được tạo trong dữ liệu bài thi.");

                if (templatePartial.Type == 1) // Theory
                {
                    var theory = partial.FeTheories.FirstOrDefault();
                    if (theory == null || theory.QuizId <= 0)
                        throw new InvalidOperationException($"Không thể {actionName}: Bài thi Lý thuyết (TE) chưa được gán đề trắc nghiệm (Quiz).");
                }
                else if (templatePartial.Type == 2) // Simulation
                {
                    var sim = partial.FeSimulations.FirstOrDefault();
                    if (sim == null || sim.PracticeId <= 0)
                        throw new InvalidOperationException($"Không thể {actionName}: Bài thi Mô phỏng (SE) chưa được gán bài thực hành (Practice).");
                }
                else if (templatePartial.Type == 3) // Practical
                {
                    if (partial.PeChecklists == null || !partial.PeChecklists.Any())
                        throw new InvalidOperationException($"Không thể {actionName}: Bài thi Thực hành (PE) chưa có Checklist đánh giá.");
                }
            }
        }

        #endregion

        #region Mapping (Kept for UpdateClassAsync)

        private static ClassDto MapToDto(Class c)
        {
            string classStatus = c.Status.HasValue ? Enum.GetName(typeof(ClassStatusEnum), c.Status.Value) ?? "Cancelled" : "Cancelled";
            var startDateVn = c.StartDate;
            var endDateVn = c.EndDate;

            return new ClassDto
            {
                Id = c.Id,
                Name = c.Name,
                Capacity = c.Capacity,
                ClassCode = c.ClassCode?.Name ?? "CLS099",
                ProgramId = c.ProgramCourse.ProgramId,
                CourseId = c.ProgramCourse.CourseId,
                Description = c.Description,
                StartDate = startDateVn,
                EndDate = endDateVn,
                Status = classStatus,
                DurationHours = c.ProgramCourse.Course?.DurationHours,
                BackgroundImageUrl = c.BackgroundImageUrl
            };
        }

        private string GetExamTypeName(int typeId)
        {
            return typeId switch
            {
                1 => "Lý thuyết (Theory)",
                2 => "Mô phỏng (Simulation)",
                3 => "Thực hành (Practical)",
                _ => "Không xác định"
            };
        }

        #endregion
    }
}