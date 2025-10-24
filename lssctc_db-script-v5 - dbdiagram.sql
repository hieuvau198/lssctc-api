

-- =========================
-- USER SYSTEM
-- =========================
CREATE TABLE [users] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [username] NVARCHAR(255) NOT NULL UNIQUE,
    [password] NVARCHAR(2000) NOT NULL,
    [email] NVARCHAR(255) NOT NULL,
    [fullname] NVARCHAR(2000),
	[role] INT DEFAULT 5,	-- Admin=1, ProgramManager=2, SimulationManager=3, Instructor=4, Trainee=5
    [phone_number] NVARCHAR(2000),
    [avatar_url] NVARCHAR(2000),
    [is_active] BIT NOT NULL DEFAULT 1,
    [is_deleted] BIT NOT NULL DEFAULT 0
);

CREATE TABLE [admins] (
    [id] INT PRIMARY KEY,
    FOREIGN KEY ([id]) REFERENCES [users]([id])
);

CREATE TABLE [simulation_managers] (
    [id] INT PRIMARY KEY,
    FOREIGN KEY ([id]) REFERENCES [users]([id])
);

CREATE TABLE [program_managers] (
    [id] INT PRIMARY KEY,
    FOREIGN KEY ([id]) REFERENCES [users]([id])
);

CREATE TABLE [instructors] (
    [id] INT PRIMARY KEY,
    [hire_date] DATETIME2(0),
    [instructor_code] NVARCHAR(2000),
    [is_active] BIT DEFAULT 1,
    [is_deleted] BIT DEFAULT 0,
    FOREIGN KEY ([id]) REFERENCES [users]([id])
);

CREATE TABLE [trainees] (
    [id] INT PRIMARY KEY,
    [trainee_code] NVARCHAR(2000) NOT NULL,
    [is_active] BIT DEFAULT 1,
    [is_deleted] BIT DEFAULT 0,
    FOREIGN KEY ([id]) REFERENCES [users]([id])
);

-- =========================
-- USER PROFILE
-- =========================
CREATE TABLE [instructor_profiles] (
    [id] INT PRIMARY KEY,
    [experience_years] INT,
    [biography] NVARCHAR(2000),
    [professional_profile_url] NVARCHAR(2000),
    [specialization] NVARCHAR(2000),
    FOREIGN KEY ([id]) REFERENCES [instructors]([id])
);

CREATE TABLE [trainee_profiles] (
    [id] INT PRIMARY KEY, 
    [driver_license_number] NVARCHAR(100),        
    [driver_license_level] NVARCHAR(50),          
    [driver_license_issued_date] DATETIME2(0),
	[driver_license_valid_start_date] DATETIME2(0),
	[driver_license_valid_end_date] DATETIME2(0),
	[driver_license_image_url] NVARCHAR(2000),
    [education_level] NVARCHAR(255),
	[education_image_url] NVARCHAR(2000),
    [citizen_card_id] NVARCHAR(20),         
    [citizen_card_issued_date] DATE,            
    [citizen_card_place_of_issue] NVARCHAR(255),
	[citizen_card_image_url] NVARCHAR(2000),
    FOREIGN KEY ([id]) REFERENCES [trainees]([id])
);

-- =========================
-- Independent Tables
-- =========================
CREATE TABLE [training_programs] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL UNIQUE,
    [description] NVARCHAR(2000),
    [is_deleted] BIT DEFAULT 0,
    [is_active] BIT DEFAULT 1,
    [duration_hours] INT,
    [total_courses] INT,
    [image_url] NVARCHAR(2000)
);	

CREATE TABLE [certificates] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL UNIQUE,
    [description] NVARCHAR(2000),
    [effective_time] INT,
    [requirement] NVARCHAR(2000),
    [certifying_authority] NVARCHAR(2000),
    [image_url] NVARCHAR(2000)
);	

CREATE TABLE [course_categories] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL UNIQUE,
    [description] NVARCHAR(2000)
);	-- Mobile Crane, Safety & Regulations, Basic Operation, Advanced Operation, Equipment Maintenance, Certification Preparation 

CREATE TABLE [course_levels] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL UNIQUE,
    [description] NVARCHAR(2000)
);	-- need seed data: Entry, Standard, Expert

CREATE TABLE [course_codes] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL UNIQUE
);	-- need seed data, CO000001, CO000002, CO000003, CO000004, CO000005

CREATE TABLE [class_codes] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL UNIQUE
); -- need seed data, CL000001, CL000002, CL000003, CL000004, CL000005

CREATE TABLE [learning_material_types] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL
); -- need seed data: PDF, Video, Image, URL

CREATE TABLE [learning_materials] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [learning_material_type_id] INT NOT NULL,
    [name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(2000) NOT NULL,
	[material_url] NVARCHAR(2000) NOT NULL,
    FOREIGN KEY ([learning_material_type_id]) REFERENCES [learning_material_types]([id])
); -- need seed data

CREATE TABLE [quizzes] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100),
    [pass_score_criteria] DECIMAL(5,2),
    [created_at] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [updated_at] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [timelimit_minute] INT,
    [total_score] DECIMAL(5,2),
    [description] NVARCHAR(2000)
); 

CREATE TABLE [quiz_questions] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [quiz_id] INT NOT NULL,
    [name] NVARCHAR(100) NOT NULL,
    [question_score] DECIMAL(5,2),
    [description] NVARCHAR(2000),
    [is_multiple_answers] BIT NOT NULL DEFAULT 1,
    FOREIGN KEY ([quiz_id]) REFERENCES [quizzes]([id])
); 

CREATE TABLE [quiz_question_options] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [quiz_question_id] INT NOT NULL,
    [description] NVARCHAR(2000),
	[explanation] NVARCHAR(2000) NULL,
    [is_correct] BIT NOT NULL DEFAULT 1,
    [display_order] INT UNIQUE,
    [option_score] DECIMAL(5,2),
    [name] NVARCHAR(100) NOT NULL,
    FOREIGN KEY ([quiz_question_id]) REFERENCES [quiz_questions]([id])
); 

CREATE TABLE [simulation_settings] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL,
    [description] NVARCHAR(2000),
    [is_active] BIT DEFAULT 1,
    [is_deleted] BIT DEFAULT 0
); 

CREATE TABLE [simulation_timeslots] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL,
    [start_time] DATETIME2(0) NOT NULL,
    [end_time] DATETIME2(0) NOT NULL,
    [note] NVARCHAR(2000),
    [status] INT DEFAULT 1,
    [is_active] BIT DEFAULT 1,
    [is_deleted] BIT DEFAULT 0,
); 

CREATE TABLE [simulation_components] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL,
    [description] NVARCHAR(2000),
    [image_url] NVARCHAR(2000),
    [is_active] BIT DEFAULT 1,
    [created_date] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [is_deleted] BIT DEFAULT 0
);

CREATE TABLE [sim_actions] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(255) NOT NULL,
    [description] NVARCHAR(2000),
    [action_key] NVARCHAR(50),
    [is_active] BIT DEFAULT 1,
    [is_deleted] BIT DEFAULT 0
);

CREATE TABLE [practices] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [practice_name] NVARCHAR(2000) NOT NULL,
    [practice_description] NVARCHAR(2000),
    [estimated_duration_minutes] INT,
    [difficulty_level] NVARCHAR(2000),
    [max_attempts] INT,
    [created_date] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [is_active] BIT DEFAULT 1,
    [is_deleted] BIT DEFAULT 0
);

CREATE TABLE [practice_steps] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [practice_id] INT NOT NULL,
    [step_name] NVARCHAR(2000) NOT NULL,
    [step_description] NVARCHAR(2000),
    [expected_result] NVARCHAR(2000),
    [step_order] INT NOT NULL,
    [is_deleted] BIT DEFAULT 0,
    FOREIGN KEY ([practice_id]) REFERENCES [practices]([id])
);



CREATE TABLE [practice_step_actions] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [step_id] INT NOT NULL,
    [action_id] INT NOT NULL,
    [name] NVARCHAR(255) NOT NULL,
    [description] NVARCHAR(2000),
    [is_deleted] BIT DEFAULT 0,
    FOREIGN KEY ([step_id]) REFERENCES [practice_steps]([id]),
    FOREIGN KEY ([action_id]) REFERENCES [sim_actions]([id])
);

CREATE TABLE [practice_step_warnings] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [practice_step_id] INT NOT NULL,
    [warning_message] NVARCHAR(2000),
    [is_critical] BIT DEFAULT 0,
    [instruction] NVARCHAR(2000),
    [is_deleted] BIT DEFAULT 0,
    FOREIGN KEY ([practice_step_id]) REFERENCES [practice_steps]([id]),
);

CREATE TABLE [practice_step_components] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [step_id] INT NOT NULL,
    [component_id] INT NOT NULL,
    [component_order] INT NOT NULL,
    [is_deleted] BIT DEFAULT 0,
    FOREIGN KEY ([step_id]) REFERENCES [practice_steps]([id]),
    FOREIGN KEY ([component_id]) REFERENCES [simulation_components]([id])
);

CREATE TABLE [syllabuses] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL,
	[course_name] NVARCHAR(100) NOT NULL,
	[course_code] NVARCHAR(100) NOT NULL,
    [description] NVARCHAR(2000),
    [is_active] BIT DEFAULT 1,
    [is_deleted] BIT DEFAULT 0
);

CREATE TABLE [syllabus_sections] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [syllabus_id] INT NOT NULL,
    [section_title] NVARCHAR(200) NOT NULL,
    [section_description] NVARCHAR(2000),
    [section_order] INT NOT NULL,
    [estimated_duration_minutes] INT,
    [is_deleted] BIT DEFAULT 0,
    FOREIGN KEY ([syllabus_id]) REFERENCES [syllabuses]([id])
);


-- =========================
-- Specific Tables
-- =========================


CREATE TABLE [program_entry_requirements] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [program_id] INT NOT NULL,
    [name] NVARCHAR(100) NOT NULL,
    [description] NVARCHAR(2000),
	[document_url] NVARCHAR(2000),
    FOREIGN KEY ([program_id]) REFERENCES [training_programs]([id])
);

CREATE TABLE [courses] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL,
    [description] NVARCHAR(2000),
    [category_id] INT,
    [level_id] INT,
    [price] DECIMAL(10, 2),
    [is_active] BIT DEFAULT 1,
    [is_deleted] BIT DEFAULT 0,
    [image_url] NVARCHAR(2000),
    [duration_hours] INT,
    [course_code_id] INT,
    FOREIGN KEY ([category_id]) REFERENCES [course_categories]([id]),
    FOREIGN KEY ([level_id]) REFERENCES [course_levels]([id]),
    FOREIGN KEY ([course_code_id]) REFERENCES [course_codes]([id])
);

CREATE TABLE [program_courses] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [program_id] INT NOT NULL, 
    [courses_id] INT NOT NULL,
    [course_order] INT NOT NULL,
    [name] NVARCHAR(100),
    [description] NVARCHAR(2000),
    FOREIGN KEY ([program_id]) REFERENCES [training_programs]([id]),
    FOREIGN KEY ([courses_id]) REFERENCES [courses]([id])
);

CREATE TABLE [course_certificates] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [course_id] INT,
    [certificate_id] INT,
    FOREIGN KEY ([course_id]) REFERENCES [courses]([id]),
    FOREIGN KEY ([certificate_id]) REFERENCES [certificates]([id])
);

CREATE TABLE [course_syllabuses] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [course_id] INT NOT NULL,
    [syllabus_id] INT NOT NULL,
    FOREIGN KEY ([course_id]) REFERENCES [courses]([id]),
    FOREIGN KEY ([syllabus_id]) REFERENCES [syllabuses]([id])
);

CREATE TABLE [classes] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL,
    [start_date] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [end_date] DATETIME2(0),
    [capacity] INT,
    [program_course_id] INT NOT NULL,
    [class_code_id] INT,
    [description] NVARCHAR(2000) NOT NULL,
    [status] INT NOT NULL DEFAULT 1,
    FOREIGN KEY ([program_course_id]) REFERENCES [program_courses]([id]),
    FOREIGN KEY ([class_code_id]) REFERENCES [class_codes]([id])
);

CREATE TABLE [class_registrations] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL,
    [created_date] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [approved_date] DATETIME2(0),
    [class_id] INT NOT NULL,
	[trainee_id] INT NOT NULL,
	[trainee_contact] NVARCHAR(100),
    [description] NVARCHAR(2000) NOT NULL,
    [status] INT NOT NULL DEFAULT 1,
    FOREIGN KEY ([class_id]) REFERENCES [classes]([id]),
    FOREIGN KEY ([trainee_id]) REFERENCES [trainees]([id])
);

CREATE TABLE [class_instructors] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [class_id] INT NOT NULL,
    [instructor_id] INT NOT NULL,
    [position] NVARCHAR(2000) NOT NULL,
    FOREIGN KEY ([class_id]) REFERENCES [classes]([id]),
    FOREIGN KEY ([instructor_id]) REFERENCES [instructors]([id])
);

CREATE TABLE [class_members] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [trainee_id] INT NOT NULL,
    [class_id] INT NOT NULL,
    [assigned_date] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [status] INT NOT NULL DEFAULT 1,
    FOREIGN KEY ([trainee_id]) REFERENCES [trainees]([id]),
    FOREIGN KEY ([class_id]) REFERENCES [classes]([id])
);

CREATE TABLE [training_progresses] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [course_member_id] INT NOT NULL,
    [status] INT NOT NULL DEFAULT 1,
    [progress_percentage] FLOAT,
    [start_date] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [last_updated] DATETIME2(0),
    [name] NVARCHAR(100),
    [description] NVARCHAR(2000),
    FOREIGN KEY ([course_member_id]) REFERENCES [class_members]([id]),
);

CREATE TABLE [training_result_types] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL,
    [description] NVARCHAR(2000)
);

CREATE TABLE [training_results] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [training_result_type_id] INT NOT NULL,
    [training_progress_id] INT NOT NULL,
    [result_value] NVARCHAR(2000),
    [result_date] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [notes] NVARCHAR(2000),
    FOREIGN KEY ([training_result_type_id]) REFERENCES [training_result_types]([id]),
    FOREIGN KEY ([training_progress_id]) REFERENCES [training_progresses]([id])
);

CREATE TABLE [trainee_certificates] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL, 
    [description] NVARCHAR(2000),
    [course_certificate_id] INT,
    [valid_date_end] DATETIME2(0),
    [trainee_id] INT,
    [issued_date_start] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [issued_date_end] DATETIME2(0),
    [status] INT NOT NULL DEFAULT 1,
    FOREIGN KEY ([course_certificate_id]) REFERENCES [course_certificates]([id]),
    FOREIGN KEY ([trainee_id]) REFERENCES [trainees]([id])
);


CREATE TABLE [sections] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(100) NOT NULL,
    [description] NVARCHAR(2000),
    [classes_id] INT NOT NULL,
	[syllabus_section_id] INT NOT NULL,
    [duration_minutes] INT,
    [order] INT NOT NULL,
    [start_date] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [end_date] DATETIME2(0),
    [status] INT NOT NULL DEFAULT 1,
    FOREIGN KEY ([classes_id]) REFERENCES [classes]([id]),
	FOREIGN KEY ([syllabus_section_id]) REFERENCES [syllabus_sections]([id])
);

CREATE TABLE [section_partition_types] (
  [id] INT IDENTITY(1,1) PRIMARY KEY,
  [description] NVARCHAR(2000) NULL,
  [name] NVARCHAR(100) NOT NULL,
  [pass_criteria] NVARCHAR(2000) NOT NULL,
  [is_action_required] BIT NULL
);

CREATE TABLE [section_partitions] (
  [id] INT IDENTITY(1,1) PRIMARY KEY,
  [section_id] INT NOT NULL,      
  [name] NVARCHAR(100) NULL,
  [partition_type_id] INT NOT NULL,
  [display_order] INT NULL,
  [description] NVARCHAR(2000) NULL,
    FOREIGN KEY ([partition_type_id]) REFERENCES [section_partition_types]([id]),
    FOREIGN KEY ([section_id]) REFERENCES [sections]([id])
);

CREATE TABLE [learning_records] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [section_id] INT NOT NULL,
    [name] NVARCHAR(100),
    [training_progress_id] INT NOT NULL,
    [section_name] NVARCHAR(255),
    [is_completed] BIT NOT NULL DEFAULT 1,
    [is_trainee_attended] BIT NOT NULL DEFAULT 1,
    [progress] DECIMAL(5,2),
    FOREIGN KEY ([section_id]) REFERENCES [sections]([id]),
    FOREIGN KEY ([training_progress_id]) REFERENCES [training_progresses]([id])
);

CREATE TABLE [learning_record_partitions] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [section_partition_id] INT NOT NULL,
    [name] NVARCHAR(100),
    [learning_record_id] INT NOT NULL,
    [description] NVARCHAR(2000),
    [updated_at] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [completed_at] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [started_at] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [is_complete] BIT NOT NULL DEFAULT 1,
	[record_partition_order] INT NOT NULL,
    FOREIGN KEY ([section_partition_id]) REFERENCES [section_partitions]([id]),
    FOREIGN KEY ([learning_record_id]) REFERENCES [learning_records]([id])
);

CREATE TABLE [section_materials] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [section_partition_id] INT NOT NULL,
	[learning_material_id] INT NOT NULL,
    [name] NVARCHAR(100) NOT NULL,
	[description] NVARCHAR(2000) NOT NULL,
    FOREIGN KEY ([section_partition_id]) REFERENCES [section_partitions]([id]),
	FOREIGN KEY ([learning_material_id]) REFERENCES [learning_materials]([id])
);

CREATE TABLE [section_quizzes] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [quiz_id] INT NOT NULL,
    [name] NVARCHAR(100) NOT NULL,
    [section_partition_id] INT NOT NULL,
    [description] NVARCHAR(2000),
    FOREIGN KEY ([quiz_id]) REFERENCES [quizzes]([id]),
    FOREIGN KEY ([section_partition_id]) REFERENCES [section_partitions]([id])
);

CREATE TABLE [section_practices] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [section_partition_id] INT NOT NULL,
    [practice_id] INT NOT NULL,
    [custom_deadline] DATETIME2(0),
    [custom_description] NVARCHAR(2000),
    [status] INT DEFAULT 1,
    [is_active] BIT DEFAULT 1,
    [is_deleted] BIT DEFAULT 0,
    FOREIGN KEY ([section_partition_id]) REFERENCES [section_partitions]([id]),
    FOREIGN KEY ([practice_id]) REFERENCES [practices]([id])
);

CREATE TABLE [section_practice_timeslots] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [section_practice_id] INT NOT NULL,
    [simulation_timeslot_id] INT NOT NULL,
    [note] NVARCHAR(1000),
    FOREIGN KEY ([section_practice_id]) REFERENCES [section_practices]([id]),
    FOREIGN KEY ([simulation_timeslot_id]) REFERENCES [simulation_timeslots]([id])
);

CREATE TABLE [section_quiz_attempts] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [section_quiz_id] INT NOT NULL,
    [name] NVARCHAR(100) NOT NULL,
    [attempt_score] DECIMAL(5,2),
    [learning_record_partition_id] INT NOT NULL,
    [max_score] DECIMAL(5,2),
    [quiz_attempt_date] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [status] INT NOT NULL DEFAULT 1,
    [attempt_order] INT,
	[is_pass] BIT,
    FOREIGN KEY ([section_quiz_id]) REFERENCES [section_quizzes]([id]),
	FOREIGN KEY ([learning_record_partition_id]) REFERENCES [learning_record_partitions]([id])

);

CREATE TABLE [section_quiz_attempt_questions] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [section_quiz_attempt_id] INT NOT NULL,
    [attempt_score] DECIMAL(5,2),
    [question_score] DECIMAL(5,2),
    [is_correct] BIT NOT NULL DEFAULT 1,
    [is_multiple_answers] BIT NOT NULL DEFAULT 1,
    [name] NVARCHAR(100) NOT NULL,
    [description] NVARCHAR(2000),
    FOREIGN KEY ([section_quiz_attempt_id]) REFERENCES [section_quiz_attempts]([id])
);

CREATE TABLE [section_quiz_attempt_answers] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [section_quiz_attempt_question_id] INT NOT NULL,
    [attempt_score] DECIMAL(5,2),
    [is_correct] BIT NOT NULL DEFAULT 1,
    [description] NVARCHAR(2000),
    [name] NVARCHAR(100) NOT NULL,
    FOREIGN KEY ([section_quiz_attempt_question_id]) REFERENCES [section_quiz_attempt_questions]([id])
);

CREATE TABLE [section_practice_attempts] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [section_practice_id] INT NOT NULL,
    [learning_record_partition_id] INT NOT NULL,
    [score] DECIMAL(5,2),
    [attempt_date] DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    [attempt_status] INT DEFAULT 1,
    [description] NVARCHAR(2000),
    [is_pass] BIT DEFAULT 0,
    [is_deleted] BIT DEFAULT 0,
    FOREIGN KEY ([section_practice_id]) REFERENCES [section_practices]([id]),
    FOREIGN KEY ([learning_record_partition_id]) REFERENCES [learning_record_partitions]([id])
);

CREATE TABLE [section_practice_attempt_steps] (
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [attempt_id] INT NOT NULL,
    [practice_step_id] INT NOT NULL,
    [score] DECIMAL(5,2),
    [description] NVARCHAR(2000),
    [is_pass] BIT DEFAULT 0,
    [is_deleted] BIT DEFAULT 0,
    FOREIGN KEY ([attempt_id]) REFERENCES [section_practice_attempts]([id]),
    FOREIGN KEY ([practice_step_id]) REFERENCES [practice_steps]([id])
);

