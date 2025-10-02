using AutoMapper;
using Lssctc.ProgramManagement.Classes.DTOs;
using Lssctc.Share.Common;
using Lssctc.Share.Entities;
using Lssctc.Share.Enums;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;
using Entities = Lssctc.Share.Entities;

namespace Lssctc.ProgramManagement.Classes.Services
{
    public class ClassService : IClassService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ClassService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<ClassDto>> GetAllClasses()
        {
            var classes = await _unitOfWork.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.ClassCode)
                .Include(c => c.ClassInstructors)
                .Include(c => c.ClassMembers)
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<List<ClassDto>>(classes);
        }

        public async Task<PagedResult<ClassDto>> GetClasses(int page = 1, int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _unitOfWork.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.ClassCode)
                .Include(c => c.ClassInstructors)
                .Include(c => c.ClassMembers)
                .AsNoTracking();

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(c => c.StartDate) 
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mapped = _mapper.Map<List<ClassDto>>(items);

            return new PagedResult<ClassDto>
            {
                Items = mapped,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<List<ClassDto>> GetClassesByProgramCourse(int programCourseId)
        {
            if (programCourseId <= 0)
                throw new Exception("Invalid ProgramCourseId");
            if (_unitOfWork.ProgramCourseRepository.GetByIdAsync(programCourseId) == null)
                throw new Exception("ProgramCourse not found");
            var query = _unitOfWork.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.ClassCode)
                .Include(c => c.ClassInstructors)
                .Include(c => c.ClassMembers)
                .Where(c => c.ProgramCourseId == programCourseId)
                .AsNoTracking();

            var items = await query
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();

            return _mapper.Map<List<ClassDto>>(items);
        }

        public async Task<ClassDto> CreateClassByProgramCourse(ClassCreateDto dto)
        {
            // Validate ProgramCourse
            var programCourse = await _unitOfWork.ProgramCourseRepository.GetByIdAsync(dto.ProgramCourseId);

            if (programCourse == null)
                throw new Exception("Invalid ProgramCourseId");

            if (string.IsNullOrWhiteSpace(dto.ClassCode))
                throw new Exception("ClassCode is required");

            if (dto.ClassCode.Length > 50) 
                throw new Exception("ClassCode too long");

            


            if(await _unitOfWork.ClassCodeRepository.GetAllAsQueryable().AnyAsync(cc => cc.Name == dto.ClassCode))
            {
                throw new Exception("ClassCode already exists");
            }

            var newClasscode = new ClassCode
            {
                Name = dto.ClassCode,
            };

            await _unitOfWork.ClassCodeRepository.CreateAsync(newClasscode);
            await _unitOfWork.SaveChangesAsync();
            
            var newClass = _mapper.Map<Class>(dto);
            newClass.ClassCodeId = newClasscode.Id;
            newClass.Status = (int)ClassStatus.Draft;
            await _unitOfWork.ClassRepository.CreateAsync(newClass);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ClassDto>(newClass);
        }
        public async Task<ClassDto> AssignInstructorToClass(AssignInstructorDto dto)
        {
            // Validate Class
            var classEntity = await _unitOfWork.ClassRepository.GetByIdAsync(dto.ClassId);
            if (classEntity == null) throw new Exception("Class not found");

            // Validate Instructor
            var instructor = await _unitOfWork.InstructorRepository.GetByIdAsync(dto.InstructorId);
            if (instructor == null) throw new Exception("Instructor not found");
            if (instructor.IsActive == false)
                throw new Exception("Instructor is not active");

            // Check if already assigned
            var existing = classEntity.ClassInstructors
                .FirstOrDefault(ci => ci.InstructorId == dto.InstructorId);

            if (existing != null)
                throw new Exception("Instructor already assigned to this class");


            // Create ClassInstructor
            var classInstructor = new ClassInstructor
            {
                ClassId = dto.ClassId,
                InstructorId = dto.InstructorId,
                Position = dto.Position
            };
            await _unitOfWork.ClassInstructorRepository.CreateAsync(classInstructor);
            await _unitOfWork.SaveChangesAsync();

            // Reload class with instructors
            var updatedClass = await _unitOfWork.ClassRepository.GetAllAsQueryable()
                .Include(c => c.ClassInstructors)
                .Include(c => c.ClassMembers)
                .FirstOrDefaultAsync(c => c.Id == dto.ClassId);

            return _mapper.Map<ClassDto>(updatedClass);
        }


        //create class enrollment
        public async Task<ClassEnrollmentDto> EnrollTrainee(ClassEnrollmentCreateDto dto)
        {
            if (dto.ClassId <= 0) throw new Exception("Invalid ClassId");
            if (dto.TraineeId <= 0) throw new Exception("Invalid TraineeId");
            // validate class
            var classEntity = await _unitOfWork.ClassRepository
                .GetAllAsQueryable()
                .Include(c => c.ClassRegistrations)
                .FirstOrDefaultAsync(c => c.Id == dto.ClassId);

            if (classEntity == null)
                throw new Exception("Class not found");

            // validate trainee
            var trainee = await _unitOfWork.TraineeRepository.GetByIdAsync(dto.TraineeId);
            if (trainee == null)
                throw new Exception("Trainee not found");

            // check if already enrolled
            if (classEntity.ClassRegistrations.Any(e => e.TraineeId == dto.TraineeId))
                throw new Exception("Trainee already enrolled in this class");

            // map to entity
            var enrollment = _mapper.Map<ClassRegistration>(dto);
            //enrollment.Status = (int)ClassRegistrationStatus.Pending; // 1 = Pending
            enrollment.Status = (int)ClassRegistrationStatus.Approved; // 1 = Pending

            await _unitOfWork.ClassRegisRepository.CreateAsync(enrollment);
            await _unitOfWork.SaveChangesAsync();

            // reload with nav props
            enrollment = await _unitOfWork.ClassRegisRepository
                .GetAllAsQueryable()
                .Include(e => e.Class)
                .Include(e => e.Trainee)
                .FirstOrDefaultAsync(e => e.Id == enrollment.Id);

            return _mapper.Map<ClassEnrollmentDto>(enrollment);
        }

        public async Task<ClassEnrollmentDto> GetClassEnrollmentById(int classid)
        {
            return _mapper.Map<ClassEnrollmentDto>(await _unitOfWork.ClassRegisRepository
                .GetAllAsQueryable()
                .FirstOrDefaultAsync(ce => ce.ClassId == classid));
        }

        public async Task<ClassMemberDto> ApproveEnrollment(ApproveEnrollmentDto dto)
        {
            // get enrollment
            var enrollment = await _unitOfWork.ClassRegisRepository
                .GetAllAsQueryable()
                .Include(e => e.Class)
                .Include(e => e.Trainee)
                .FirstOrDefaultAsync(e => e.Id == dto.EnrollmentId);

            if (enrollment == null)
                throw new Exception("Enrollment not found");

            if (enrollment.Status == ((int)ClassRegistrationStatus.Approved)) // already approved
                throw new Exception("Enrollment already approved");

            // update enrollment
            enrollment.Status = ((int)ClassRegistrationStatus.Approved); // 2 = Approved
            enrollment.ApprovedDate = DateTime.UtcNow;
            enrollment.Description = dto.Description ?? enrollment.Description;

            _unitOfWork.ClassRegisRepository.UpdateAsync(enrollment);

            // create class member
            var member = new ClassMember
            {
                ClassId = enrollment.ClassId,
                TraineeId = enrollment.TraineeId,
                AssignedDate = DateTime.UtcNow,
                Status = 1 // Active
            };

            await _unitOfWork.ClassMemberRepository.CreateAsync(member);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ClassMemberDto>(member);
        }

        public async Task<IEnumerable<ClassMemberDto>> GetMembersByClassId(int classId)
        {
            var members = await _unitOfWork.ClassMemberRepository
                .GetAllAsQueryable()
                .Include(cm => cm.Trainee)
                .Where(cm => cm.ClassId == classId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ClassMemberDto>>(members);
        }

        public async Task<InstructorDto> GetInstructorByClassId(int classId)
        {
            if (classId <= 0) throw new Exception("Invalid classId");
            var classEntity = await _unitOfWork.ClassRepository.GetByIdAsync(classId);
            if (classEntity == null)
                throw new Exception("Class not found");
            var classInstructor = await _unitOfWork.ClassInstructorRepository
                .GetAllAsQueryable()
                .Include(ci => ci.Instructor)
                .ThenInclude(i => i.InstructorProfile)
                .FirstOrDefaultAsync(i => i.ClassId == classId);
            if(classInstructor == null)
            {
                throw new Exception("No instructor found for this class");
            }
            var instructorDTO = new InstructorDto
            {
                Biography = classInstructor.Instructor.InstructorProfile.Biography,
                ExperienceYears  = classInstructor.Instructor.InstructorProfile.ExperienceYears,
                ProfessionalProfileUrl = classInstructor.Instructor.InstructorProfile.ProfessionalProfileUrl,
                Specialization = classInstructor.Instructor.InstructorProfile.Specialization,
                HireDate = classInstructor.Instructor.HireDate,
                Id = classInstructor.InstructorId,
                InstructorCode  = classInstructor.Instructor.InstructorCode,
                IsActive = classInstructor.Instructor.IsActive,

            };
            return instructorDTO;
        }



        // TRAINGING PROGRESS & RESULTS

        public async Task<List<TrainingProgressDto>> GetProgressByMember(int memberId)
        {
            var progresses = await _unitOfWork.TrainingProgressRepository
                .GetAllAsQueryable()
                .Include(tp => tp.TrainingResults)
                    .ThenInclude(r => r.TrainingResultType)
                .Where(tp => tp.CourseMemberId == memberId)
                .ToListAsync();

            return _mapper.Map<List<TrainingProgressDto>>(progresses);
        }

        public async Task<TrainingProgressDto> CreateProgress(CreateTrainingProgressDto dto)
        {
            if (dto.CourseMemberId <= 0) throw new Exception("Invalid CourseMemberId");
            if (dto.ProgressPercentage < 0 || dto.ProgressPercentage > 100)
                throw new Exception("Progress must be between 0 and 100");

            var classMember = await _unitOfWork.ClassMemberRepository.GetByIdAsync(dto.CourseMemberId);
            if (classMember == null) throw new Exception("Class member not found");
            var entity = _mapper.Map<TrainingProgress>(dto);
            await _unitOfWork.TrainingProgressRepository.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<TrainingProgressDto>(entity);
        }

        public async Task<TrainingProgressDto> UpdateProgress(UpdateTrainingProgressDto dto)
        {
            var entity = await _unitOfWork.TrainingProgressRepository.GetByIdAsync(dto.Id);
            if (entity == null) throw new Exception("Training progress not found");

            _mapper.Map(dto, entity);
            await _unitOfWork.TrainingProgressRepository.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<TrainingProgressDto>(entity);
        }

        public async Task<bool> DeleteProgress(int id)
        {
            var entity = await _unitOfWork.TrainingProgressRepository.GetByIdAsync(id);
            if (entity == null) return false;

            await _unitOfWork.TrainingProgressRepository.DeleteAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // Training Results 

        public async Task<List<TrainingResultDto>> GetResultsByProgress(int progressId)
        {
            var results = await _unitOfWork.TrainingResultRepository
                .GetAllAsQueryable()
                .Include(r => r.TrainingResultType)
                .Where(r => r.TrainingProgressId == progressId)
                .ToListAsync();

            return _mapper.Map<List<TrainingResultDto>>(results);
        }

        public async Task<TrainingResultDto> CreateResult(CreateTrainingResultDto dto)
        {
            if (dto.TrainingProgressId <= 0) throw new Exception("Invalid TrainingProgressId");
            if (dto.TrainingResultTypeId <= 0) throw new Exception("Invalid TrainingResultTypeId");
            if (dto.ResultDate > DateTime.UtcNow)
                throw new Exception("Result date cannot be in the future");

            var progress = await _unitOfWork.TrainingProgressRepository.GetByIdAsync(dto.TrainingProgressId);
            if (progress == null) throw new Exception("Training progress not found");

            var type = await _unitOfWork.TrainingResultTypeRepository.GetByIdAsync(dto.TrainingResultTypeId);
            if (type == null) throw new Exception("Invalid TrainingResultTypeId");


            var entity = _mapper.Map<Entities.TrainingResult>(dto);
            await _unitOfWork.TrainingResultRepository.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<TrainingResultDto>(entity);
        }

        public async Task<TrainingResultDto> UpdateResult(UpdateTrainingResultDto dto)
        {

            var entity = await _unitOfWork.TrainingResultRepository.GetByIdAsync(dto.Id);
            if (entity == null) throw new Exception("Training result not found");

            _mapper.Map(dto, entity);
            await _unitOfWork.TrainingResultRepository.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<TrainingResultDto>(entity);
        }

        public async Task<bool> DeleteResult(int id)
        {
            var entity = await _unitOfWork.TrainingResultRepository.GetByIdAsync(id);
            if (entity == null) return false;

            await _unitOfWork.TrainingResultRepository.DeleteAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }



        //section

        public async Task<SectionDto> CreateSectionAsync(SectionCreateDto dto)
        {
            var classEntity = await _unitOfWork.ClassRepository.GetByIdAsync(dto.ClassId);
            if (classEntity == null) throw new Exception("Class not found");

            var syllabusSection = await _unitOfWork.SyllabusSectionRepository.GetByIdAsync(dto.SyllabusSectionId);
            if (syllabusSection == null) throw new Exception("Syllabus section not found");

            var entity = _mapper.Map<Section>(dto);

            await _unitOfWork.SectionRepository.CreateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SectionDto>(entity);
        }

        public async Task<SyllabusSectionDto> CreateSyllabusSectionAsync(SyllabusSectionCreateDto dto)
        {
            // Create Syllabus
            var syllabus = _mapper.Map<Syllabuse>(dto);
            syllabus.IsActive = true;
            syllabus.IsDeleted = false;

            await _unitOfWork.SyllabuseRepository.CreateAsync(syllabus);
            await _unitOfWork.SaveChangesAsync();

            // Create Section linked to Syllabus
            var section = _mapper.Map<SyllabusSection>(dto);
            section.SyllabusId = syllabus.Id;

            await _unitOfWork.SyllabusSectionRepository.CreateAsync(section);
            await _unitOfWork.SaveChangesAsync();

            // Reload with navigation
            section = await _unitOfWork.SyllabusSectionRepository
                .GetAllAsQueryable()
                .Include(s => s.Syllabus)
                .FirstOrDefaultAsync(s => s.Id == section.Id);

            return _mapper.Map<SyllabusSectionDto>(section);
        }

    }
}
