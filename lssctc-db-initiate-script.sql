-- Create Database
CREATE DATABASE lssctc_db;
GO

USE lssctc_db;
GO

BEGIN TRY
    BEGIN TRANSACTION;

-- ========================================
-- USER MANAGEMENT TABLES
-- ========================================

-- Roles table
CREATE TABLE roles (
    id TINYINT PRIMARY KEY,
    name NVARCHAR(20) NOT NULL UNIQUE
);

-- Users table
CREATE TABLE users (
    id INT IDENTITY(1,1) PRIMARY KEY,
    username NVARCHAR(50) NOT NULL UNIQUE,
    password NVARCHAR(255) NOT NULL,
    email NVARCHAR(100) NOT NULL UNIQUE,
    fullname NVARCHAR(100) NOT NULL,
    profile_image_url NVARCHAR(500) NULL,
    role_id TINYINT NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    is_deleted BIT DEFAULT 0,
    CONSTRAINT FK_users_roles FOREIGN KEY (role_id) REFERENCES roles(id)
);

-- Learners table
CREATE TABLE learners (
    user_id INT PRIMARY KEY,
    date_of_birth DATE NULL,
    enrollment_status NVARCHAR(20) DEFAULT 'active' CHECK (enrollment_status IN ('active', 'inactive', 'suspended')),
    is_deleted BIT DEFAULT 0,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

-- Instructors table
CREATE TABLE instructors (
    user_id INT PRIMARY KEY,
    bio NVARCHAR NULL,
    specialization NVARCHAR(100) NULL,
    years_experience INT DEFAULT 0 CHECK (years_experience >= 0),
    is_deleted BIT DEFAULT 0,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

-- Staff table
CREATE TABLE staff (
    user_id INT PRIMARY KEY,
    employment_status NVARCHAR(20) DEFAULT 'active' CHECK (employment_status IN ('active', 'inactive', 'on_leave')),
    is_deleted BIT DEFAULT 0,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

-- Admins table
CREATE TABLE admins (
    user_id INT PRIMARY KEY,
    is_deleted BIT DEFAULT 0,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

-- ========================================
-- COURSE MANAGEMENT TABLES
-- ========================================

-- Course categories
CREATE TABLE course_categories (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(100) NOT NULL UNIQUE
);

-- Course levels
CREATE TABLE course_levels (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(50) NOT NULL UNIQUE
);

-- Course definitions
CREATE TABLE course_definitions (
    id INT IDENTITY(1,1) PRIMARY KEY,
    title NVARCHAR(200) NOT NULL,
    course_code NVARCHAR(20) NOT NULL UNIQUE,
    description NVARCHAR(MAX) NULL,
    category_id INT NOT NULL,
    level_id INT NOT NULL,
    duration INT NULL CHECK (duration > 0), -- in hours
    is_deleted BIT DEFAULT 0,
    FOREIGN KEY (category_id) REFERENCES course_categories(id),
    FOREIGN KEY (level_id) REFERENCES course_levels(id)
);

-- Courses
CREATE TABLE courses (
    id INT IDENTITY(1,1) PRIMARY KEY,
    course_definition_id INT NOT NULL,
    title NVARCHAR(200) NOT NULL,
    course_code NVARCHAR(20) NOT NULL,
    description NVARCHAR(MAX) NULL,
    category NVARCHAR(100) NOT NULL,
    level NVARCHAR(50) NOT NULL,
    start_date DATE NULL,
    end_date DATE NULL,
    location NVARCHAR(200) NULL,
    status NVARCHAR(20) DEFAULT 'planned' CHECK (status IN ('planned', 'ongoing', 'completed', 'cancelled')),
    is_deleted BIT DEFAULT 0,
    FOREIGN KEY (course_definition_id) REFERENCES course_definitions(id),
    CHECK (end_date >= start_date OR end_date IS NULL)
);

-- Course prerequisites
CREATE TABLE course_prerequisites (
    course_id INT NOT NULL,
    prerequisite_id INT NOT NULL,
    PRIMARY KEY (course_id, prerequisite_id),
    FOREIGN KEY (course_id) REFERENCES courses(id) ON DELETE CASCADE,
    FOREIGN KEY (prerequisite_id) REFERENCES courses(id),
    CHECK (course_id != prerequisite_id)
);

-- Course instructors
CREATE TABLE course_instructors (
    course_id INT NOT NULL,
    user_id INT NOT NULL,
    PRIMARY KEY (course_id, user_id),
    FOREIGN KEY (course_id) REFERENCES courses(id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users(id)
);

-- Course learners
CREATE TABLE course_learners (
    course_id INT NOT NULL,
    user_id INT NOT NULL,
    PRIMARY KEY (course_id, user_id),
    FOREIGN KEY (course_id) REFERENCES courses(id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users(id)
);

-- Course enrollments
CREATE TABLE course_enrollments (
    id INT IDENTITY(1,1) PRIMARY KEY,
    course_id INT NOT NULL,
    user_id INT NOT NULL,
    enrollment_date DATETIME2 DEFAULT GETDATE(),
    status NVARCHAR(20) DEFAULT 'enrolled' CHECK (status IN ('enrolled', 'completed', 'dropped', 'failed')),
    is_deleted BIT DEFAULT 0,
    FOREIGN KEY (course_id) REFERENCES courses(id),
    FOREIGN KEY (user_id) REFERENCES users(id)
);

-- Material types
CREATE TABLE material_types (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(50) NOT NULL UNIQUE
);

-- Learning materials
CREATE TABLE learning_materials (
    id INT IDENTITY(1,1) PRIMARY KEY,
    title NVARCHAR(200) NOT NULL,
    content NVARCHAR(MAX) NULL,
    source_url NVARCHAR(500) NULL,
    material_type_id INT NOT NULL,
    is_deleted BIT DEFAULT 0,
    FOREIGN KEY (material_type_id) REFERENCES material_types(id)
);

-- Course materials
CREATE TABLE course_materials (
    course_id INT NOT NULL,
    material_id INT NOT NULL,
    PRIMARY KEY (course_id, material_id),
    FOREIGN KEY (course_id) REFERENCES courses(id) ON DELETE CASCADE,
    FOREIGN KEY (material_id) REFERENCES learning_materials(id)
);

-- ========================================
-- TRAINING SESSION TABLES
-- ========================================

-- Session types
CREATE TABLE session_types (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(100) NOT NULL UNIQUE,
    description NVARCHAR(MAX) NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- Training sessions
CREATE TABLE training_sessions (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(200) NOT NULL,
    description NVARCHAR(MAX) NULL,
    session_type_id INT NOT NULL,
    instructor_id INT NOT NULL,
    start_date DATETIME2 NOT NULL,
    end_date DATETIME2 NOT NULL,
    location NVARCHAR(200) NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (session_type_id) REFERENCES session_types(id),
    FOREIGN KEY (instructor_id) REFERENCES instructors(user_id),
    CHECK (end_date > start_date)
);

-- Course sessions
CREATE TABLE course_sessions (
    id INT IDENTITY(1,1) PRIMARY KEY,
    course_id INT NOT NULL,
    training_session_id INT NOT NULL,
    FOREIGN KEY (course_id) REFERENCES courses(id) ON DELETE CASCADE,
    FOREIGN KEY (training_session_id) REFERENCES training_sessions(id)
);

-- Session schedules
CREATE TABLE session_schedules (
    id INT IDENTITY(1,1) PRIMARY KEY,
    trainingsession_id INT NOT NULL,
    schedule_date DATE NOT NULL,
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (trainingsession_id) REFERENCES training_sessions(id) ON DELETE CASCADE,
    CHECK (end_time > start_time)
);

-- Session attendances
CREATE TABLE session_attendances (
    id INT IDENTITY(1,1) PRIMARY KEY,
    trainingsession_id INT NOT NULL,
    learner_id INT NOT NULL,
    attendance_status NVARCHAR(10) DEFAULT 'Present' CHECK (attendance_status IN ('Present', 'Absent', 'Late')),
    attended_at DATETIME2 NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (trainingsession_id) REFERENCES training_sessions(id) ON DELETE CASCADE,
    FOREIGN KEY (learner_id) REFERENCES learners(user_id)
);

-- Session learners
CREATE TABLE session_learners (
    id INT IDENTITY(1,1) PRIMARY KEY,
    trainingsession_id INT NOT NULL,
    learner_id INT NOT NULL,
    enrollment_status NVARCHAR(20) DEFAULT 'Applied' CHECK (enrollment_status IN ('Applied', 'Approved', 'Rejected')),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (trainingsession_id) REFERENCES training_sessions(id) ON DELETE CASCADE,
    FOREIGN KEY (learner_id) REFERENCES learners(user_id)
);

-- ========================================
-- TEST MANAGEMENT TABLES
-- ========================================

-- Tests
CREATE TABLE tests (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(200) NOT NULL,
    description NVARCHAR(MAX) NULL,
    duration_minutes INT NOT NULL CHECK (duration_minutes > 0),
    total_points DECIMAL(5,2) DEFAULT 0 CHECK (total_points >= 0),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    is_deleted BIT DEFAULT 0
);

-- Test questions
CREATE TABLE test_questions (
    id INT IDENTITY(1,1) PRIMARY KEY,
    test_id INT NOT NULL,
    is_multiple_answers BIT DEFAULT 0,
    option_quantity INT NOT NULL CHECK (option_quantity >= 2),
    answer_quantity INT NOT NULL CHECK (answer_quantity >= 1),
    points DECIMAL(5,2) DEFAULT 1 CHECK (points >= 0),
    explanation NVARCHAR(MAX) NULL,
    is_deleted BIT DEFAULT 0,
    FOREIGN KEY (test_id) REFERENCES tests(id) ON DELETE CASCADE,
    CHECK (answer_quantity <= option_quantity)
);

-- Question options
CREATE TABLE question_options (
    id INT IDENTITY(1,1) PRIMARY KEY,
    question_id INT NOT NULL,
    is_correct BIT DEFAULT 0,
    points DECIMAL(5,2) DEFAULT 0 CHECK (points >= 0),
    order_index INT NOT NULL CHECK (order_index >= 1),
    is_deleted BIT DEFAULT 0,
    FOREIGN KEY (question_id) REFERENCES test_questions(id) ON DELETE CASCADE
);

-- Learner tests
CREATE TABLE learner_tests (
    id INT IDENTITY(1,1) PRIMARY KEY,
    learner_id INT NOT NULL,
    test_id INT NOT NULL,
    attempt_number INT DEFAULT 1 CHECK (attempt_number >= 1),
    score DECIMAL(5,2) DEFAULT 0 CHECK (score >= 0),
    started_at DATETIME2 DEFAULT GETDATE(),
    completed_at DATETIME2 NULL,
    status NVARCHAR(20) DEFAULT 'started' CHECK (status IN ('started', 'in_progress', 'completed', 'abandoned')),
    review NVARCHAR(MAX) NULL,
    is_deleted BIT DEFAULT 0,
    FOREIGN KEY (learner_id) REFERENCES learners(user_id),
    FOREIGN KEY (test_id) REFERENCES tests(id),
    CHECK (completed_at >= started_at OR completed_at IS NULL)
);

-- Learner test questions
CREATE TABLE learner_test_questions (
    id INT IDENTITY(1,1) PRIMARY KEY,
    learner_test_id INT NOT NULL,
    question_id INT NOT NULL,
    order_index INT NOT NULL CHECK (order_index >= 1),
    points DECIMAL(5,2) DEFAULT 0 CHECK (points >= 0),
    is_deleted BIT DEFAULT 0,
    FOREIGN KEY (learner_test_id) REFERENCES learner_tests(id) ON DELETE CASCADE,
    FOREIGN KEY (question_id) REFERENCES test_questions(id)
);

-- Learner question answers
CREATE TABLE learner_question_answers (
    id INT IDENTITY(1,1) PRIMARY KEY,
    learner_test_question_id INT NOT NULL,
    option_id INT NOT NULL,
    answer_text NVARCHAR(MAX) NULL,
    is_correct BIT DEFAULT 0,
    points DECIMAL(5,2) DEFAULT 0 CHECK (points >= 0),
    is_deleted BIT DEFAULT 0,
    FOREIGN KEY (learner_test_question_id) REFERENCES learner_test_questions(id) ON DELETE CASCADE,
    FOREIGN KEY (option_id) REFERENCES question_options(id)
);

-- ========================================
-- SIMULATION TASK TABLES
-- ========================================

-- Simulation tasks
CREATE TABLE simulation_tasks (
    id INT IDENTITY(1,1) PRIMARY KEY,
    course_id INT NOT NULL,
    name NVARCHAR(200) NOT NULL,
    description NVARCHAR(MAX) NULL,
    passing_criteria NVARCHAR(MAX) NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (course_id) REFERENCES courses(id) ON DELETE CASCADE
);

-- Training session simulation tasks
CREATE TABLE training_session_simulation_tasks (
    id INT IDENTITY(1,1) PRIMARY KEY,
    training_session_id INT NOT NULL,
    simulation_task_id INT NOT NULL,
    assigned_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (training_session_id) REFERENCES training_sessions(id) ON DELETE CASCADE,
    FOREIGN KEY (simulation_task_id) REFERENCES simulation_tasks(id)
);

-- Learner simulation tasks
CREATE TABLE learner_simulation_tasks (
    id INT IDENTITY(1,1) PRIMARY KEY,
    learner_id INT NOT NULL,
    training_session_simulation_task_id INT NOT NULL,
    status NVARCHAR(20) DEFAULT 'not_started' CHECK (status IN ('not_started', 'in_progress', 'completed', 'failed')),
    progress NVARCHAR(200) NULL, -- Can store JSON or simple text like "75%" or "score: 85"
    feedback NVARCHAR(MAX) NULL,
    started_at DATETIME2 NULL,
    completed_at DATETIME2 NULL,
    FOREIGN KEY (learner_id) REFERENCES learners(user_id),
    FOREIGN KEY (training_session_simulation_task_id) REFERENCES training_session_simulation_tasks(id) ON DELETE CASCADE,
    CHECK (completed_at >= started_at OR completed_at IS NULL)
);

-- ========================================
-- INDEXES FOR PERFORMANCE
-- ========================================

-- User-related indexes
CREATE INDEX IX_users_role_id ON users(role_id);
CREATE INDEX IX_users_email ON users(email);
CREATE INDEX IX_users_is_deleted ON users(is_deleted);

-- Course-related indexes
CREATE INDEX IX_courses_status ON courses(status);
CREATE INDEX IX_courses_start_date ON courses(start_date);
CREATE INDEX IX_course_enrollments_status ON course_enrollments(status);

-- Session-related indexes
CREATE INDEX IX_training_sessions_start_date ON training_sessions(start_date);
CREATE INDEX IX_session_attendances_status ON session_attendances(attendance_status);
CREATE INDEX IX_session_learners_status ON session_learners(enrollment_status);

-- Test-related indexes
CREATE INDEX IX_learner_tests_status ON learner_tests(status);
CREATE INDEX IX_learner_tests_started_at ON learner_tests(started_at);

-- Simulation-related indexes
CREATE INDEX IX_learner_simulation_tasks_status ON learner_simulation_tasks(status);

    COMMIT TRANSACTION; -- Commit only if all above succeed
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION; -- Roll back the whole transaction if any error occurs

    -- Print the error for debugging
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();
    
    PRINT 'Error occurred: ' + @ErrorMessage;
    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH;

GO