using System;
using System.Collections.Generic;
using Lssctc.Share.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lssctc.Share.Contexts;

public partial class LssctcDbContext : DbContext
{
    public LssctcDbContext()
    {
    }

    public LssctcDbContext(DbContextOptions<LssctcDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Activity> Activities { get; set; }

    public virtual DbSet<ActivityMaterial> ActivityMaterials { get; set; }

    public virtual DbSet<ActivityPractice> ActivityPractices { get; set; }

    public virtual DbSet<ActivityQuiz> ActivityQuizzes { get; set; }

    public virtual DbSet<ActivityRecord> ActivityRecords { get; set; }

    public virtual DbSet<ActivitySession> ActivitySessions { get; set; }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<BrandModel> BrandModels { get; set; }

    public virtual DbSet<Certificate> Certificates { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<ClassCode> ClassCodes { get; set; }

    public virtual DbSet<ClassInstructor> ClassInstructors { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CourseCategory> CourseCategories { get; set; }

    public virtual DbSet<CourseCertificate> CourseCertificates { get; set; }

    public virtual DbSet<CourseCode> CourseCodes { get; set; }

    public virtual DbSet<CourseLevel> CourseLevels { get; set; }

    public virtual DbSet<CourseSection> CourseSections { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<FeSimulation> FeSimulations { get; set; }

    public virtual DbSet<FeTheory> FeTheories { get; set; }

    public virtual DbSet<FinalExam> FinalExams { get; set; }

    public virtual DbSet<FinalExamPartial> FinalExamPartials { get; set; }

    public virtual DbSet<FinalExamPartialsTemplate> FinalExamPartialsTemplates { get; set; }

    public virtual DbSet<FinalExamTemplate> FinalExamTemplates { get; set; }

    public virtual DbSet<Instructor> Instructors { get; set; }

    public virtual DbSet<InstructorFeedback> InstructorFeedbacks { get; set; }

    public virtual DbSet<InstructorProfile> InstructorProfiles { get; set; }

    public virtual DbSet<LearningMaterial> LearningMaterials { get; set; }

    public virtual DbSet<LearningProgress> LearningProgresses { get; set; }

    public virtual DbSet<MaterialAuthor> MaterialAuthors { get; set; }

    public virtual DbSet<PeChecklist> PeChecklists { get; set; }

    public virtual DbSet<Practice> Practices { get; set; }

    public virtual DbSet<PracticeAttempt> PracticeAttempts { get; set; }

    public virtual DbSet<PracticeAttemptTask> PracticeAttemptTasks { get; set; }

    public virtual DbSet<PracticeTask> PracticeTasks { get; set; }

    public virtual DbSet<ProgramCourse> ProgramCourses { get; set; }

    public virtual DbSet<Quiz> Quizzes { get; set; }

    public virtual DbSet<QuizAttempt> QuizAttempts { get; set; }

    public virtual DbSet<QuizAttemptAnswer> QuizAttemptAnswers { get; set; }

    public virtual DbSet<QuizAttemptQuestion> QuizAttemptQuestions { get; set; }

    public virtual DbSet<QuizAuthor> QuizAuthors { get; set; }

    public virtual DbSet<QuizQuestion> QuizQuestions { get; set; }

    public virtual DbSet<QuizQuestionOption> QuizQuestionOptions { get; set; }

    public virtual DbSet<SeTask> SeTasks { get; set; }

    public virtual DbSet<Section> Sections { get; set; }

    public virtual DbSet<SectionActivity> SectionActivities { get; set; }

    public virtual DbSet<SectionRecord> SectionRecords { get; set; }

    public virtual DbSet<SimSetting> SimSettings { get; set; }

    public virtual DbSet<SimTask> SimTasks { get; set; }

    public virtual DbSet<SimulationComponent> SimulationComponents { get; set; }

    public virtual DbSet<SimulationManager> SimulationManagers { get; set; }

    public virtual DbSet<Timeslot> Timeslots { get; set; }

    public virtual DbSet<Trainee> Trainees { get; set; }

    public virtual DbSet<TraineeCertificate> TraineeCertificates { get; set; }

    public virtual DbSet<TraineeProfile> TraineeProfiles { get; set; }

    public virtual DbSet<TrainingProgram> TrainingPrograms { get; set; }

    public virtual DbSet<User> Users { get; set; }

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__activiti__3213E83F00AB123E");

            entity.ToTable("activities");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActivityDescription)
                .HasMaxLength(1000)
                .HasColumnName("activity_description");
            entity.Property(e => e.ActivityTitle)
                .HasMaxLength(200)
                .HasColumnName("activity_title");
            entity.Property(e => e.ActivityType)
                .HasDefaultValue(1)
                .HasColumnName("activity_type");
            entity.Property(e => e.EstimatedDurationMinutes).HasColumnName("estimated_duration_minutes");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
        });

        modelBuilder.Entity<ActivityMaterial>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__activity__3213E83F981338D7");

            entity.ToTable("activity_materials");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActivityId).HasColumnName("activity_id");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.LearningMaterialId).HasColumnName("learning_material_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");

            entity.HasOne(d => d.Activity).WithMany(p => p.ActivityMaterials)
                .HasForeignKey(d => d.ActivityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__activity___activ__7211DF33");

            entity.HasOne(d => d.LearningMaterial).WithMany(p => p.ActivityMaterials)
                .HasForeignKey(d => d.LearningMaterialId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__activity___learn__7306036C");
        });

        modelBuilder.Entity<ActivityPractice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__activity__3213E83F5BA51EBF");

            entity.ToTable("activity_practices");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActivityId).HasColumnName("activity_id");
            entity.Property(e => e.CustomDeadline)
                .HasPrecision(0)
                .HasColumnName("custom_deadline");
            entity.Property(e => e.CustomDescription)
                .HasMaxLength(1000)
                .HasColumnName("custom_description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.PracticeId).HasColumnName("practice_id");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");

            entity.HasOne(d => d.Activity).WithMany(p => p.ActivityPractices)
                .HasForeignKey(d => d.ActivityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__activity___activ__155B1B70");

            entity.HasOne(d => d.Practice).WithMany(p => p.ActivityPractices)
                .HasForeignKey(d => d.PracticeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__activity___pract__164F3FA9");
        });

        modelBuilder.Entity<ActivityQuiz>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__activity__3213E83FFB052D4B");

            entity.ToTable("activity_quizzes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActivityId).HasColumnName("activity_id");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.QuizId).HasColumnName("quiz_id");

            entity.HasOne(d => d.Activity).WithMany(p => p.ActivityQuizzes)
                .HasForeignKey(d => d.ActivityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__activity___activ__033C6B35");

            entity.HasOne(d => d.Quiz).WithMany(p => p.ActivityQuizzes)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__activity___quiz___024846FC");
        });

        modelBuilder.Entity<ActivityRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__activity__3213E83FDE3423DC");

            entity.ToTable("activity_records");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActivityId)
                .HasDefaultValue(-1)
                .HasColumnName("activity_id");
            entity.Property(e => e.ActivityType).HasColumnName("activity_type");
            entity.Property(e => e.CompletedDate)
                .HasPrecision(0)
                .HasColumnName("completed_date");
            entity.Property(e => e.IsCompleted)
                .HasDefaultValue(false)
                .HasColumnName("is_completed");
            entity.Property(e => e.Score)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("score");
            entity.Property(e => e.SectionRecordId).HasColumnName("section_record_id");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");

            entity.HasOne(d => d.SectionRecord).WithMany(p => p.ActivityRecords)
                .HasForeignKey(d => d.SectionRecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__activity___secti__5540965B");
        });

        modelBuilder.Entity<ActivitySession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__activity__3213E83FB1923640");

            entity.ToTable("activity_sessions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActivityId).HasColumnName("activity_id");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.EndTime)
                .HasPrecision(0)
                .HasColumnName("end_time");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.StartTime)
                .HasPrecision(0)
                .HasColumnName("start_time");

            entity.HasOne(d => d.Activity).WithMany(p => p.ActivitySessions)
                .HasForeignKey(d => d.ActivityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__activity___activ__1BD30ED5");

            entity.HasOne(d => d.Class).WithMany(p => p.ActivitySessions)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__activity___class__1ADEEA9C");
        });

        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__admins__3213E83F03E6C5F8");

            entity.ToTable("admins");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Admin)
                .HasForeignKey<Admin>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__admins__id__3414ACBA");
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__attendan__3213E83F16FB9E66");

            entity.ToTable("attendances");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.EnrollmentId).HasColumnName("enrollment_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.TimeslotId).HasColumnName("timeslot_id");

            entity.HasOne(d => d.Enrollment).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.EnrollmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__attendanc__enrol__161A357F");

            entity.HasOne(d => d.Timeslot).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.TimeslotId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__attendanc__times__170E59B8");
        });

        modelBuilder.Entity<BrandModel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__brand_mo__3213E83FE766448F");

            entity.ToTable("brand_models");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CountryOfOrigin)
                .HasMaxLength(100)
                .HasColumnName("country_of_origin");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Manufacturer)
                .HasMaxLength(200)
                .HasColumnName("manufacturer");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__certific__3213E83F73BA09F3");

            entity.ToTable("certificates");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.TemplateHtml).HasColumnName("template_html");
            entity.Property(e => e.TemplateUrl)
                .HasMaxLength(1000)
                .HasColumnName("template_url");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__classes__3213E83FC491A581");

            entity.ToTable("classes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BackgroundImageUrl)
                .HasMaxLength(256)
                .HasColumnName("background_image_url");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.ClassCodeId).HasColumnName("class_code_id");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.EndDate)
                .HasPrecision(0)
                .HasColumnName("end_date");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.ProgramCourseId).HasColumnName("program_course_id");
            entity.Property(e => e.StartDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");

            entity.HasOne(d => d.ClassCode).WithMany(p => p.Classes)
                .HasForeignKey(d => d.ClassCodeId)
                .HasConstraintName("FK__classes__class_c__286DEFE4");

            entity.HasOne(d => d.ProgramCourse).WithMany(p => p.Classes)
                .HasForeignKey(d => d.ProgramCourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__classes__program__2779CBAB");
        });

        modelBuilder.Entity<ClassCode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__class_co__3213E83F5A58BEAE");

            entity.ToTable("class_codes");

            entity.HasIndex(e => e.Name, "UQ__class_co__72E12F1BA6B4B9AC").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ClassInstructor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__class_in__3213E83FB8439A87");

            entity.ToTable("class_instructors");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.InstructorId).HasColumnName("instructor_id");
            entity.Property(e => e.Position)
                .HasMaxLength(1000)
                .HasColumnName("position");

            entity.HasOne(d => d.Class).WithMany(p => p.ClassInstructors)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__class_ins__class__2B4A5C8F");

            entity.HasOne(d => d.Instructor).WithMany(p => p.ClassInstructors)
                .HasForeignKey(d => d.InstructorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__class_ins__instr__2C3E80C8");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__courses__3213E83F4E458272");

            entity.ToTable("courses");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BackgroundImageUrl)
                .HasMaxLength(256)
                .HasColumnName("background_image_url");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CourseCodeId).HasColumnName("course_code_id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.DurationHours).HasColumnName("duration_hours");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(1000)
                .HasColumnName("image_url");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.LevelId).HasColumnName("level_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Category).WithMany(p => p.Courses)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__courses__categor__538D5813");

            entity.HasOne(d => d.CourseCode).WithMany(p => p.Courses)
                .HasForeignKey(d => d.CourseCodeId)
                .HasConstraintName("FK__courses__course___5575A085");

            entity.HasOne(d => d.Level).WithMany(p => p.Courses)
                .HasForeignKey(d => d.LevelId)
                .HasConstraintName("FK__courses__level_i__54817C4C");
        });

        modelBuilder.Entity<CourseCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__course_c__3213E83FB0CE257D");

            entity.ToTable("course_categories");

            entity.HasIndex(e => e.Name, "UQ__course_c__72E12F1BABA78332").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<CourseCertificate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__course_c__3213E83FB2BDD47E");

            entity.ToTable("course_certificates");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CertificateId).HasColumnName("certificate_id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.PassingScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("passing_score");

            entity.HasOne(d => d.Certificate).WithMany(p => p.CourseCertificates)
                .HasForeignKey(d => d.CertificateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__course_ce__certi__3B80C458");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseCertificates)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__course_ce__cours__3A8CA01F");
        });

        modelBuilder.Entity<CourseCode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__course_c__3213E83F33F5A9EF");

            entity.ToTable("course_codes");

            entity.HasIndex(e => e.Name, "UQ__course_c__72E12F1BE30B7F69").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<CourseLevel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__course_l__3213E83F108058E8");

            entity.ToTable("course_levels");

            entity.HasIndex(e => e.Name, "UQ__course_l__72E12F1BCB1A3FB0").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<CourseSection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__course_s__3214EC073030D98C");

            entity.ToTable("course_sections");

            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.SectionOrder).HasColumnName("section_order");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseSections)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__course_se__cours__63C3BFDC");

            entity.HasOne(d => d.Section).WithMany(p => p.CourseSections)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__course_se__secti__64B7E415");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__enrollme__3213E83F9E62DB1A");

            entity.ToTable("enrollments");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.EnrollDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("enroll_date");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Note)
                .HasMaxLength(500)
                .HasColumnName("note");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.TraineeId).HasColumnName("trainee_id");

            entity.HasOne(d => d.Class).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__enrollmen__class__32EB7E57");

            entity.HasOne(d => d.Trainee).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.TraineeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__enrollmen__train__33DFA290");
        });

        modelBuilder.Entity<FeSimulation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__fe_simul__3213E83FCB94973F");

            entity.ToTable("fe_simulation");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.FinalExamPartialId).HasColumnName("final_exam_partial_id");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.PracticeId).HasColumnName("practice_id");

            entity.HasOne(d => d.FinalExamPartial).WithMany(p => p.FeSimulations)
                .HasForeignKey(d => d.FinalExamPartialId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__fe_simula__final__2A212E2C");

            entity.HasOne(d => d.Practice).WithMany(p => p.FeSimulations)
                .HasForeignKey(d => d.PracticeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__fe_simula__pract__2B155265");
        });

        modelBuilder.Entity<FeTheory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__fe_theor__3213E83F533F3135");

            entity.ToTable("fe_theory");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.FinalExamPartialId).HasColumnName("final_exam_partial_id");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.QuizId).HasColumnName("quiz_id");

            entity.HasOne(d => d.FinalExamPartial).WithMany(p => p.FeTheories)
                .HasForeignKey(d => d.FinalExamPartialId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__fe_theory__final__26509D48");

            entity.HasOne(d => d.Quiz).WithMany(p => p.FeTheories)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__fe_theory__quiz___2744C181");
        });

        modelBuilder.Entity<FinalExam>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__final_ex__3213E83FD51A812D");

            entity.ToTable("final_exams");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CompleteTime)
                .HasPrecision(0)
                .HasColumnName("complete_time");
            entity.Property(e => e.EnrollmentId).HasColumnName("enrollment_id");
            entity.Property(e => e.ExamCode)
                .HasMaxLength(20)
                .HasColumnName("exam_code");
            entity.Property(e => e.IsPass)
                .HasDefaultValue(false)
                .HasColumnName("is_pass");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.TotalMarks)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("total_marks");

            entity.HasOne(d => d.Enrollment).WithMany(p => p.FinalExams)
                .HasForeignKey(d => d.EnrollmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__final_exa__enrol__1FA39FB9");
        });

        modelBuilder.Entity<FinalExamPartial>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__final_ex__3213E83F63C1C510");

            entity.ToTable("final_exam_partials");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CompleteTime)
                .HasPrecision(0)
                .HasColumnName("complete_time");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.EndTime)
                .HasPrecision(0)
                .HasColumnName("end_time");
            entity.Property(e => e.ExamWeight)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("exam_weight");
            entity.Property(e => e.FinalExamId).HasColumnName("final_exam_id");
            entity.Property(e => e.IsPass)
                .HasDefaultValue(false)
                .HasColumnName("is_pass");
            entity.Property(e => e.Marks)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("marks");
            entity.Property(e => e.StartTime)
                .HasPrecision(0)
                .HasColumnName("start_time");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TotalScore)
                .HasDefaultValue(10)
                .HasColumnName("total_score");
            entity.Property(e => e.Type).HasColumnName("type");

            entity.HasOne(d => d.FinalExam).WithMany(p => p.FinalExamPartials)
                .HasForeignKey(d => d.FinalExamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__final_exa__final__2374309D");
        });

        modelBuilder.Entity<FinalExamPartialsTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__final_ex__3213E83F97053949");

            entity.ToTable("final_exam_partials_templates");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FinalExamTemplateId).HasColumnName("final_exam_template_id");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.Weight)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("weight");

            entity.HasOne(d => d.FinalExamTemplate).WithMany(p => p.FinalExamPartialsTemplates)
                .HasForeignKey(d => d.FinalExamTemplateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_fe_partials_templates_exam_template");
        });

        modelBuilder.Entity<FinalExamTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__final_ex__3213E83F7361632A");

            entity.ToTable("final_exam_templates");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");

            entity.HasOne(d => d.Class).WithMany(p => p.FinalExamTemplates)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_final_exam_templates_classes");
        });

        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__instruct__3213E83FE810DB05");

            entity.ToTable("instructors");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.HireDate)
                .HasPrecision(0)
                .HasColumnName("hire_date");
            entity.Property(e => e.InstructorCode)
                .HasMaxLength(1000)
                .HasColumnName("instructor_code");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Instructor)
                .HasForeignKey<Instructor>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__instructors__id__3BB5CE82");
        });

        modelBuilder.Entity<InstructorFeedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__instruct__3213E83FCDE3F976");

            entity.ToTable("instructor_feedbacks");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActivityRecordId).HasColumnName("activity_record_id");
            entity.Property(e => e.CreatedDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_date");
            entity.Property(e => e.FeedbackText)
                .HasMaxLength(1000)
                .HasColumnName("feedback_text");
            entity.Property(e => e.InstructorId).HasColumnName("instructor_id");

            entity.HasOne(d => d.ActivityRecord).WithMany(p => p.InstructorFeedbacks)
                .HasForeignKey(d => d.ActivityRecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__instructo__activ__7795AE5F");
        });

        modelBuilder.Entity<InstructorProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__instruct__3213E83F6AC0AE47");

            entity.ToTable("instructor_profiles");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Biography)
                .HasMaxLength(1000)
                .HasColumnName("biography");
            entity.Property(e => e.ExperienceYears).HasColumnName("experience_years");
            entity.Property(e => e.ProfessionalProfileUrl)
                .HasMaxLength(1000)
                .HasColumnName("professional_profile_url");
            entity.Property(e => e.Specialization)
                .HasMaxLength(1000)
                .HasColumnName("specialization");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.InstructorProfile)
                .HasForeignKey<InstructorProfile>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__instructor_p__id__4356F04A");
        });

        modelBuilder.Entity<LearningMaterial>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__learning__3213E83F842F0FD0");

            entity.ToTable("learning_materials");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.LearningMaterialType)
                .HasDefaultValue(1)
                .HasColumnName("learning_material_type");
            entity.Property(e => e.MaterialUrl)
                .HasMaxLength(1000)
                .HasColumnName("material_url");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<LearningProgress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__learning__3213E83FEF76D7C0");

            entity.ToTable("learning_progresses");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.EnrollmentId).HasColumnName("enrollment_id");
            entity.Property(e => e.FinalScore)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("final_score");
            entity.Property(e => e.LastUpdated)
                .HasPrecision(0)
                .HasColumnName("last_updated");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PracticalScore)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("practical_score");
            entity.Property(e => e.ProgressPercentage)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("progress_percentage");
            entity.Property(e => e.StartDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.TheoryScore)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("theory_score");

            entity.HasOne(d => d.Enrollment).WithMany(p => p.LearningProgresses)
                .HasForeignKey(d => d.EnrollmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__learning___enrol__49CEE3AF");
        });

        modelBuilder.Entity<MaterialAuthor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__material__3213E83F1417AC77");

            entity.ToTable("material_authors");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.InstructorId).HasColumnName("instructor_id");
            entity.Property(e => e.MaterialId).HasColumnName("material_id");

            entity.HasOne(d => d.Instructor).WithMany(p => p.MaterialAuthors)
                .HasForeignKey(d => d.InstructorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__material___instr__02133CD2");

            entity.HasOne(d => d.Material).WithMany(p => p.MaterialAuthors)
                .HasForeignKey(d => d.MaterialId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__material___mater__0307610B");
        });

        modelBuilder.Entity<PeChecklist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__pe_check__3213E83FBC3C70A7");

            entity.ToTable("pe_checklist");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.FinalExamPartialId).HasColumnName("final_exam_partial_id");
            entity.Property(e => e.IsPass)
                .HasDefaultValue(false)
                .HasColumnName("is_pass");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");

            entity.HasOne(d => d.FinalExamPartial).WithMany(p => p.PeChecklists)
                .HasForeignKey(d => d.FinalExamPartialId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__pe_checkl__final__2FDA0782");
        });

        modelBuilder.Entity<Practice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__practice__3213E83FDA38D349");

            entity.ToTable("practices");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_date");
            entity.Property(e => e.DifficultyLevel)
                .HasMaxLength(1000)
                .HasColumnName("difficulty_level");
            entity.Property(e => e.EstimatedDurationMinutes).HasColumnName("estimated_duration_minutes");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.MaxAttempts).HasColumnName("max_attempts");
            entity.Property(e => e.PracticeCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("NO PRACTICE CODE ASSIGNED")
                .HasColumnName("practice_code");
            entity.Property(e => e.PracticeDescription)
                .HasMaxLength(1000)
                .HasColumnName("practice_description");
            entity.Property(e => e.PracticeName)
                .HasMaxLength(1000)
                .HasColumnName("practice_name");
        });

        modelBuilder.Entity<PracticeAttempt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__practice__3213E83FA0A1536A");

            entity.ToTable("practice_attempts");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActivityRecordId).HasColumnName("activity_record_id");
            entity.Property(e => e.AttemptDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("attempt_date");
            entity.Property(e => e.AttemptStatus)
                .HasDefaultValue(1)
                .HasColumnName("attempt_status");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.DurationSeconds).HasColumnName("duration_seconds");
            entity.Property(e => e.EndTime)
                .HasPrecision(0)
                .HasColumnName("end_time");
            entity.Property(e => e.IsCurrent).HasColumnName("is_current");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.IsPass)
                .HasDefaultValue(false)
                .HasColumnName("is_pass");
            entity.Property(e => e.PracticeId)
                .HasDefaultValue(-1)
                .HasColumnName("practice_id");
            entity.Property(e => e.Score)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("score");
            entity.Property(e => e.StartTime)
                .HasPrecision(0)
                .HasColumnName("start_time");
            entity.Property(e => e.TotalMistakes)
                .HasDefaultValue(0)
                .HasColumnName("total_mistakes");

            entity.HasOne(d => d.ActivityRecord).WithMany(p => p.PracticeAttempts)
                .HasForeignKey(d => d.ActivityRecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__practice___activ__5CE1B823");
        });

        modelBuilder.Entity<PracticeAttemptTask>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__practice__3213E83F980037AC");

            entity.ToTable("practice_attempt_tasks");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.IsPass)
                .HasDefaultValue(false)
                .HasColumnName("is_pass");
            entity.Property(e => e.Mistakes).HasColumnName("mistakes");
            entity.Property(e => e.PracticeAttemptId).HasColumnName("practice_attempt_id");
            entity.Property(e => e.Score)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("score");
            entity.Property(e => e.TaskId)
                .HasDefaultValue(-1)
                .HasColumnName("task_id");

            entity.HasOne(d => d.PracticeAttempt).WithMany(p => p.PracticeAttemptTasks)
                .HasForeignKey(d => d.PracticeAttemptId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__practice___pract__629A9179");
        });

        modelBuilder.Entity<PracticeTask>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__practice__3213E83F909D392A");

            entity.ToTable("practice_tasks");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PracticeId).HasColumnName("practice_id");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.TaskId).HasColumnName("task_id");

            entity.HasOne(d => d.Practice).WithMany(p => p.PracticeTasks)
                .HasForeignKey(d => d.PracticeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__practice___pract__0EAE1DE1");

            entity.HasOne(d => d.Task).WithMany(p => p.PracticeTasks)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__practice___task___0FA2421A");
        });

        modelBuilder.Entity<ProgramCourse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__program___3213E83F69A3D3D6");

            entity.ToTable("program_courses");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.CourseOrder).HasColumnName("course_order");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.ProgramId).HasColumnName("program_id");

            entity.HasOne(d => d.Course).WithMany(p => p.ProgramCourses)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__program_c__cours__5E0AE686");

            entity.HasOne(d => d.Program).WithMany(p => p.ProgramCourses)
                .HasForeignKey(d => d.ProgramId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__program_c__progr__5D16C24D");
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__quizzes__3213E83F5C0B5C80");

            entity.ToTable("quizzes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.MaxAttempts)
                .HasDefaultValue(1)
                .HasColumnName("max_attempts");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PassScoreCriteria)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("pass_score_criteria");
            entity.Property(e => e.TimelimitMinute).HasColumnName("timelimit_minute");
            entity.Property(e => e.TotalScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("total_score");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<QuizAttempt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__quiz_att__3213E83F7492972E");

            entity.ToTable("quiz_attempts");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActivityRecordId).HasColumnName("activity_record_id");
            entity.Property(e => e.AttemptOrder).HasColumnName("attempt_order");
            entity.Property(e => e.AttemptScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("attempt_score");
            entity.Property(e => e.IsCurrent).HasColumnName("is_current");
            entity.Property(e => e.IsPass).HasColumnName("is_pass");
            entity.Property(e => e.MaxScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("max_score");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.QuizAttemptDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("quiz_attempt_date");
            entity.Property(e => e.QuizId)
                .HasDefaultValue(-1)
                .HasColumnName("quiz_id");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");

            entity.HasOne(d => d.ActivityRecord).WithMany(p => p.QuizAttempts)
                .HasForeignKey(d => d.ActivityRecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__quiz_atte__activ__68536ACF");
        });

        modelBuilder.Entity<QuizAttemptAnswer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__quiz_att__3213E83F84B58B46");

            entity.ToTable("quiz_attempt_answers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttemptScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("attempt_score");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.IsCorrect)
                .HasDefaultValue(true)
                .HasColumnName("is_correct");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.QuizAttemptQuestionId).HasColumnName("quiz_attempt_question_id");
            entity.Property(e => e.QuizOptionId)
                .HasDefaultValue(-1)
                .HasColumnName("quiz_option_id");

            entity.HasOne(d => d.QuizAttemptQuestion).WithMany(p => p.QuizAttemptAnswers)
                .HasForeignKey(d => d.QuizAttemptQuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__quiz_atte__quiz___72D0F942");
        });

        modelBuilder.Entity<QuizAttemptQuestion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__quiz_att__3213E83F394BB86B");

            entity.ToTable("quiz_attempt_questions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttemptScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("attempt_score");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.IsCorrect)
                .HasDefaultValue(true)
                .HasColumnName("is_correct");
            entity.Property(e => e.IsMultipleAnswers)
                .HasDefaultValue(true)
                .HasColumnName("is_multiple_answers");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.QuestionId)
                .HasDefaultValue(-1)
                .HasColumnName("question_id");
            entity.Property(e => e.QuestionScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("question_score");
            entity.Property(e => e.QuizAttemptId).HasColumnName("quiz_attempt_id");

            entity.HasOne(d => d.QuizAttempt).WithMany(p => p.QuizAttemptQuestions)
                .HasForeignKey(d => d.QuizAttemptId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__quiz_atte__quiz___6E0C4425");
        });

        modelBuilder.Entity<QuizAuthor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__quiz_aut__3213E83FACE10714");

            entity.ToTable("quiz_authors");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.InstructorId).HasColumnName("instructor_id");
            entity.Property(e => e.QuizId).HasColumnName("quiz_id");

            entity.HasOne(d => d.Instructor).WithMany(p => p.QuizAuthors)
                .HasForeignKey(d => d.InstructorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__quiz_auth__instr__7E42ABEE");

            entity.HasOne(d => d.Quiz).WithMany(p => p.QuizAuthors)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__quiz_auth__quiz___7F36D027");
        });

        modelBuilder.Entity<QuizQuestion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__quiz_que__3213E83F14457C70");

            entity.ToTable("quiz_questions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .HasColumnName("image_url");
            entity.Property(e => e.IsMultipleAnswers)
                .HasDefaultValue(true)
                .HasColumnName("is_multiple_answers");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.QuestionScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("question_score");
            entity.Property(e => e.QuizId).HasColumnName("quiz_id");

            entity.HasOne(d => d.Quiz).WithMany(p => p.QuizQuestions)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__quiz_ques__quiz___7AA72534");
        });

        modelBuilder.Entity<QuizQuestionOption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__quiz_que__3213E83F1D5113CA");

            entity.ToTable("quiz_question_options");

            entity.HasIndex(e => e.DisplayOrder, "UQ__quiz_que__1B16645E64534A96").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
            entity.Property(e => e.Explanation)
                .HasMaxLength(1000)
                .HasColumnName("explanation");
            entity.Property(e => e.IsCorrect)
                .HasDefaultValue(true)
                .HasColumnName("is_correct");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.OptionScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("option_score");
            entity.Property(e => e.QuizQuestionId).HasColumnName("quiz_question_id");

            entity.HasOne(d => d.QuizQuestion).WithMany(p => p.QuizQuestionOptions)
                .HasForeignKey(d => d.QuizQuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__quiz_ques__quiz___7F6BDA51");
        });

        modelBuilder.Entity<SeTask>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__se_tasks__3213E83FA67B61C8");

            entity.ToTable("se_tasks");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttemptTime)
                .HasPrecision(0)
                .HasColumnName("attempt_time");
            entity.Property(e => e.CompleteTime)
                .HasPrecision(0)
                .HasColumnName("complete_time");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.DurationSecond).HasColumnName("duration_second");
            entity.Property(e => e.FeSimulationId).HasColumnName("fe_simulation_id");
            entity.Property(e => e.IsPass)
                .HasDefaultValue(false)
                .HasColumnName("is_pass");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.SimTaskId).HasColumnName("sim_task_id");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");

            entity.HasOne(d => d.FeSimulation).WithMany(p => p.SeTasks)
                .HasForeignKey(d => d.FeSimulationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__se_tasks__fe_sim__349EBC9F");

            entity.HasOne(d => d.SimTask).WithMany(p => p.SeTasks)
                .HasForeignKey(d => d.SimTaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__se_tasks__sim_ta__3592E0D8");
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__sections__3214EC0786C7FDE1");

            entity.ToTable("sections");

            entity.Property(e => e.EstimatedDurationMinutes).HasColumnName("estimated_duration_minutes");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.SectionDescription)
                .HasMaxLength(1000)
                .HasColumnName("section_description");
            entity.Property(e => e.SectionTitle)
                .HasMaxLength(200)
                .HasColumnName("section_title");
        });

        modelBuilder.Entity<SectionActivity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83FBD52BA1D");

            entity.ToTable("section_activities");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActivityId).HasColumnName("activity_id");
            entity.Property(e => e.ActivityOrder).HasColumnName("activity_order");
            entity.Property(e => e.SectionId).HasColumnName("section_id");

            entity.HasOne(d => d.Activity).WithMany(p => p.SectionActivities)
                .HasForeignKey(d => d.ActivityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_a__activ__6F357288");

            entity.HasOne(d => d.Section).WithMany(p => p.SectionActivities)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_a__secti__6E414E4F");
        });

        modelBuilder.Entity<SectionRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83F77EFC1B1");

            entity.ToTable("section_records");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DurationMinutes)
                .HasDefaultValue(20)
                .HasColumnName("duration_minutes");
            entity.Property(e => e.IsCompleted)
                .HasDefaultValue(true)
                .HasColumnName("is_completed");
            entity.Property(e => e.IsTraineeAttended)
                .HasDefaultValue(true)
                .HasColumnName("is_trainee_attended");
            entity.Property(e => e.LearningProgressId).HasColumnName("learning_progress_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Progress)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("progress");
            entity.Property(e => e.SectionId)
                .HasDefaultValue(-1)
                .HasColumnName("section_id");
            entity.Property(e => e.SectionName)
                .HasMaxLength(255)
                .HasColumnName("section_name");

            entity.HasOne(d => d.LearningProgress).WithMany(p => p.SectionRecords)
                .HasForeignKey(d => d.LearningProgressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_r__learn__4F87BD05");
        });

        modelBuilder.Entity<SimSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__sim_sett__3213E83F7D1BDE8E");

            entity.ToTable("sim_settings");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_date");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(1000)
                .HasColumnName("image_url");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.SettingCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("SETTING_UNKNOWN")
                .HasColumnName("setting_code");
        });

        modelBuilder.Entity<SimTask>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__sim_task__3213E83F391099E1");

            entity.ToTable("sim_tasks");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ExpectedResult)
                .HasMaxLength(1000)
                .HasColumnName("expected_result");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.TaskCode)
                .HasMaxLength(50)
                .HasDefaultValue("NO PRACTICE CODE ASSIGNED")
                .HasColumnName("task_code");
            entity.Property(e => e.TaskDescription)
                .HasMaxLength(1000)
                .HasColumnName("task_description");
            entity.Property(e => e.TaskName)
                .HasMaxLength(1000)
                .HasColumnName("task_name");
        });

        modelBuilder.Entity<SimulationComponent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__simulati__3213E83F8F4168E0");

            entity.ToTable("simulation_components");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BrandModelId).HasColumnName("brand_model_id");
            entity.Property(e => e.ComponentCode)
                .HasMaxLength(50)
                .HasDefaultValue("COMPONENT_UNKNOWN")
                .HasColumnName("component_code");
            entity.Property(e => e.CreatedDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_date");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(1000)
                .HasColumnName("image_url");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");

            entity.HasOne(d => d.BrandModel).WithMany(p => p.SimulationComponents)
                .HasForeignKey(d => d.BrandModelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__simulatio__brand__1FD8A9E3");
        });

        modelBuilder.Entity<SimulationManager>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__simulati__3213E83F3445C7F2");

            entity.ToTable("simulation_managers");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.SimulationManager)
                .HasForeignKey<SimulationManager>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__simulation_m__id__36F11965");
        });

        modelBuilder.Entity<Timeslot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__timeslot__3213E83F8B5C84B1");

            entity.ToTable("timeslots");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.EndTime)
                .HasPrecision(0)
                .HasColumnName("end_time");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.LocationBuilding)
                .HasMaxLength(200)
                .HasColumnName("location_building");
            entity.Property(e => e.LocationDetail)
                .HasMaxLength(1000)
                .HasColumnName("location_detail");
            entity.Property(e => e.LocationRoom)
                .HasMaxLength(100)
                .HasColumnName("location_room");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.StartTime)
                .HasPrecision(0)
                .HasColumnName("start_time");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");

            entity.HasOne(d => d.Class).WithMany(p => p.Timeslots)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__timeslots__class__10615C29");
        });

        modelBuilder.Entity<Trainee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__trainees__3213E83FC48FDD83");

            entity.ToTable("trainees");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.TraineeCode)
                .HasMaxLength(1000)
                .HasColumnName("trainee_code");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Trainee)
                .HasForeignKey<Trainee>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__trainees__id__407A839F");
        });

        modelBuilder.Entity<TraineeCertificate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__trainee___3213E83F8C9B51B3");

            entity.ToTable("trainee_certificates");

            entity.HasIndex(e => e.CertificateCode, "UQ__trainee___2283DB56A286AD43").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CertificateCode)
                .HasMaxLength(100)
                .HasColumnName("certificate_code");
            entity.Property(e => e.CourseCertificateId).HasColumnName("course_certificate_id");
            entity.Property(e => e.EnrollmentId).HasColumnName("enrollment_id");
            entity.Property(e => e.IssuedDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("issued_date");
            entity.Property(e => e.PdfUrl)
                .HasMaxLength(1000)
                .HasColumnName("pdf_url");

            entity.HasOne(d => d.CourseCertificate).WithMany(p => p.TraineeCertificates)
                .HasForeignKey(d => d.CourseCertificateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__trainee_c__cours__41399DAE");

            entity.HasOne(d => d.Enrollment).WithMany(p => p.TraineeCertificates)
                .HasForeignKey(d => d.EnrollmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__trainee_c__enrol__40457975");
        });

        modelBuilder.Entity<TraineeProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__trainee___3213E83F47828283");

            entity.ToTable("trainee_profiles");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CitizenCardId)
                .HasMaxLength(20)
                .HasColumnName("citizen_card_id");
            entity.Property(e => e.CitizenCardImageUrl)
                .HasMaxLength(1000)
                .HasColumnName("citizen_card_image_url");
            entity.Property(e => e.CitizenCardIssuedDate).HasColumnName("citizen_card_issued_date");
            entity.Property(e => e.CitizenCardPlaceOfIssue)
                .HasMaxLength(255)
                .HasColumnName("citizen_card_place_of_issue");
            entity.Property(e => e.DriverLicenseImageUrl)
                .HasMaxLength(1000)
                .HasColumnName("driver_license_image_url");
            entity.Property(e => e.DriverLicenseIssuedDate)
                .HasPrecision(0)
                .HasColumnName("driver_license_issued_date");
            entity.Property(e => e.DriverLicenseLevel)
                .HasMaxLength(50)
                .HasColumnName("driver_license_level");
            entity.Property(e => e.DriverLicenseNumber)
                .HasMaxLength(100)
                .HasColumnName("driver_license_number");
            entity.Property(e => e.DriverLicenseValidEndDate)
                .HasPrecision(0)
                .HasColumnName("driver_license_valid_end_date");
            entity.Property(e => e.DriverLicenseValidStartDate)
                .HasPrecision(0)
                .HasColumnName("driver_license_valid_start_date");
            entity.Property(e => e.EducationImageUrl)
                .HasMaxLength(1000)
                .HasColumnName("education_image_url");
            entity.Property(e => e.EducationLevel)
                .HasMaxLength(255)
                .HasColumnName("education_level");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.TraineeProfile)
                .HasForeignKey<TraineeProfile>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__trainee_prof__id__46335CF5");
        });

        modelBuilder.Entity<TrainingProgram>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__training__3213E83FD3EE38BD");

            entity.ToTable("training_programs");

            entity.HasIndex(e => e.Name, "UQ__training__72E12F1B91F47C6B").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BackgroundImageUrl)
                .HasMaxLength(256)
                .HasColumnName("background_image_url");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.DurationHours).HasColumnName("duration_hours");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(1000)
                .HasColumnName("image_url");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.TotalCourses).HasColumnName("total_courses");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__users__3213E83F1DAE5E5B");

            entity.ToTable("users");

            entity.HasIndex(e => e.Username, "UQ__users__F3DBC572BDA2D8C9").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(1000)
                .HasColumnName("avatar_url");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(1000)
                .HasColumnName("fullname");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.Password)
                .HasMaxLength(1000)
                .HasColumnName("password");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(1000)
                .HasColumnName("phone_number");
            entity.Property(e => e.Role)
                .HasDefaultValue(5)
                .HasColumnName("role");
            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
