-- Create database
CREATE DATABASE [lssctc_db];
GO
USE [lssctc_db];
GO

-- Create tables
CREATE TABLE [roles](
    [id] tinyint NOT NULL PRIMARY KEY,
    [name] nvarchar(20) NOT NULL UNIQUE
);

CREATE TABLE [users](
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [username] nvarchar(50) NOT NULL UNIQUE,
    [password] nvarchar(255) NOT NULL,
    [email] nvarchar(100) NOT NULL UNIQUE,
    [fullname] nvarchar(100) NOT NULL,
    [profile_image_url] nvarchar(500) NULL,
    [role_id] tinyint NOT NULL,
    [created_at] datetime2(7) NULL DEFAULT (getdate()),
    [updated_at] datetime2(7) NULL DEFAULT (getdate()),
    [is_deleted] bit NULL DEFAULT (0),
    FOREIGN KEY([role_id]) REFERENCES [roles]([id])
);

CREATE TABLE [admins](
    [user_id] int NOT NULL PRIMARY KEY,
    [is_deleted] bit NULL DEFAULT (0),
    FOREIGN KEY([user_id]) REFERENCES [users]([id]) ON DELETE CASCADE
);

CREATE TABLE [instructors](
    [user_id] int NOT NULL PRIMARY KEY,
    [bio] nvarchar(1) NULL,
    [specialization] nvarchar(100) NULL,
    [years_experience] int NULL DEFAULT (0),
    [is_deleted] bit NULL DEFAULT (0),
    FOREIGN KEY([user_id]) REFERENCES [users]([id]) ON DELETE CASCADE,
    CHECK ([years_experience] >= 0)
);

CREATE TABLE [learners](
    [user_id] int NOT NULL PRIMARY KEY,
    [date_of_birth] date NULL,
    [enrollment_status] nvarchar(20) NULL DEFAULT ('active'),
    [is_deleted] bit NULL DEFAULT (0),
    FOREIGN KEY([user_id]) REFERENCES [users]([id]) ON DELETE CASCADE,
    CHECK ([enrollment_status] IN ('active', 'inactive', 'suspended'))
);

CREATE TABLE [staff](
    [user_id] int NOT NULL PRIMARY KEY,
    [employment_status] nvarchar(20) NULL DEFAULT ('active'),
    [is_deleted] bit NULL DEFAULT (0),
    FOREIGN KEY([user_id]) REFERENCES [users]([id]) ON DELETE CASCADE,
    CHECK ([employment_status] IN ('active', 'inactive', 'on_leave'))
);

CREATE TABLE [course_categories](
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [name] nvarchar(100) NOT NULL UNIQUE
);

CREATE TABLE [course_levels](
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [name] nvarchar(50) NOT NULL UNIQUE
);

CREATE TABLE [course_definitions](
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [title] nvarchar(200) NOT NULL,
    [course_code] nvarchar(20) NOT NULL UNIQUE,
    [description] nvarchar(max) NULL,
    [category_id] int NOT NULL,
    [level_id] int NOT NULL,
    [duration] int NULL,
    [is_deleted] bit NULL DEFAULT (0),
    FOREIGN KEY([category_id]) REFERENCES [course_categories]([id]),
    FOREIGN KEY([level_id]) REFERENCES [course_levels]([id]),
    CHECK ([duration] > 0)
);

CREATE TABLE [courses](
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [course_definition_id] int NOT NULL,
    [title] nvarchar(200) NOT NULL,
    [course_code] nvarchar(20) NOT NULL,
    [description] nvarchar(max) NULL,
    [category] nvarchar(100) NOT NULL,
    [level] nvarchar(50) NOT NULL,
    [start_date] date NULL,
    [end_date] date NULL,
    [location] nvarchar(200) NULL,
    [status] nvarchar(20) NULL DEFAULT ('planned'),
    [is_deleted] bit NULL DEFAULT (0),
    FOREIGN KEY([course_definition_id]) REFERENCES [course_definitions]([id]),
    CHECK ([end_date] >= [start_date] OR [end_date] IS NULL),
    CHECK ([status] IN ('planned', 'ongoing', 'completed', 'cancelled'))
);

CREATE TABLE [course_enrollments](
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [course_id] int NOT NULL,
    [user_id] int NOT NULL,
    [enrollment_date] datetime2(7) NULL DEFAULT (getdate()),
    [status] nvarchar(20) NULL DEFAULT ('enrolled'),
    [is_deleted] bit NULL DEFAULT (0),
    FOREIGN KEY([course_id]) REFERENCES [courses]([id]),
    FOREIGN KEY([user_id]) REFERENCES [users]([id]),
    CHECK ([status] IN ('enrolled', 'completed', 'dropped', 'failed'))
);

CREATE TABLE [course_instructors](
    [course_id] int NOT NULL,
    [user_id] int NOT NULL,
    PRIMARY KEY ([course_id], [user_id]),
    FOREIGN KEY([course_id]) REFERENCES [courses]([id]) ON DELETE CASCADE,
    FOREIGN KEY([user_id]) REFERENCES [users]([id])
);

CREATE TABLE [course_learners](
    [course_id] int NOT NULL,
    [user_id] int NOT NULL,
    PRIMARY KEY ([course_id], [user_id]),
    FOREIGN KEY([course_id]) REFERENCES [courses]([id]) ON DELETE CASCADE,
    FOREIGN KEY([user_id]) REFERENCES [users]([id])
);

CREATE TABLE [course_prerequisites](
    [course_id] int NOT NULL,
    [prerequisite_id] int NOT NULL,
    PRIMARY KEY ([course_id], [prerequisite_id]),
    FOREIGN KEY([course_id]) REFERENCES [courses]([id]) ON DELETE CASCADE,
    FOREIGN KEY([prerequisite_id]) REFERENCES [courses]([id]),
    CHECK ([course_id] <> [prerequisite_id])
);

CREATE TABLE [material_types](
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [name] nvarchar(50) NOT NULL UNIQUE
);

CREATE TABLE [learning_materials](
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [title] nvarchar(200) NOT NULL,
    [content] nvarchar(max) NULL,
    [source_url] nvarchar(500) NULL,
    [material_type_id] int NOT NULL,
    [is_deleted] bit NULL DEFAULT (0),
    FOREIGN KEY([material_type_id]) REFERENCES [material_types]([id])
);

CREATE TABLE [course_materials](
    [course_id] int NOT NULL,
    [material_id] int NOT NULL,
    PRIMARY KEY ([course_id], [material_id]),
    FOREIGN KEY([course_id]) REFERENCES [courses]([id]) ON DELETE CASCADE,
    FOREIGN KEY([material_id]) REFERENCES [learning_materials]([id])
);

CREATE TABLE [session_types](
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [name] nvarchar(100) NOT NULL UNIQUE,
    [description] nvarchar(max) NULL,
    [created_at] datetime2(7) NULL DEFAULT (getdate()),
    [updated_at] datetime2(7) NULL DEFAULT (getdate())
);

CREATE TABLE [training_sessions](
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [name] nvarchar(200) NOT NULL,
    [description] nvarchar(max) NULL,
    [session_type_id] int NOT NULL,
    [instructor_id] int NOT NULL,
    [start_date] datetime2(7) NOT NULL,
    [end_date] datetime2(7) NOT NULL,
    [location] nvarchar(200) NULL,
    [created_at] datetime2(7) NULL DEFAULT (getdate()),
    [updated_at] datetime2(7) NULL DEFAULT (getdate()),
    FOREIGN KEY([session_type_id]) REFERENCES [session_types]([id]),
    FOREIGN KEY([instructor_id]) REFERENCES [instructors]([user_id]),
    CHECK ([end_date] > [start_date])
);

CREATE TABLE [course_sessions](
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [course_id] int NOT NULL,
    [training_session_id] int NOT NULL,
    FOREIGN KEY([course_id]) REFERENCES [courses]([id]) ON DELETE CASCADE,
    FOREIGN KEY([training_session_id]) REFERENCES [training_sessions]([id])
);

CREATE TABLE [session_learners](
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [trainingsession_id] int NOT NULL,
    [learner_id] int NOT NULL,
    [enrollment_status] nvarchar(20) NULL DEFAULT ('Applied'),
    [created_at] datetime2(7) NULL DEFAULT (getdate()),
    [updated_at] datetime2(7) NULL DEFAULT (getdate()),
    FOREIGN KEY([trainingsession_id]) REFERENCES [training_sessions]([id]) ON DELETE CASCADE,
    FOREIGN KEY([learner_id]) REFERENCES [learners]([user_id]),
    CHECK ([enrollment_status] IN ('Applied', 'Approved', 'Rejected'))
);

CREATE TABLE [session_attendances](
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [trainingsession_id] int NOT NULL,
    [learner_id] int NOT NULL,
    [attendance_status] nvarchar(10) NULL DEFAULT ('Present'),
    [attended_at] datetime2(7) NULL,
    [created_at] datetime2(7) NULL DEFAULT (getdate()),
    [updated_at] datetime2(7) NULL DEFAULT (getdate()),
    FOREIGN KEY([trainingsession_id]) REFERENCES [training_sessions]([id]) ON DELETE CASCADE,
    FOREIGN KEY([learner_id]) REFERENCES [learners]([user_id]),
    CHECK ([attendance_status] IN ('Present', 'Absent', 'Late'))
);

CREATE TABLE [session_schedules](
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [trainingsession_id] int NOT NULL,
    [schedule_date] date NOT NULL,
    [start_time] time(7) NOT NULL,
    [end_time] time(7) NOT NULL,
    [created_at] datetime2(7) NULL DEFAULT (getdate()),
    [updated_at] datetime2(7) NULL DEFAULT (getdate()),
    FOREIGN KEY([trainingsession_id]) REFERENCES [training_sessions]([id]) ON DELETE CASCADE,
    CHECK ([end_time] > [start_time])
);

CREATE TABLE [simulation_tasks](
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [course_id] int NOT NULL,
    [name] nvarchar(200) NOT NULL,
    [description] nvarchar(max) NULL,
    [passing_criteria] nvarchar(max) NULL,
    [created_at] datetime2(7) NULL DEFAULT (getdate()),
    [updated_at] datetime2(7) NULL DEFAULT (getdate()),
    FOREIGN KEY([course_id]) REFERENCES [courses]([id]) ON DELETE CASCADE
);

CREATE TABLE [training_session_simulation_tasks](
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [training_session_id] int NOT NULL,
    [simulation_task_id] int NOT NULL,
    [assigned_at] datetime2(7) NULL DEFAULT (getdate()),
    FOREIGN KEY([training_session_id]) REFERENCES [training_sessions]([id]) ON DELETE CASCADE,
    FOREIGN KEY([simulation_task_id]) REFERENCES [simulation_tasks]([id])
);

CREATE TABLE [learner_simulation_tasks](
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [learner_id] int NOT NULL,
    [training_session_simulation_task_id] int NOT NULL,
    [status] nvarchar(20) NULL DEFAULT ('not_started'),
    [progress] nvarchar(200) NULL,
    [feedback] nvarchar(max) NULL,
    [started_at] datetime2(7) NULL,
    [completed_at] datetime2(7) NULL,
    FOREIGN KEY([learner_id]) REFERENCES [learners]([user_id]),
    FOREIGN KEY([training_session_simulation_task_id]) REFERENCES [training_session_simulation_tasks]([id]) ON DELETE CASCADE,
    CHECK ([status] IN ('not_started', 'in_progress', 'completed', 'failed')),
    CHECK ([completed_at] >= [started_at] OR [completed_at] IS NULL)
);

CREATE TABLE [tests](
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [name] nvarchar(200) NOT NULL,
    [description] nvarchar(max) NULL,
    [duration_minutes] int NOT NULL,
    [total_points] decimal(5,2) NULL DEFAULT (0),
    [created_at] datetime2(7) NULL DEFAULT (getdate()),
    [updated_at] datetime2(7) NULL DEFAULT (getdate()),
    [is_deleted] bit NULL DEFAULT (0),
    CHECK ([duration_minutes] > 0),
    CHECK ([total_points] >= 0)
);

CREATE TABLE [test_questions](
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [test_id] int NOT NULL,
    [is_multiple_answers] bit NULL DEFAULT (0),
    [option_quantity] int NOT NULL,
    [answer_quantity] int NOT NULL,
    [points] decimal(5,2) NULL DEFAULT (1),
    [explanation] nvarchar(max) NULL,
    [is_deleted] bit NULL DEFAULT (0),
    FOREIGN KEY([test_id]) REFERENCES [tests]([id]) ON DELETE CASCADE,
    CHECK ([option_quantity] >= 2),
    CHECK ([answer_quantity] >= 1),
    CHECK ([points] >= 0),
    CHECK ([answer_quantity] <= [option_quantity])
);

CREATE TABLE [question_options](
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [question_id] int NOT NULL,
    [is_correct] bit NULL DEFAULT (0),
    [points] decimal(5,2) NULL DEFAULT (0),
    [order_index] int NOT NULL,
    [is_deleted] bit NULL DEFAULT (0),
    FOREIGN KEY([question_id]) REFERENCES [test_questions]([id]) ON DELETE CASCADE,
    CHECK ([order_index] >= 1),
    CHECK ([points] >= 0)
);

CREATE TABLE [learner_tests](
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [learner_id] int NOT NULL,
    [test_id] int NOT NULL,
    [attempt_number] int NULL DEFAULT (1),
    [score] decimal(5,2) NULL DEFAULT (0),
    [started_at] datetime2(7) NULL DEFAULT (getdate()),
    [completed_at] datetime2(7) NULL,
    [status] nvarchar(20) NULL DEFAULT ('started'),
    [review] nvarchar(max) NULL,
    [is_deleted] bit NULL DEFAULT (0),
    FOREIGN KEY([learner_id]) REFERENCES [learners]([user_id]),
    FOREIGN KEY([test_id]) REFERENCES [tests]([id]),
    CHECK ([attempt_number] >= 1),
    CHECK ([score] >= 0),
    CHECK ([status] IN ('started', 'in_progress', 'completed', 'abandoned')),
    CHECK ([completed_at] >= [started_at] OR [completed_at] IS NULL)
);

CREATE TABLE [learner_test_questions](
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [learner_test_id] int NOT NULL,
    [question_id] int NOT NULL,
    [order_index] int NOT NULL,
    [points] decimal(5,2) NULL DEFAULT (0),
    [is_deleted] bit NULL DEFAULT (0),
    FOREIGN KEY([learner_test_id]) REFERENCES [learner_tests]([id]) ON DELETE CASCADE,
    FOREIGN KEY([question_id]) REFERENCES [test_questions]([id]),
    CHECK ([order_index] >= 1),
    CHECK ([points] >= 0)
);

CREATE TABLE [learner_question_answers](
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [learner_test_question_id] int NOT NULL,
    [option_id] int NOT NULL,
    [answer_text] nvarchar(max) NULL,
    [is_correct] bit NULL DEFAULT (0),
    [points] decimal(5,2) NULL DEFAULT (0),
    [is_deleted] bit NULL DEFAULT (0),
    FOREIGN KEY([learner_test_question_id]) REFERENCES [learner_test_questions]([id]) ON DELETE CASCADE,
    FOREIGN KEY([option_id]) REFERENCES [question_options]([id]),
    CHECK ([points] >= 0)
);

-- Create indexes
CREATE INDEX [IX_course_enrollments_status] ON [course_enrollments]([status]);
CREATE INDEX [IX_courses_start_date] ON [courses]([start_date]);
CREATE INDEX [IX_courses_status] ON [courses]([status]);
CREATE INDEX [IX_learner_simulation_tasks_status] ON [learner_simulation_tasks]([status]);
CREATE INDEX [IX_learner_tests_started_at] ON [learner_tests]([started_at]);
CREATE INDEX [IX_learner_tests_status] ON [learner_tests]([status]);
CREATE INDEX [IX_session_attendances_status] ON [session_attendances]([attendance_status]);
CREATE INDEX [IX_session_learners_status] ON [session_learners]([enrollment_status]);
CREATE INDEX [IX_training_sessions_start_date] ON [training_sessions]([start_date]);
CREATE INDEX [IX_users_email] ON [users]([email]);
CREATE INDEX [IX_users_is_deleted] ON [users]([is_deleted]);
CREATE INDEX [IX_users_role_id] ON [users]([role_id]);