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

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Certificate> Certificates { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<ClassCode> ClassCodes { get; set; }

    public virtual DbSet<ClassInstructor> ClassInstructors { get; set; }

    public virtual DbSet<ClassMember> ClassMembers { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CourseCategory> CourseCategories { get; set; }

    public virtual DbSet<CourseCode> CourseCodes { get; set; }

    public virtual DbSet<CourseLevel> CourseLevels { get; set; }

    public virtual DbSet<Instructor> Instructors { get; set; }

    public virtual DbSet<InstructorProfile> InstructorProfiles { get; set; }

    public virtual DbSet<LearningMaterial> LearningMaterials { get; set; }

    public virtual DbSet<LearningMaterialType> LearningMaterialTypes { get; set; }

    public virtual DbSet<LearningRecord> LearningRecords { get; set; }

    public virtual DbSet<LearningRecordPartition> LearningRecordPartitions { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentTransaction> PaymentTransactions { get; set; }

    public virtual DbSet<Practice> Practices { get; set; }

    public virtual DbSet<PracticeStep> PracticeSteps { get; set; }

    public virtual DbSet<PracticeStepComponent> PracticeStepComponents { get; set; }

    public virtual DbSet<PracticeStepType> PracticeStepTypes { get; set; }

    public virtual DbSet<PracticeStepWarning> PracticeStepWarnings { get; set; }

    public virtual DbSet<PracticeStepWarningType> PracticeStepWarningTypes { get; set; }

    public virtual DbSet<ProgramCertificate> ProgramCertificates { get; set; }

    public virtual DbSet<ProgramCourse> ProgramCourses { get; set; }

    public virtual DbSet<ProgramManager> ProgramManagers { get; set; }

    public virtual DbSet<ProgramPrerequisite> ProgramPrerequisites { get; set; }

    public virtual DbSet<Quiz> Quizzes { get; set; }

    public virtual DbSet<QuizQuestion> QuizQuestions { get; set; }

    public virtual DbSet<QuizQuestionOption> QuizQuestionOptions { get; set; }

    public virtual DbSet<Section> Sections { get; set; }

    public virtual DbSet<SectionMaterial> SectionMaterials { get; set; }

    public virtual DbSet<SectionPartition> SectionPartitions { get; set; }

    public virtual DbSet<SectionPartitionType> SectionPartitionTypes { get; set; }

    public virtual DbSet<SectionPractice> SectionPractices { get; set; }

    public virtual DbSet<SectionPracticeAttempt> SectionPracticeAttempts { get; set; }

    public virtual DbSet<SectionPracticeAttemptStep> SectionPracticeAttemptSteps { get; set; }

    public virtual DbSet<SectionQuiz> SectionQuizzes { get; set; }

    public virtual DbSet<SectionQuizAttempt> SectionQuizAttempts { get; set; }

    public virtual DbSet<SectionQuizAttemptAnswer> SectionQuizAttemptAnswers { get; set; }

    public virtual DbSet<SectionQuizAttemptQuestion> SectionQuizAttemptQuestions { get; set; }

    public virtual DbSet<SimulationComponent> SimulationComponents { get; set; }

    public virtual DbSet<SimulationComponentType> SimulationComponentTypes { get; set; }

    public virtual DbSet<SimulationManager> SimulationManagers { get; set; }

    public virtual DbSet<SimulationSetting> SimulationSettings { get; set; }

    public virtual DbSet<SimulationTimeslot> SimulationTimeslots { get; set; }

    public virtual DbSet<Trainee> Trainees { get; set; }

    public virtual DbSet<TraineeCertificate> TraineeCertificates { get; set; }

    public virtual DbSet<TrainingProgram> TrainingPrograms { get; set; }

    public virtual DbSet<TrainingProgress> TrainingProgresses { get; set; }

    public virtual DbSet<TrainingResult> TrainingResults { get; set; }

    public virtual DbSet<TrainingResultType> TrainingResultTypes { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<TransactionProgram> TransactionPrograms { get; set; }

    public virtual DbSet<User> Users { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=localhost;Database=lssctc-db-test;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__admins__3213E83F8FFF99B6");

            entity.ToTable("admins");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Admin)
                .HasForeignKey<Admin>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__admins__id__3C69FB99");
        });

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__certific__3213E83F7F2D303E");

            entity.ToTable("certificates");

            entity.HasIndex(e => e.Name, "UQ__certific__72E12F1BBC4E741D").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CertifyingAuthority)
                .HasMaxLength(2000)
                .HasColumnName("certifying_authority");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.EffectiveTime).HasColumnName("effective_time");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(2000)
                .HasColumnName("image_url");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Requirement)
                .HasMaxLength(2000)
                .HasColumnName("requirement");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__classes__3213E83F8638671B");

            entity.ToTable("classes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.ClassCodeId).HasColumnName("class_code_id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
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
                .HasConstraintName("FK__classes__class_c__10566F31");

            entity.HasOne(d => d.ProgramCourse).WithMany(p => p.Classes)
                .HasForeignKey(d => d.ProgramCourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__classes__program__0F624AF8");
        });

        modelBuilder.Entity<ClassCode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__class_co__3213E83F56E7E87A");

            entity.ToTable("class_codes");

            entity.HasIndex(e => e.Name, "UQ__class_co__72E12F1B3461B4E2").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ClassInstructor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__class_in__3213E83F3A230984");

            entity.ToTable("class_instructors");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.InstructorId).HasColumnName("instructor_id");
            entity.Property(e => e.Position)
                .HasMaxLength(2000)
                .HasColumnName("position");

            entity.HasOne(d => d.Class).WithMany(p => p.ClassInstructors)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__class_ins__class__1332DBDC");

            entity.HasOne(d => d.Instructor).WithMany(p => p.ClassInstructors)
                .HasForeignKey(d => d.InstructorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__class_ins__instr__14270015");
        });

        modelBuilder.Entity<ClassMember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__class_me__3213E83F5B8A9861");

            entity.ToTable("class_members");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AssignedDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("assigned_date");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.TraineeId).HasColumnName("trainee_id");

            entity.HasOne(d => d.Class).WithMany(p => p.ClassMembers)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__class_mem__class__19DFD96B");

            entity.HasOne(d => d.Trainee).WithMany(p => p.ClassMembers)
                .HasForeignKey(d => d.TraineeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__class_mem__train__18EBB532");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__courses__3213E83F4E3EC844");

            entity.ToTable("courses");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CourseCodeId).HasColumnName("course_code_id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.DurationHours).HasColumnName("duration_hours");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(2000)
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

            entity.HasOne(d => d.Category).WithMany(p => p.Courses)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__courses__categor__7D439ABD");

            entity.HasOne(d => d.CourseCode).WithMany(p => p.Courses)
                .HasForeignKey(d => d.CourseCodeId)
                .HasConstraintName("FK__courses__course___7F2BE32F");

            entity.HasOne(d => d.Level).WithMany(p => p.Courses)
                .HasForeignKey(d => d.LevelId)
                .HasConstraintName("FK__courses__level_i__7E37BEF6");
        });

        modelBuilder.Entity<CourseCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__course_c__3213E83FAED5C14A");

            entity.ToTable("course_categories");

            entity.HasIndex(e => e.Name, "UQ__course_c__72E12F1B9CC4AE61").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<CourseCode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__course_c__3213E83F3EF605F5");

            entity.ToTable("course_codes");

            entity.HasIndex(e => e.Name, "UQ__course_c__72E12F1B0F8BFE2C").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<CourseLevel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__course_l__3213E83F3BE1B036");

            entity.ToTable("course_levels");

            entity.HasIndex(e => e.Name, "UQ__course_l__72E12F1BB90E099B").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__instruct__3213E83FC50F8CE1");

            entity.ToTable("instructors");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.HireDate)
                .HasPrecision(0)
                .HasColumnName("hire_date");
            entity.Property(e => e.InstructorCode)
                .HasMaxLength(2000)
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
                .HasConstraintName("FK__instructors__id__46E78A0C");
        });

        modelBuilder.Entity<InstructorProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__instruct__3213E83F07BC7A06");

            entity.ToTable("instructor_profiles");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Biography)
                .HasMaxLength(2000)
                .HasColumnName("biography");
            entity.Property(e => e.ExperienceYears).HasColumnName("experience_years");
            entity.Property(e => e.ProfessionalProfileUrl)
                .HasMaxLength(2000)
                .HasColumnName("professional_profile_url");
            entity.Property(e => e.Specialization)
                .HasMaxLength(2000)
                .HasColumnName("specialization");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.InstructorProfile)
                .HasForeignKey<InstructorProfile>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__instructor_p__id__4E88ABD4");
        });

        modelBuilder.Entity<LearningMaterial>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__learning__3213E83F273F8029");

            entity.ToTable("learning_materials");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.LearningMaterialTypeId).HasColumnName("learning_material_type_id");
            entity.Property(e => e.MaterialUrl)
                .HasMaxLength(2000)
                .HasColumnName("material_url");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");

            entity.HasOne(d => d.LearningMaterialType).WithMany(p => p.LearningMaterials)
                .HasForeignKey(d => d.LearningMaterialTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__learning___learn__5EBF139D");
        });

        modelBuilder.Entity<LearningMaterialType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__learning__3213E83F317CE1EC");

            entity.ToTable("learning_material_types");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<LearningRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__learning__3213E83F5C3C6D60");

            entity.ToTable("learning_records");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClassMemberId).HasColumnName("class_member_id");
            entity.Property(e => e.IsCompleted)
                .HasDefaultValue(true)
                .HasColumnName("is_completed");
            entity.Property(e => e.IsTraineeAttended)
                .HasDefaultValue(true)
                .HasColumnName("is_trainee_attended");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Progress)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("progress");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.SectionName)
                .HasMaxLength(255)
                .HasColumnName("section_name");

            entity.HasOne(d => d.ClassMember).WithMany(p => p.LearningRecords)
                .HasForeignKey(d => d.ClassMemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__learning___class__3E1D39E1");

            entity.HasOne(d => d.Section).WithMany(p => p.LearningRecords)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__learning___secti__3D2915A8");
        });

        modelBuilder.Entity<LearningRecordPartition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__learning__3213E83F072BDD87");

            entity.ToTable("learning_record_partitions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CompletedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("completed_at");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.IsComplete)
                .HasDefaultValue(true)
                .HasColumnName("is_complete");
            entity.Property(e => e.LearningRecordId).HasColumnName("learning_record_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.RecordPartitionOrder).HasColumnName("record_partition_order");
            entity.Property(e => e.SectionPartitionId).HasColumnName("section_partition_id");
            entity.Property(e => e.StartedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("started_at");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.LearningRecord).WithMany(p => p.LearningRecordPartitions)
                .HasForeignKey(d => d.LearningRecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__learning___learn__45BE5BA9");

            entity.HasOne(d => d.SectionPartition).WithMany(p => p.LearningRecordPartitions)
                .HasForeignKey(d => d.SectionPartitionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__learning___secti__44CA3770");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__payments__3213E83FB49BADF4");

            entity.ToTable("payments");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .HasDefaultValue("VND")
                .HasColumnName("currency");
            entity.Property(e => e.Note)
                .HasMaxLength(2000)
                .HasColumnName("note");
            entity.Property(e => e.PaymentDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("payment_date");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasColumnName("payment_method");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.TraineeId).HasColumnName("trainee_id");

            entity.HasOne(d => d.Trainee).WithMany(p => p.Payments)
                .HasForeignKey(d => d.TraineeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__payments__traine__17C286CF");
        });

        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__payment___3213E83FCCF7C72E");

            entity.ToTable("payment_transactions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");

            entity.HasOne(d => d.Payment).WithMany(p => p.PaymentTransactions)
                .HasForeignKey(d => d.PaymentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__payment_t__payme__214BF109");

            entity.HasOne(d => d.Transaction).WithMany(p => p.PaymentTransactions)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__payment_t__trans__22401542");
        });

        modelBuilder.Entity<Practice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__practice__3213E83F7119C4A9");

            entity.ToTable("practices");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_date");
            entity.Property(e => e.DifficultyLevel)
                .HasMaxLength(2000)
                .HasColumnName("difficulty_level");
            entity.Property(e => e.EstimatedDurationMinutes).HasColumnName("estimated_duration_minutes");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.MaxAttempts).HasColumnName("max_attempts");
            entity.Property(e => e.PracticeDescription)
                .HasMaxLength(2000)
                .HasColumnName("practice_description");
            entity.Property(e => e.PracticeName)
                .HasMaxLength(2000)
                .HasColumnName("practice_name");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
        });

        modelBuilder.Entity<PracticeStep>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__practice__3213E83F98AF5083");

            entity.ToTable("practice_steps");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ExpectedResult)
                .HasMaxLength(2000)
                .HasColumnName("expected_result");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.PracticeId).HasColumnName("practice_id");
            entity.Property(e => e.StepDescription)
                .HasMaxLength(2000)
                .HasColumnName("step_description");
            entity.Property(e => e.StepName)
                .HasMaxLength(2000)
                .HasColumnName("step_name");
            entity.Property(e => e.StepOrder).HasColumnName("step_order");

            entity.HasOne(d => d.Practice).WithMany(p => p.PracticeSteps)
                .HasForeignKey(d => d.PracticeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__practice___pract__5224328E");
        });

        modelBuilder.Entity<PracticeStepComponent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__practice__3213E83F5BB1367D");

            entity.ToTable("practice_step_components");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ComponentId).HasColumnName("component_id");
            entity.Property(e => e.ComponentOrder).HasColumnName("component_order");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.StepId).HasColumnName("step_id");

            entity.HasOne(d => d.Component).WithMany(p => p.PracticeStepComponents)
                .HasForeignKey(d => d.ComponentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__practice___compo__5E8A0973");

            entity.HasOne(d => d.Step).WithMany(p => p.PracticeStepComponents)
                .HasForeignKey(d => d.StepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__practice___step___5D95E53A");
        });

        modelBuilder.Entity<PracticeStepType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__practice__3213E83F0D21B4D1");

            entity.ToTable("practice_step_types");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<PracticeStepWarning>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__practice__3213E83F16E46B4B");

            entity.ToTable("practice_step_warnings");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Instruction)
                .HasMaxLength(2000)
                .HasColumnName("instruction");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.PracticeStepId).HasColumnName("practice_step_id");
            entity.Property(e => e.WarningMessage)
                .HasMaxLength(2000)
                .HasColumnName("warning_message");
            entity.Property(e => e.WarningTypeId).HasColumnName("warning_type_id");

            entity.HasOne(d => d.PracticeStep).WithMany(p => p.PracticeStepWarnings)
                .HasForeignKey(d => d.PracticeStepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__practice___pract__58D1301D");

            entity.HasOne(d => d.WarningType).WithMany(p => p.PracticeStepWarnings)
                .HasForeignKey(d => d.WarningTypeId)
                .HasConstraintName("FK__practice___warni__59C55456");
        });

        modelBuilder.Entity<PracticeStepWarningType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__practice__3213E83F159E125D");

            entity.ToTable("practice_step_warning_types");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.IsFailedCriteria)
                .HasDefaultValue(false)
                .HasColumnName("is_failed_criteria");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ProgramCertificate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__program___3213E83FB4A754F7");

            entity.ToTable("program_certificates");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CertificateId).HasColumnName("certificate_id");
            entity.Property(e => e.ProgramId).HasColumnName("program_id");

            entity.HasOne(d => d.Certificate).WithMany(p => p.ProgramCertificates)
                .HasForeignKey(d => d.CertificateId)
                .HasConstraintName("FK__program_c__certi__7FEAFD3E");

            entity.HasOne(d => d.Program).WithMany(p => p.ProgramCertificates)
                .HasForeignKey(d => d.ProgramId)
                .HasConstraintName("FK__program_c__progr__7EF6D905");
        });

        modelBuilder.Entity<ProgramCourse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__program___3213E83FB9EE65CC");

            entity.ToTable("program_courses");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CourseOrder).HasColumnName("course_order");
            entity.Property(e => e.CoursesId).HasColumnName("courses_id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.ProgramId).HasColumnName("program_id");

            entity.HasOne(d => d.Courses).WithMany(p => p.ProgramCourses)
                .HasForeignKey(d => d.CoursesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__program_c__cours__0A9D95DB");

            entity.HasOne(d => d.Program).WithMany(p => p.ProgramCourses)
                .HasForeignKey(d => d.ProgramId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__program_c__progr__09A971A2");
        });

        modelBuilder.Entity<ProgramManager>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__program___3213E83F9269CBAF");

            entity.ToTable("program_managers");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.ProgramManager)
                .HasForeignKey<ProgramManager>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__program_mana__id__4222D4EF");
        });

        modelBuilder.Entity<ProgramPrerequisite>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__program___3213E83F3002FC69");

            entity.ToTable("program_prerequisites");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.ProgramId).HasColumnName("program_id");

            entity.HasOne(d => d.Program).WithMany(p => p.ProgramPrerequisites)
                .HasForeignKey(d => d.ProgramId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__program_p__progr__06CD04F7");
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__quizzes__3213E83F9F17C8CB");

            entity.ToTable("quizzes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
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

        modelBuilder.Entity<QuizQuestion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__quiz_que__3213E83F85303C9F");

            entity.ToTable("quiz_questions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
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
                .HasConstraintName("FK__quiz_ques__quiz___66603565");
        });

        modelBuilder.Entity<QuizQuestionOption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__quiz_que__3213E83FB3EE785E");

            entity.ToTable("quiz_question_options");

            entity.HasIndex(e => e.DisplayOrder, "UQ__quiz_que__1B16645E366FF10E").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
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
                .HasConstraintName("FK__quiz_ques__quiz___6B24EA82");
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__sections__3213E83FFF94E2B4");

            entity.ToTable("sections");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClassesId).HasColumnName("classes_id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.EndDate)
                .HasPrecision(0)
                .HasColumnName("end_date");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Order).HasColumnName("order");
            entity.Property(e => e.StartDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");

            entity.HasOne(d => d.Classes).WithMany(p => p.Sections)
                .HasForeignKey(d => d.ClassesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__sections__classe__1EA48E88");
        });

        modelBuilder.Entity<SectionMaterial>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83FAE8584D9");

            entity.ToTable("section_materials");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.LearningMaterialId).HasColumnName("learning_material_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.SectionPartitionId).HasColumnName("section_partition_id");

            entity.HasOne(d => d.LearningMaterial).WithMany(p => p.SectionMaterials)
                .HasForeignKey(d => d.LearningMaterialId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_m__learn__2645B050");

            entity.HasOne(d => d.SectionPartition).WithMany(p => p.SectionMaterials)
                .HasForeignKey(d => d.SectionPartitionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_m__secti__25518C17");
        });

        modelBuilder.Entity<SectionPartition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83F949F45C9");

            entity.ToTable("section_partitions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PartitionTypeId).HasColumnName("partition_type_id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");

            entity.HasOne(d => d.PartitionType).WithMany(p => p.SectionPartitions)
                .HasForeignKey(d => d.PartitionTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_p__parti__2180FB33");

            entity.HasOne(d => d.Section).WithMany(p => p.SectionPartitions)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_p__secti__22751F6C");
        });

        modelBuilder.Entity<SectionPartitionType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83FBEF0F445");

            entity.ToTable("section_partition_types");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.IsActionRequired).HasColumnName("is_action_required");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PassCriteria)
                .HasMaxLength(2000)
                .HasColumnName("pass_criteria");
        });

        modelBuilder.Entity<SectionPractice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83F3ACC37AB");

            entity.ToTable("section_practices");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CustomDeadline)
                .HasPrecision(0)
                .HasColumnName("custom_deadline");
            entity.Property(e => e.CustomDescription)
                .HasMaxLength(2000)
                .HasColumnName("custom_description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.PracticeId).HasColumnName("practice_id");
            entity.Property(e => e.SectionPartitionId).HasColumnName("section_partition_id");
            entity.Property(e => e.SimulationTimeslotId).HasColumnName("simulation_timeslot_id");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");

            entity.HasOne(d => d.Practice).WithMany(p => p.SectionPractices)
                .HasForeignKey(d => d.PracticeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_p__pract__6AEFE058");

            entity.HasOne(d => d.SectionPartition).WithMany(p => p.SectionPractices)
                .HasForeignKey(d => d.SectionPartitionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_p__secti__69FBBC1F");

            entity.HasOne(d => d.SimulationTimeslot).WithMany(p => p.SectionPractices)
                .HasForeignKey(d => d.SimulationTimeslotId)
                .HasConstraintName("FK__section_p__simul__6BE40491");
        });

        modelBuilder.Entity<SectionPracticeAttempt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83F24349AD5");

            entity.ToTable("section_practice_attempts");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttemptDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("attempt_date");
            entity.Property(e => e.AttemptStatus)
                .HasDefaultValue(1)
                .HasColumnName("attempt_status");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.IsPass)
                .HasDefaultValue(false)
                .HasColumnName("is_pass");
            entity.Property(e => e.Score)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("score");
            entity.Property(e => e.SectionPracticeId).HasColumnName("section_practice_id");
            entity.Property(e => e.TraineeId).HasColumnName("trainee_id");

            entity.HasOne(d => d.SectionPractice).WithMany(p => p.SectionPracticeAttempts)
                .HasForeignKey(d => d.SectionPracticeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_p__secti__72910220");

            entity.HasOne(d => d.Trainee).WithMany(p => p.SectionPracticeAttempts)
                .HasForeignKey(d => d.TraineeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_p__train__73852659");
        });

        modelBuilder.Entity<SectionPracticeAttemptStep>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83FE71370B1");

            entity.ToTable("section_practice_attempt_steps");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttemptId).HasColumnName("attempt_id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.IsPass)
                .HasDefaultValue(false)
                .HasColumnName("is_pass");
            entity.Property(e => e.PracticeStepId).HasColumnName("practice_step_id");
            entity.Property(e => e.Score)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("score");

            entity.HasOne(d => d.Attempt).WithMany(p => p.SectionPracticeAttemptSteps)
                .HasForeignKey(d => d.AttemptId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_p__attem__7849DB76");

            entity.HasOne(d => d.PracticeStep).WithMany(p => p.SectionPracticeAttemptSteps)
                .HasForeignKey(d => d.PracticeStepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_p__pract__793DFFAF");
        });

        modelBuilder.Entity<SectionQuiz>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83F5A1AC0E2");

            entity.ToTable("section_quizzes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.QuizId).HasColumnName("quiz_id");
            entity.Property(e => e.SectionPartitionId).HasColumnName("section_partition_id");

            entity.HasOne(d => d.Quiz).WithMany(p => p.SectionQuizzes)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_q__quiz___29221CFB");

            entity.HasOne(d => d.SectionPartition).WithMany(p => p.SectionQuizzes)
                .HasForeignKey(d => d.SectionPartitionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_q__secti__2A164134");
        });

        modelBuilder.Entity<SectionQuizAttempt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83F1EA925A2");

            entity.ToTable("section_quiz_attempts");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttemptOrder).HasColumnName("attempt_order");
            entity.Property(e => e.AttemptScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("attempt_score");
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
            entity.Property(e => e.SectionQuizId).HasColumnName("section_quiz_id");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.TraineeId).HasColumnName("trainee_id");

            entity.HasOne(d => d.SectionQuiz).WithMany(p => p.SectionQuizAttempts)
                .HasForeignKey(d => d.SectionQuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_q__secti__2EDAF651");

            entity.HasOne(d => d.Trainee).WithMany(p => p.SectionQuizAttempts)
                .HasForeignKey(d => d.TraineeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_q__train__2FCF1A8A");
        });

        modelBuilder.Entity<SectionQuizAttemptAnswer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83F1B05F503");

            entity.ToTable("section_quiz_attempt_answers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttemptScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("attempt_score");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.IsCorrect)
                .HasDefaultValue(true)
                .HasColumnName("is_correct");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.SectionQuizAttemptQuestionId).HasColumnName("section_quiz_attempt_question_id");

            entity.HasOne(d => d.SectionQuizAttemptQuestion).WithMany(p => p.SectionQuizAttemptAnswers)
                .HasForeignKey(d => d.SectionQuizAttemptQuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_q__secti__3864608B");
        });

        modelBuilder.Entity<SectionQuizAttemptQuestion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section___3213E83F7BB55F79");

            entity.ToTable("section_quiz_attempt_questions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttemptScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("attempt_score");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
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
            entity.Property(e => e.QuestionScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("question_score");
            entity.Property(e => e.SectionQuizAttemptId).HasColumnName("section_quiz_attempt_id");

            entity.HasOne(d => d.SectionQuizAttempt).WithMany(p => p.SectionQuizAttemptQuestions)
                .HasForeignKey(d => d.SectionQuizAttemptId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__section_q__secti__3493CFA7");
        });

        modelBuilder.Entity<SimulationComponent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__simulati__3213E83F7ACBE48A");

            entity.ToTable("simulation_components");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_date");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(2000)
                .HasColumnName("image_url");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
        });

        modelBuilder.Entity<SimulationComponentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__simulati__3213E83FAB4CA112");

            entity.ToTable("simulation_component_types");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<SimulationManager>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__simulati__3213E83F9465E20C");

            entity.ToTable("simulation_managers");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.SimulationManager)
                .HasForeignKey<SimulationManager>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__simulation_m__id__3F466844");
        });

        modelBuilder.Entity<SimulationSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__simulati__3213E83F13AD68B6");

            entity.ToTable("simulation_settings");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<SimulationTimeslot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__simulati__3213E83F10D4F3E0");

            entity.ToTable("simulation_timeslots");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EndTime)
                .HasPrecision(0)
                .HasColumnName("end_time");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Note)
                .HasMaxLength(2000)
                .HasColumnName("note");
            entity.Property(e => e.SectionPartitionId).HasColumnName("section_partition_id");
            entity.Property(e => e.StartTime)
                .HasPrecision(0)
                .HasColumnName("start_time");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");

            entity.HasOne(d => d.SectionPartition).WithMany(p => p.SimulationTimeslots)
                .HasForeignKey(d => d.SectionPartitionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__simulatio__secti__6442E2C9");
        });

        modelBuilder.Entity<Trainee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__trainees__3213E83F26DDC7A4");

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
                .HasMaxLength(2000)
                .HasColumnName("trainee_code");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Trainee)
                .HasForeignKey<Trainee>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__trainees__id__4BAC3F29");
        });

        modelBuilder.Entity<TraineeCertificate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__trainee___3213E83FB131CF95");

            entity.ToTable("trainee_certificates");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CertificateId).HasColumnName("certificate_id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.IssuedDateEnd)
                .HasPrecision(0)
                .HasColumnName("issued_date_end");
            entity.Property(e => e.IssuedDateStart)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("issued_date_start");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.TraineeId).HasColumnName("trainee_id");
            entity.Property(e => e.ValidDateEnd)
                .HasPrecision(0)
                .HasColumnName("valid_date_end");

            entity.HasOne(d => d.Certificate).WithMany(p => p.TraineeCertificates)
                .HasForeignKey(d => d.CertificateId)
                .HasConstraintName("FK__trainee_c__certi__04AFB25B");

            entity.HasOne(d => d.Trainee).WithMany(p => p.TraineeCertificates)
                .HasForeignKey(d => d.TraineeId)
                .HasConstraintName("FK__trainee_c__train__05A3D694");
        });

        modelBuilder.Entity<TrainingProgram>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__training__3213E83F03BD6E98");

            entity.ToTable("training_programs");

            entity.HasIndex(e => e.Name, "UQ__training__72E12F1BB11F2254").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.DurationHours).HasColumnName("duration_hours");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(2000)
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
        });

        modelBuilder.Entity<TrainingProgress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__training__3213E83F16C8C2BD");

            entity.ToTable("training_progresses");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CourseMemberId).HasColumnName("course_member_id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.LastUpdated)
                .HasPrecision(0)
                .HasColumnName("last_updated");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.ProgressPercentage).HasColumnName("progress_percentage");
            entity.Property(e => e.StartDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.TrainingResultId).HasColumnName("training_result_id");

            entity.HasOne(d => d.CourseMember).WithMany(p => p.TrainingProgresses)
                .HasForeignKey(d => d.CourseMemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__training___cours__11158940");

            entity.HasOne(d => d.TrainingResult).WithMany(p => p.TrainingProgresses)
                .HasForeignKey(d => d.TrainingResultId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__training___train__1209AD79");
        });

        modelBuilder.Entity<TrainingResult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__training__3213E83F7A60C83F");

            entity.ToTable("training_results");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Notes)
                .HasMaxLength(2000)
                .HasColumnName("notes");
            entity.Property(e => e.ResultDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("result_date");
            entity.Property(e => e.ResultValue)
                .HasMaxLength(2000)
                .HasColumnName("result_value");
            entity.Property(e => e.TraineeId).HasColumnName("trainee_id");
            entity.Property(e => e.TrainingResultTypeId).HasColumnName("training_result_type_id");

            entity.HasOne(d => d.Trainee).WithMany(p => p.TrainingResults)
                .HasForeignKey(d => d.TraineeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__training___train__0C50D423");

            entity.HasOne(d => d.TrainingResultType).WithMany(p => p.TrainingResults)
                .HasForeignKey(d => d.TrainingResultTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__training___train__0B5CAFEA");
        });

        modelBuilder.Entity<TrainingResultType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__training__3213E83FDA9334F4");

            entity.ToTable("training_result_types");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__transact__3213E83F9A6EC14B");

            entity.ToTable("transactions");

            entity.HasIndex(e => e.TransactionCode, "UQ__transact__DD5740BED9F431B8").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .HasDefaultValue("VND")
                .HasColumnName("currency");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .HasColumnName("description");
            entity.Property(e => e.IssuedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("issued_at");
            entity.Property(e => e.Message)
                .HasMaxLength(2000)
                .HasColumnName("message");
            entity.Property(e => e.Note)
                .HasMaxLength(2000)
                .HasColumnName("note");
            entity.Property(e => e.PaidAt)
                .HasPrecision(0)
                .HasColumnName("paid_at");
            entity.Property(e => e.PayerId).HasColumnName("payer_id");
            entity.Property(e => e.PayerName)
                .HasMaxLength(2000)
                .HasColumnName("payer_name");
            entity.Property(e => e.ReceiverId).HasColumnName("receiver_id");
            entity.Property(e => e.ReceiverName)
                .HasMaxLength(2000)
                .HasColumnName("receiver_name");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.TransactionCode)
                .HasMaxLength(255)
                .HasColumnName("transaction_code");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(50)
                .HasColumnName("transaction_type");
        });

        modelBuilder.Entity<TransactionProgram>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__transact__3213E83F924EB944");

            entity.ToTable("transaction_programs");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProgramId).HasColumnName("program_id");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");

            entity.HasOne(d => d.Program).WithMany(p => p.TransactionPrograms)
                .HasForeignKey(d => d.ProgramId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__transacti__progr__2610A626");

            entity.HasOne(d => d.Transaction).WithMany(p => p.TransactionPrograms)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__transacti__trans__251C81ED");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__users__3213E83FFF94FC1A");

            entity.ToTable("users");

            entity.HasIndex(e => e.Username, "UQ__users__F3DBC572A82C91F6").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(2000)
                .HasColumnName("avatar_url");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(2000)
                .HasColumnName("fullname");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.Password)
                .HasMaxLength(2000)
                .HasColumnName("password");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(2000)
                .HasColumnName("phone_number");
            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
