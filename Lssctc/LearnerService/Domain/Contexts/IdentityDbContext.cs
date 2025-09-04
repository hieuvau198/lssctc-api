using System;
using System.Collections.Generic;
using LearnerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LearnerService.Domain.Contexts;

public partial class IdentityDbContext : DbContext
{
    public IdentityDbContext()
    {
    }

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CourseCategory> CourseCategories { get; set; }

    public virtual DbSet<CourseDefinition> CourseDefinitions { get; set; }

    public virtual DbSet<CourseEnrollment> CourseEnrollments { get; set; }

    public virtual DbSet<CourseLevel> CourseLevels { get; set; }

    public virtual DbSet<CourseSession> CourseSessions { get; set; }

    public virtual DbSet<Instructor> Instructors { get; set; }

    public virtual DbSet<Learner> Learners { get; set; }

    public virtual DbSet<LearnerQuestionAnswer> LearnerQuestionAnswers { get; set; }

    public virtual DbSet<LearnerSimulationTask> LearnerSimulationTasks { get; set; }

    public virtual DbSet<LearnerTest> LearnerTests { get; set; }

    public virtual DbSet<LearnerTestQuestion> LearnerTestQuestions { get; set; }

    public virtual DbSet<LearningMaterial> LearningMaterials { get; set; }

    public virtual DbSet<MaterialType> MaterialTypes { get; set; }

    public virtual DbSet<QuestionOption> QuestionOptions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SessionAttendance> SessionAttendances { get; set; }

    public virtual DbSet<SessionLearner> SessionLearners { get; set; }

    public virtual DbSet<SessionSchedule> SessionSchedules { get; set; }

    public virtual DbSet<SessionType> SessionTypes { get; set; }

    public virtual DbSet<SimulationTask> SimulationTasks { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<Test> Tests { get; set; }

    public virtual DbSet<TestQuestion> TestQuestions { get; set; }

    public virtual DbSet<TrainingSession> TrainingSessions { get; set; }

    public virtual DbSet<TrainingSessionSimulationTask> TrainingSessionSimulationTasks { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=tcp:hieuvauserver.database.windows.net,1433;Initial Catalog=lssctc_db;Persist Security Info=False;User ID=hieuvau198;Password=ouneerasmE1@;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Latin1_General_100_CI_AS_SC_UTF8");

        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__admins__B9BE370F47BF029B");

            entity.ToTable("admins");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");

            entity.HasOne(d => d.User).WithOne(p => p.Admin)
                .HasForeignKey<Admin>(d => d.UserId)
                .HasConstraintName("FK__admins__user_id__55BFB948");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__courses__3213E83FE08E5B51");

            entity.ToTable("courses");

            entity.HasIndex(e => e.StartDate, "IX_courses_start_date");

            entity.HasIndex(e => e.Status, "IX_courses_status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .HasColumnName("category");
            entity.Property(e => e.CourseCode)
                .HasMaxLength(20)
                .HasColumnName("course_code");
            entity.Property(e => e.CourseDefinitionId).HasColumnName("course_definition_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Level)
                .HasMaxLength(50)
                .HasColumnName("level");
            entity.Property(e => e.Location)
                .HasMaxLength(200)
                .HasColumnName("location");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("planned")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");

            entity.HasOne(d => d.CourseDefinition).WithMany(p => p.Courses)
                .HasForeignKey(d => d.CourseDefinitionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__courses__course___67DE6983");

            entity.HasMany(d => d.Courses).WithMany(p => p.Prerequisites)
                .UsingEntity<Dictionary<string, object>>(
                    "CoursePrerequisite",
                    r => r.HasOne<Course>().WithMany()
                        .HasForeignKey("CourseId")
                        .HasConstraintName("FK__course_pr__cours__6BAEFA67"),
                    l => l.HasOne<Course>().WithMany()
                        .HasForeignKey("PrerequisiteId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__course_pr__prere__6CA31EA0"),
                    j =>
                    {
                        j.HasKey("CourseId", "PrerequisiteId").HasName("PK__course_p__75D16F3E6B62DDC1");
                        j.ToTable("course_prerequisites");
                        j.IndexerProperty<int>("CourseId").HasColumnName("course_id");
                        j.IndexerProperty<int>("PrerequisiteId").HasColumnName("prerequisite_id");
                    });

            entity.HasMany(d => d.Materials).WithMany(p => p.Courses)
                .UsingEntity<Dictionary<string, object>>(
                    "CourseMaterial",
                    r => r.HasOne<LearningMaterial>().WithMany()
                        .HasForeignKey("MaterialId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__course_ma__mater__075714DC"),
                    l => l.HasOne<Course>().WithMany()
                        .HasForeignKey("CourseId")
                        .HasConstraintName("FK__course_ma__cours__0662F0A3"),
                    j =>
                    {
                        j.HasKey("CourseId", "MaterialId").HasName("PK__course_m__19A1167C3D9D1D46");
                        j.ToTable("course_materials");
                        j.IndexerProperty<int>("CourseId").HasColumnName("course_id");
                        j.IndexerProperty<int>("MaterialId").HasColumnName("material_id");
                    });

            entity.HasMany(d => d.Prerequisites).WithMany(p => p.Courses)
                .UsingEntity<Dictionary<string, object>>(
                    "CoursePrerequisite",
                    r => r.HasOne<Course>().WithMany()
                        .HasForeignKey("PrerequisiteId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__course_pr__prere__6CA31EA0"),
                    l => l.HasOne<Course>().WithMany()
                        .HasForeignKey("CourseId")
                        .HasConstraintName("FK__course_pr__cours__6BAEFA67"),
                    j =>
                    {
                        j.HasKey("CourseId", "PrerequisiteId").HasName("PK__course_p__75D16F3E6B62DDC1");
                        j.ToTable("course_prerequisites");
                        j.IndexerProperty<int>("CourseId").HasColumnName("course_id");
                        j.IndexerProperty<int>("PrerequisiteId").HasColumnName("prerequisite_id");
                    });

            entity.HasMany(d => d.Users).WithMany(p => p.Courses)
                .UsingEntity<Dictionary<string, object>>(
                    "CourseInstructor",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__course_in__user___7167D3BD"),
                    l => l.HasOne<Course>().WithMany()
                        .HasForeignKey("CourseId")
                        .HasConstraintName("FK__course_in__cours__7073AF84"),
                    j =>
                    {
                        j.HasKey("CourseId", "UserId").HasName("PK__course_i__648514DE440A7972");
                        j.ToTable("course_instructors");
                        j.IndexerProperty<int>("CourseId").HasColumnName("course_id");
                        j.IndexerProperty<int>("UserId").HasColumnName("user_id");
                    });

            entity.HasMany(d => d.UsersNavigation).WithMany(p => p.CoursesNavigation)
                .UsingEntity<Dictionary<string, object>>(
                    "CourseLearner",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__course_le__user___753864A1"),
                    l => l.HasOne<Course>().WithMany()
                        .HasForeignKey("CourseId")
                        .HasConstraintName("FK__course_le__cours__74444068"),
                    j =>
                    {
                        j.HasKey("CourseId", "UserId").HasName("PK__course_l__648514DE89C0B3DF");
                        j.ToTable("course_learners");
                        j.IndexerProperty<int>("CourseId").HasColumnName("course_id");
                        j.IndexerProperty<int>("UserId").HasColumnName("user_id");
                    });
        });

        modelBuilder.Entity<CourseCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__course_c__3213E83F56B2DFD1");

            entity.ToTable("course_categories");

            entity.HasIndex(e => e.Name, "UQ__course_c__72E12F1BDFAF82D6").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<CourseDefinition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__course_d__3213E83F54EF4A99");

            entity.ToTable("course_definitions");

            entity.HasIndex(e => e.CourseCode, "UQ__course_d__AB6B45F1D4D11300").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CourseCode)
                .HasMaxLength(20)
                .HasColumnName("course_code");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.LevelId).HasColumnName("level_id");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");

            entity.HasOne(d => d.Category).WithMany(p => p.CourseDefinitions)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__course_de__categ__61316BF4");

            entity.HasOne(d => d.Level).WithMany(p => p.CourseDefinitions)
                .HasForeignKey(d => d.LevelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__course_de__level__6225902D");
        });

        modelBuilder.Entity<CourseEnrollment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__course_e__3213E83FA6774FFD");

            entity.ToTable("course_enrollments");

            entity.HasIndex(e => e.Status, "IX_course_enrollments_status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.EnrollmentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("enrollment_date");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("enrolled")
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseEnrollments)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__course_en__cours__7BE56230");

            entity.HasOne(d => d.User).WithMany(p => p.CourseEnrollments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__course_en__user___7CD98669");
        });

        modelBuilder.Entity<CourseLevel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__course_l__3213E83FFD836C6A");

            entity.ToTable("course_levels");

            entity.HasIndex(e => e.Name, "UQ__course_l__72E12F1B58E1398D").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<CourseSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__course_s__3213E83F5A6FAAC6");

            entity.ToTable("course_sessions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.TrainingSessionId).HasColumnName("training_session_id");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseSessions)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__course_se__cours__15A53433");

            entity.HasOne(d => d.TrainingSession).WithMany(p => p.CourseSessions)
                .HasForeignKey(d => d.TrainingSessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__course_se__train__1699586C");
        });

        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__instruct__B9BE370FBCA4DEAF");

            entity.ToTable("instructors");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.Bio)
                .HasMaxLength(1)
                .HasColumnName("bio");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Specialization)
                .HasMaxLength(100)
                .HasColumnName("specialization");
            entity.Property(e => e.YearsExperience)
                .HasDefaultValue(0)
                .HasColumnName("years_experience");

            entity.HasOne(d => d.User).WithOne(p => p.Instructor)
                .HasForeignKey<Instructor>(d => d.UserId)
                .HasConstraintName("FK__instructo__user___4C364F0E");
        });

        modelBuilder.Entity<Learner>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__learners__B9BE370FFDEA9141");

            entity.ToTable("learners");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.EnrollmentStatus)
                .HasMaxLength(20)
                .HasDefaultValue("active")
                .HasColumnName("enrollment_status");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");

            entity.HasOne(d => d.User).WithOne(p => p.Learner)
                .HasForeignKey<Learner>(d => d.UserId)
                .HasConstraintName("FK__learners__user_i__467D75B8");
        });

        modelBuilder.Entity<LearnerQuestionAnswer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__learner___3213E83F327DB6CE");

            entity.ToTable("learner_question_answers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AnswerText).HasColumnName("answer_text");
            entity.Property(e => e.IsCorrect)
                .HasDefaultValue(false)
                .HasColumnName("is_correct");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.LearnerTestQuestionId).HasColumnName("learner_test_question_id");
            entity.Property(e => e.OptionId).HasColumnName("option_id");
            entity.Property(e => e.Points)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("points");

            entity.HasOne(d => d.LearnerTestQuestion).WithMany(p => p.LearnerQuestionAnswers)
                .HasForeignKey(d => d.LearnerTestQuestionId)
                .HasConstraintName("FK__learner_q__learn__5F141958");

            entity.HasOne(d => d.Option).WithMany(p => p.LearnerQuestionAnswers)
                .HasForeignKey(d => d.OptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__learner_q__optio__60083D91");
        });

        modelBuilder.Entity<LearnerSimulationTask>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__learner___3213E83F337C3A3A");

            entity.ToTable("learner_simulation_tasks");

            entity.HasIndex(e => e.Status, "IX_learner_simulation_tasks_status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.Feedback).HasColumnName("feedback");
            entity.Property(e => e.LearnerId).HasColumnName("learner_id");
            entity.Property(e => e.Progress)
                .HasMaxLength(200)
                .HasColumnName("progress");
            entity.Property(e => e.StartedAt).HasColumnName("started_at");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("not_started")
                .HasColumnName("status");
            entity.Property(e => e.TrainingSessionSimulationTaskId).HasColumnName("training_session_simulation_task_id");

            entity.HasOne(d => d.Learner).WithMany(p => p.LearnerSimulationTasks)
                .HasForeignKey(d => d.LearnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__learner_s__learn__6E565CE8");

            entity.HasOne(d => d.TrainingSessionSimulationTask).WithMany(p => p.LearnerSimulationTasks)
                .HasForeignKey(d => d.TrainingSessionSimulationTaskId)
                .HasConstraintName("FK__learner_s__train__6F4A8121");
        });

        modelBuilder.Entity<LearnerTest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__learner___3213E83FC5E8D8D3");

            entity.ToTable("learner_tests");

            entity.HasIndex(e => e.StartedAt, "IX_learner_tests_started_at");

            entity.HasIndex(e => e.Status, "IX_learner_tests_status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttemptNumber)
                .HasDefaultValue(1)
                .HasColumnName("attempt_number");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.LearnerId).HasColumnName("learner_id");
            entity.Property(e => e.Review).HasColumnName("review");
            entity.Property(e => e.Score)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("score");
            entity.Property(e => e.StartedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("started_at");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("started")
                .HasColumnName("status");
            entity.Property(e => e.TestId).HasColumnName("test_id");

            entity.HasOne(d => d.Learner).WithMany(p => p.LearnerTests)
                .HasForeignKey(d => d.LearnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__learner_t__learn__4EDDB18F");

            entity.HasOne(d => d.Test).WithMany(p => p.LearnerTests)
                .HasForeignKey(d => d.TestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__learner_t__test___4FD1D5C8");
        });

        modelBuilder.Entity<LearnerTestQuestion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__learner___3213E83FAC78C482");

            entity.ToTable("learner_test_questions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.LearnerTestId).HasColumnName("learner_test_id");
            entity.Property(e => e.OrderIndex).HasColumnName("order_index");
            entity.Property(e => e.Points)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("points");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");

            entity.HasOne(d => d.LearnerTest).WithMany(p => p.LearnerTestQuestions)
                .HasForeignKey(d => d.LearnerTestId)
                .HasConstraintName("FK__learner_t__learn__5772F790");

            entity.HasOne(d => d.Question).WithMany(p => p.LearnerTestQuestions)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__learner_t__quest__58671BC9");
        });

        modelBuilder.Entity<LearningMaterial>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__learning__3213E83F968B8392");

            entity.ToTable("learning_materials");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.MaterialTypeId).HasColumnName("material_type_id");
            entity.Property(e => e.SourceUrl)
                .HasMaxLength(500)
                .HasColumnName("source_url");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");

            entity.HasOne(d => d.MaterialType).WithMany(p => p.LearningMaterials)
                .HasForeignKey(d => d.MaterialTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__learning___mater__038683F8");
        });

        modelBuilder.Entity<MaterialType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__material__3213E83FC6CC8785");

            entity.ToTable("material_types");

            entity.HasIndex(e => e.Name, "UQ__material__72E12F1B392977D6").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<QuestionOption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__question__3213E83FD17EA431");

            entity.ToTable("question_options");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IsCorrect)
                .HasDefaultValue(false)
                .HasColumnName("is_correct");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.OrderIndex).HasColumnName("order_index");
            entity.Property(e => e.Points)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("points");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");

            entity.HasOne(d => d.Question).WithMany(p => p.QuestionOptions)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK__question___quest__4460231C");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__roles__3213E83F16FA15D1");

            entity.ToTable("roles");

            entity.HasIndex(e => e.Name, "UQ__roles__72E12F1B5F97B744").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(20)
                .HasColumnName("name");
        });

        modelBuilder.Entity<SessionAttendance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__session___3213E83FD1C2CA95");

            entity.ToTable("session_attendances");

            entity.HasIndex(e => e.AttendanceStatus, "IX_session_attendances_status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttendanceStatus)
                .HasMaxLength(10)
                .HasDefaultValue("Present")
                .HasColumnName("attendance_status");
            entity.Property(e => e.AttendedAt).HasColumnName("attended_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.LearnerId).HasColumnName("learner_id");
            entity.Property(e => e.TrainingsessionId).HasColumnName("trainingsession_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Learner).WithMany(p => p.SessionAttendances)
                .HasForeignKey(d => d.LearnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__session_a__learn__23F3538A");

            entity.HasOne(d => d.Trainingsession).WithMany(p => p.SessionAttendances)
                .HasForeignKey(d => d.TrainingsessionId)
                .HasConstraintName("FK__session_a__train__22FF2F51");
        });

        modelBuilder.Entity<SessionLearner>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__session___3213E83F57860795");

            entity.ToTable("session_learners");

            entity.HasIndex(e => e.EnrollmentStatus, "IX_session_learners_status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.EnrollmentStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Applied")
                .HasColumnName("enrollment_status");
            entity.Property(e => e.LearnerId).HasColumnName("learner_id");
            entity.Property(e => e.TrainingsessionId).HasColumnName("trainingsession_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Learner).WithMany(p => p.SessionLearners)
                .HasForeignKey(d => d.LearnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__session_l__learn__2B947552");

            entity.HasOne(d => d.Trainingsession).WithMany(p => p.SessionLearners)
                .HasForeignKey(d => d.TrainingsessionId)
                .HasConstraintName("FK__session_l__train__2AA05119");
        });

        modelBuilder.Entity<SessionSchedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__session___3213E83F87F6A377");

            entity.ToTable("session_schedules");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.ScheduleDate).HasColumnName("schedule_date");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.TrainingsessionId).HasColumnName("trainingsession_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Trainingsession).WithMany(p => p.SessionSchedules)
                .HasForeignKey(d => d.TrainingsessionId)
                .HasConstraintName("FK__session_s__train__1B5E0D89");
        });

        modelBuilder.Entity<SessionType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__session___3213E83FC4D2B71D");

            entity.ToTable("session_types");

            entity.HasIndex(e => e.Name, "UQ__session___72E12F1BE37E613C").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<SimulationTask>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__simulati__3213E83FB4B23B1E");

            entity.ToTable("simulation_tasks");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.PassingCriteria).HasColumnName("passing_criteria");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Course).WithMany(p => p.SimulationTasks)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__simulatio__cours__64CCF2AE");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__staff__B9BE370F7B49825B");

            entity.ToTable("staff");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.EmploymentStatus)
                .HasMaxLength(20)
                .HasDefaultValue("active")
                .HasColumnName("employment_status");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");

            entity.HasOne(d => d.User).WithOne(p => p.Staff)
                .HasForeignKey<Staff>(d => d.UserId)
                .HasConstraintName("FK__staff__user_id__51EF2864");
        });

        modelBuilder.Entity<Test>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tests__3213E83F6345408A");

            entity.ToTable("tests");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.TotalPoints)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("total_points");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<TestQuestion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__test_que__3213E83F0B213C02");

            entity.ToTable("test_questions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AnswerQuantity).HasColumnName("answer_quantity");
            entity.Property(e => e.Explanation).HasColumnName("explanation");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.IsMultipleAnswers)
                .HasDefaultValue(false)
                .HasColumnName("is_multiple_answers");
            entity.Property(e => e.OptionQuantity).HasColumnName("option_quantity");
            entity.Property(e => e.Points)
                .HasDefaultValue(1m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("points");
            entity.Property(e => e.TestId).HasColumnName("test_id");

            entity.HasOne(d => d.Test).WithMany(p => p.TestQuestions)
                .HasForeignKey(d => d.TestId)
                .HasConstraintName("FK__test_ques__test___3BCADD1B");
        });

        modelBuilder.Entity<TrainingSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__training__3213E83FCF7DCB99");

            entity.ToTable("training_sessions");

            entity.HasIndex(e => e.StartDate, "IX_training_sessions_start_date");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.InstructorId).HasColumnName("instructor_id");
            entity.Property(e => e.Location)
                .HasMaxLength(200)
                .HasColumnName("location");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.SessionTypeId).HasColumnName("session_type_id");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Instructor).WithMany(p => p.TrainingSessions)
                .HasForeignKey(d => d.InstructorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__training___instr__11D4A34F");

            entity.HasOne(d => d.SessionType).WithMany(p => p.TrainingSessions)
                .HasForeignKey(d => d.SessionTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__training___sessi__10E07F16");
        });

        modelBuilder.Entity<TrainingSessionSimulationTask>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__training__3213E83F9BF6D704");

            entity.ToTable("training_session_simulation_tasks");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("assigned_at");
            entity.Property(e => e.SimulationTaskId).HasColumnName("simulation_task_id");
            entity.Property(e => e.TrainingSessionId).HasColumnName("training_session_id");

            entity.HasOne(d => d.SimulationTask).WithMany(p => p.TrainingSessionSimulationTasks)
                .HasForeignKey(d => d.SimulationTaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__training___simul__6991A7CB");

            entity.HasOne(d => d.TrainingSession).WithMany(p => p.TrainingSessionSimulationTasks)
                .HasForeignKey(d => d.TrainingSessionId)
                .HasConstraintName("FK__training___train__689D8392");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__users__3213E83FC5675BFC");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "IX_users_email");

            entity.HasIndex(e => e.IsDeleted, "IX_users_is_deleted");

            entity.HasIndex(e => e.RoleId, "IX_users_role_id");

            entity.HasIndex(e => e.Email, "UQ__users__AB6E61649437509A").IsUnique();

            entity.HasIndex(e => e.Username, "UQ__users__F3DBC57213C4B61D").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(100)
                .HasColumnName("fullname");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.ProfileImageUrl)
                .HasMaxLength(500)
                .HasColumnName("profile_image_url");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_users_roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
