/****** Object:  Database [lssctc_db]    Script Date: 9/5/2025 9:45:42 AM ******/
CREATE DATABASE [lssctc_db]  (EDITION = 'Basic', SERVICE_OBJECTIVE = 'Basic', MAXSIZE = 2 GB) WITH CATALOG_COLLATION = SQL_Latin1_General_CP1_CI_AS, LEDGER = OFF;
GO
ALTER DATABASE [lssctc_db] SET COMPATIBILITY_LEVEL = 170
GO
ALTER DATABASE [lssctc_db] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [lssctc_db] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [lssctc_db] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [lssctc_db] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [lssctc_db] SET ARITHABORT OFF 
GO
ALTER DATABASE [lssctc_db] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [lssctc_db] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [lssctc_db] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [lssctc_db] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [lssctc_db] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [lssctc_db] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [lssctc_db] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [lssctc_db] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [lssctc_db] SET ALLOW_SNAPSHOT_ISOLATION ON 
GO
ALTER DATABASE [lssctc_db] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [lssctc_db] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [lssctc_db] SET  MULTI_USER 
GO
ALTER DATABASE [lssctc_db] SET ENCRYPTION ON
GO
ALTER DATABASE [lssctc_db] SET QUERY_STORE = ON
GO
ALTER DATABASE [lssctc_db] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 7), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 10, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
/*** The scripts of database scoped configurations in Azure should be executed inside the target database connection. ***/
GO
-- ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = 8;
GO
/****** Object:  Table [dbo].[admins]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[admins](
	[user_id] [int] NOT NULL,
	[is_deleted] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[user_id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[course_categories]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[course_categories](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[name] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[course_definitions]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[course_definitions](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[title] [nvarchar](200) NOT NULL,
	[course_code] [nvarchar](20) NOT NULL,
	[description] [nvarchar](max) NULL,
	[category_id] [int] NOT NULL,
	[level_id] [int] NOT NULL,
	[duration] [int] NULL,
	[is_deleted] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[course_code] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[course_enrollments]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[course_enrollments](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[course_id] [int] NOT NULL,
	[user_id] [int] NOT NULL,
	[enrollment_date] [datetime2](7) NULL,
	[status] [nvarchar](20) NULL,
	[is_deleted] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[course_instructors]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[course_instructors](
	[course_id] [int] NOT NULL,
	[user_id] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[course_id] ASC,
	[user_id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[course_learners]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[course_learners](
	[course_id] [int] NOT NULL,
	[user_id] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[course_id] ASC,
	[user_id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[course_levels]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[course_levels](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[name] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[course_materials]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[course_materials](
	[course_id] [int] NOT NULL,
	[material_id] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[course_id] ASC,
	[material_id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[course_prerequisites]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[course_prerequisites](
	[course_id] [int] NOT NULL,
	[prerequisite_id] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[course_id] ASC,
	[prerequisite_id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[course_sessions]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[course_sessions](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[course_id] [int] NOT NULL,
	[training_session_id] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[courses]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[courses](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[course_definition_id] [int] NOT NULL,
	[title] [nvarchar](200) NOT NULL,
	[course_code] [nvarchar](20) NOT NULL,
	[description] [nvarchar](max) NULL,
	[category] [nvarchar](100) NOT NULL,
	[level] [nvarchar](50) NOT NULL,
	[start_date] [date] NULL,
	[end_date] [date] NULL,
	[location] [nvarchar](200) NULL,
	[status] [nvarchar](20) NULL,
	[is_deleted] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[instructors]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[instructors](
	[user_id] [int] NOT NULL,
	[bio] [nvarchar](1) NULL,
	[specialization] [nvarchar](100) NULL,
	[years_experience] [int] NULL,
	[is_deleted] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[user_id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[learner_question_answers]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[learner_question_answers](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[learner_test_question_id] [int] NOT NULL,
	[option_id] [int] NOT NULL,
	[answer_text] [nvarchar](max) NULL,
	[is_correct] [bit] NULL,
	[points] [decimal](5, 2) NULL,
	[is_deleted] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[learner_simulation_tasks]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[learner_simulation_tasks](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[learner_id] [int] NOT NULL,
	[training_session_simulation_task_id] [int] NOT NULL,
	[status] [nvarchar](20) NULL,
	[progress] [nvarchar](200) NULL,
	[feedback] [nvarchar](max) NULL,
	[started_at] [datetime2](7) NULL,
	[completed_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[learner_test_questions]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[learner_test_questions](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[learner_test_id] [int] NOT NULL,
	[question_id] [int] NOT NULL,
	[order_index] [int] NOT NULL,
	[points] [decimal](5, 2) NULL,
	[is_deleted] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[learner_tests]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[learner_tests](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[learner_id] [int] NOT NULL,
	[test_id] [int] NOT NULL,
	[attempt_number] [int] NULL,
	[score] [decimal](5, 2) NULL,
	[started_at] [datetime2](7) NULL,
	[completed_at] [datetime2](7) NULL,
	[status] [nvarchar](20) NULL,
	[review] [nvarchar](max) NULL,
	[is_deleted] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[learners]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[learners](
	[user_id] [int] NOT NULL,
	[date_of_birth] [date] NULL,
	[enrollment_status] [nvarchar](20) NULL,
	[is_deleted] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[user_id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[learning_materials]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[learning_materials](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[title] [nvarchar](200) NOT NULL,
	[content] [nvarchar](max) NULL,
	[source_url] [nvarchar](500) NULL,
	[material_type_id] [int] NOT NULL,
	[is_deleted] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[material_types]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[material_types](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[name] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[question_options]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[question_options](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[question_id] [int] NOT NULL,
	[is_correct] [bit] NULL,
	[points] [decimal](5, 2) NULL,
	[order_index] [int] NOT NULL,
	[is_deleted] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[roles]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[roles](
	[id] [tinyint] NOT NULL,
	[name] [nvarchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[name] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[session_attendances]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[session_attendances](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[trainingsession_id] [int] NOT NULL,
	[learner_id] [int] NOT NULL,
	[attendance_status] [nvarchar](10) NULL,
	[attended_at] [datetime2](7) NULL,
	[created_at] [datetime2](7) NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[session_learners]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[session_learners](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[trainingsession_id] [int] NOT NULL,
	[learner_id] [int] NOT NULL,
	[enrollment_status] [nvarchar](20) NULL,
	[created_at] [datetime2](7) NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[session_schedules]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[session_schedules](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[trainingsession_id] [int] NOT NULL,
	[schedule_date] [date] NOT NULL,
	[start_time] [time](7) NOT NULL,
	[end_time] [time](7) NOT NULL,
	[created_at] [datetime2](7) NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[session_types]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[session_types](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](100) NOT NULL,
	[description] [nvarchar](max) NULL,
	[created_at] [datetime2](7) NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[name] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[simulation_tasks]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[simulation_tasks](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[course_id] [int] NOT NULL,
	[name] [nvarchar](200) NOT NULL,
	[description] [nvarchar](max) NULL,
	[passing_criteria] [nvarchar](max) NULL,
	[created_at] [datetime2](7) NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[staff]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[staff](
	[user_id] [int] NOT NULL,
	[employment_status] [nvarchar](20) NULL,
	[is_deleted] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[user_id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[test_questions]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[test_questions](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[test_id] [int] NOT NULL,
	[is_multiple_answers] [bit] NULL,
	[option_quantity] [int] NOT NULL,
	[answer_quantity] [int] NOT NULL,
	[points] [decimal](5, 2) NULL,
	[explanation] [nvarchar](max) NULL,
	[is_deleted] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tests]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tests](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](200) NOT NULL,
	[description] [nvarchar](max) NULL,
	[duration_minutes] [int] NOT NULL,
	[total_points] [decimal](5, 2) NULL,
	[created_at] [datetime2](7) NULL,
	[updated_at] [datetime2](7) NULL,
	[is_deleted] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[training_session_simulation_tasks]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[training_session_simulation_tasks](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[training_session_id] [int] NOT NULL,
	[simulation_task_id] [int] NOT NULL,
	[assigned_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[training_sessions]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[training_sessions](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](200) NOT NULL,
	[description] [nvarchar](max) NULL,
	[session_type_id] [int] NOT NULL,
	[instructor_id] [int] NOT NULL,
	[start_date] [datetime2](7) NOT NULL,
	[end_date] [datetime2](7) NOT NULL,
	[location] [nvarchar](200) NULL,
	[created_at] [datetime2](7) NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[users]    Script Date: 9/5/2025 9:45:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[users](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[username] [nvarchar](50) NOT NULL,
	[password] [nvarchar](255) NOT NULL,
	[email] [nvarchar](100) NOT NULL,
	[fullname] [nvarchar](100) NOT NULL,
	[profile_image_url] [nvarchar](500) NULL,
	[role_id] [tinyint] NOT NULL,
	[created_at] [datetime2](7) NULL,
	[updated_at] [datetime2](7) NULL,
	[is_deleted] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[email] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[username] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_course_enrollments_status]    Script Date: 9/5/2025 9:45:42 AM ******/
CREATE NONCLUSTERED INDEX [IX_course_enrollments_status] ON [dbo].[course_enrollments]
(
	[status] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_courses_start_date]    Script Date: 9/5/2025 9:45:42 AM ******/
CREATE NONCLUSTERED INDEX [IX_courses_start_date] ON [dbo].[courses]
(
	[start_date] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_courses_status]    Script Date: 9/5/2025 9:45:42 AM ******/
CREATE NONCLUSTERED INDEX [IX_courses_status] ON [dbo].[courses]
(
	[status] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_learner_simulation_tasks_status]    Script Date: 9/5/2025 9:45:42 AM ******/
CREATE NONCLUSTERED INDEX [IX_learner_simulation_tasks_status] ON [dbo].[learner_simulation_tasks]
(
	[status] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_learner_tests_started_at]    Script Date: 9/5/2025 9:45:42 AM ******/
CREATE NONCLUSTERED INDEX [IX_learner_tests_started_at] ON [dbo].[learner_tests]
(
	[started_at] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_learner_tests_status]    Script Date: 9/5/2025 9:45:42 AM ******/
CREATE NONCLUSTERED INDEX [IX_learner_tests_status] ON [dbo].[learner_tests]
(
	[status] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_session_attendances_status]    Script Date: 9/5/2025 9:45:42 AM ******/
CREATE NONCLUSTERED INDEX [IX_session_attendances_status] ON [dbo].[session_attendances]
(
	[attendance_status] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_session_learners_status]    Script Date: 9/5/2025 9:45:42 AM ******/
CREATE NONCLUSTERED INDEX [IX_session_learners_status] ON [dbo].[session_learners]
(
	[enrollment_status] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_training_sessions_start_date]    Script Date: 9/5/2025 9:45:42 AM ******/
CREATE NONCLUSTERED INDEX [IX_training_sessions_start_date] ON [dbo].[training_sessions]
(
	[start_date] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_users_email]    Script Date: 9/5/2025 9:45:42 AM ******/
CREATE NONCLUSTERED INDEX [IX_users_email] ON [dbo].[users]
(
	[email] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_users_is_deleted]    Script Date: 9/5/2025 9:45:42 AM ******/
CREATE NONCLUSTERED INDEX [IX_users_is_deleted] ON [dbo].[users]
(
	[is_deleted] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_users_role_id]    Script Date: 9/5/2025 9:45:42 AM ******/
CREATE NONCLUSTERED INDEX [IX_users_role_id] ON [dbo].[users]
(
	[role_id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[admins] ADD  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [dbo].[course_definitions] ADD  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [dbo].[course_enrollments] ADD  DEFAULT (getdate()) FOR [enrollment_date]
GO
ALTER TABLE [dbo].[course_enrollments] ADD  DEFAULT ('enrolled') FOR [status]
GO
ALTER TABLE [dbo].[course_enrollments] ADD  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [dbo].[courses] ADD  DEFAULT ('planned') FOR [status]
GO
ALTER TABLE [dbo].[courses] ADD  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [dbo].[instructors] ADD  DEFAULT ((0)) FOR [years_experience]
GO
ALTER TABLE [dbo].[instructors] ADD  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [dbo].[learner_question_answers] ADD  DEFAULT ((0)) FOR [is_correct]
GO
ALTER TABLE [dbo].[learner_question_answers] ADD  DEFAULT ((0)) FOR [points]
GO
ALTER TABLE [dbo].[learner_question_answers] ADD  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [dbo].[learner_simulation_tasks] ADD  DEFAULT ('not_started') FOR [status]
GO
ALTER TABLE [dbo].[learner_test_questions] ADD  DEFAULT ((0)) FOR [points]
GO
ALTER TABLE [dbo].[learner_test_questions] ADD  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [dbo].[learner_tests] ADD  DEFAULT ((1)) FOR [attempt_number]
GO
ALTER TABLE [dbo].[learner_tests] ADD  DEFAULT ((0)) FOR [score]
GO
ALTER TABLE [dbo].[learner_tests] ADD  DEFAULT (getdate()) FOR [started_at]
GO
ALTER TABLE [dbo].[learner_tests] ADD  DEFAULT ('started') FOR [status]
GO
ALTER TABLE [dbo].[learner_tests] ADD  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [dbo].[learners] ADD  DEFAULT ('active') FOR [enrollment_status]
GO
ALTER TABLE [dbo].[learners] ADD  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [dbo].[learning_materials] ADD  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [dbo].[question_options] ADD  DEFAULT ((0)) FOR [is_correct]
GO
ALTER TABLE [dbo].[question_options] ADD  DEFAULT ((0)) FOR [points]
GO
ALTER TABLE [dbo].[question_options] ADD  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [dbo].[session_attendances] ADD  DEFAULT ('Present') FOR [attendance_status]
GO
ALTER TABLE [dbo].[session_attendances] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[session_attendances] ADD  DEFAULT (getdate()) FOR [updated_at]
GO
ALTER TABLE [dbo].[session_learners] ADD  DEFAULT ('Applied') FOR [enrollment_status]
GO
ALTER TABLE [dbo].[session_learners] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[session_learners] ADD  DEFAULT (getdate()) FOR [updated_at]
GO
ALTER TABLE [dbo].[session_schedules] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[session_schedules] ADD  DEFAULT (getdate()) FOR [updated_at]
GO
ALTER TABLE [dbo].[session_types] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[session_types] ADD  DEFAULT (getdate()) FOR [updated_at]
GO
ALTER TABLE [dbo].[simulation_tasks] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[simulation_tasks] ADD  DEFAULT (getdate()) FOR [updated_at]
GO
ALTER TABLE [dbo].[staff] ADD  DEFAULT ('active') FOR [employment_status]
GO
ALTER TABLE [dbo].[staff] ADD  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [dbo].[test_questions] ADD  DEFAULT ((0)) FOR [is_multiple_answers]
GO
ALTER TABLE [dbo].[test_questions] ADD  DEFAULT ((1)) FOR [points]
GO
ALTER TABLE [dbo].[test_questions] ADD  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [dbo].[tests] ADD  DEFAULT ((0)) FOR [total_points]
GO
ALTER TABLE [dbo].[tests] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[tests] ADD  DEFAULT (getdate()) FOR [updated_at]
GO
ALTER TABLE [dbo].[tests] ADD  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [dbo].[training_session_simulation_tasks] ADD  DEFAULT (getdate()) FOR [assigned_at]
GO
ALTER TABLE [dbo].[training_sessions] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[training_sessions] ADD  DEFAULT (getdate()) FOR [updated_at]
GO
ALTER TABLE [dbo].[users] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[users] ADD  DEFAULT (getdate()) FOR [updated_at]
GO
ALTER TABLE [dbo].[users] ADD  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [dbo].[admins]  WITH CHECK ADD FOREIGN KEY([user_id])
REFERENCES [dbo].[users] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[course_definitions]  WITH CHECK ADD FOREIGN KEY([category_id])
REFERENCES [dbo].[course_categories] ([id])
GO
ALTER TABLE [dbo].[course_definitions]  WITH CHECK ADD FOREIGN KEY([level_id])
REFERENCES [dbo].[course_levels] ([id])
GO
ALTER TABLE [dbo].[course_enrollments]  WITH CHECK ADD FOREIGN KEY([course_id])
REFERENCES [dbo].[courses] ([id])
GO
ALTER TABLE [dbo].[course_enrollments]  WITH CHECK ADD FOREIGN KEY([user_id])
REFERENCES [dbo].[users] ([id])
GO
ALTER TABLE [dbo].[course_instructors]  WITH CHECK ADD FOREIGN KEY([course_id])
REFERENCES [dbo].[courses] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[course_instructors]  WITH CHECK ADD FOREIGN KEY([user_id])
REFERENCES [dbo].[users] ([id])
GO
ALTER TABLE [dbo].[course_learners]  WITH CHECK ADD FOREIGN KEY([course_id])
REFERENCES [dbo].[courses] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[course_learners]  WITH CHECK ADD FOREIGN KEY([user_id])
REFERENCES [dbo].[users] ([id])
GO
ALTER TABLE [dbo].[course_materials]  WITH CHECK ADD FOREIGN KEY([course_id])
REFERENCES [dbo].[courses] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[course_materials]  WITH CHECK ADD FOREIGN KEY([material_id])
REFERENCES [dbo].[learning_materials] ([id])
GO
ALTER TABLE [dbo].[course_prerequisites]  WITH CHECK ADD FOREIGN KEY([course_id])
REFERENCES [dbo].[courses] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[course_prerequisites]  WITH CHECK ADD FOREIGN KEY([prerequisite_id])
REFERENCES [dbo].[courses] ([id])
GO
ALTER TABLE [dbo].[course_sessions]  WITH CHECK ADD FOREIGN KEY([course_id])
REFERENCES [dbo].[courses] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[course_sessions]  WITH CHECK ADD FOREIGN KEY([training_session_id])
REFERENCES [dbo].[training_sessions] ([id])
GO
ALTER TABLE [dbo].[courses]  WITH CHECK ADD FOREIGN KEY([course_definition_id])
REFERENCES [dbo].[course_definitions] ([id])
GO
ALTER TABLE [dbo].[instructors]  WITH CHECK ADD FOREIGN KEY([user_id])
REFERENCES [dbo].[users] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[learner_question_answers]  WITH CHECK ADD FOREIGN KEY([learner_test_question_id])
REFERENCES [dbo].[learner_test_questions] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[learner_question_answers]  WITH CHECK ADD FOREIGN KEY([option_id])
REFERENCES [dbo].[question_options] ([id])
GO
ALTER TABLE [dbo].[learner_simulation_tasks]  WITH CHECK ADD FOREIGN KEY([learner_id])
REFERENCES [dbo].[learners] ([user_id])
GO
ALTER TABLE [dbo].[learner_simulation_tasks]  WITH CHECK ADD FOREIGN KEY([training_session_simulation_task_id])
REFERENCES [dbo].[training_session_simulation_tasks] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[learner_test_questions]  WITH CHECK ADD FOREIGN KEY([learner_test_id])
REFERENCES [dbo].[learner_tests] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[learner_test_questions]  WITH CHECK ADD FOREIGN KEY([question_id])
REFERENCES [dbo].[test_questions] ([id])
GO
ALTER TABLE [dbo].[learner_tests]  WITH CHECK ADD FOREIGN KEY([learner_id])
REFERENCES [dbo].[learners] ([user_id])
GO
ALTER TABLE [dbo].[learner_tests]  WITH CHECK ADD FOREIGN KEY([test_id])
REFERENCES [dbo].[tests] ([id])
GO
ALTER TABLE [dbo].[learners]  WITH CHECK ADD FOREIGN KEY([user_id])
REFERENCES [dbo].[users] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[learning_materials]  WITH CHECK ADD FOREIGN KEY([material_type_id])
REFERENCES [dbo].[material_types] ([id])
GO
ALTER TABLE [dbo].[question_options]  WITH CHECK ADD FOREIGN KEY([question_id])
REFERENCES [dbo].[test_questions] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[session_attendances]  WITH CHECK ADD FOREIGN KEY([learner_id])
REFERENCES [dbo].[learners] ([user_id])
GO
ALTER TABLE [dbo].[session_attendances]  WITH CHECK ADD FOREIGN KEY([trainingsession_id])
REFERENCES [dbo].[training_sessions] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[session_learners]  WITH CHECK ADD FOREIGN KEY([learner_id])
REFERENCES [dbo].[learners] ([user_id])
GO
ALTER TABLE [dbo].[session_learners]  WITH CHECK ADD FOREIGN KEY([trainingsession_id])
REFERENCES [dbo].[training_sessions] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[session_schedules]  WITH CHECK ADD FOREIGN KEY([trainingsession_id])
REFERENCES [dbo].[training_sessions] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[simulation_tasks]  WITH CHECK ADD FOREIGN KEY([course_id])
REFERENCES [dbo].[courses] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[staff]  WITH CHECK ADD FOREIGN KEY([user_id])
REFERENCES [dbo].[users] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[test_questions]  WITH CHECK ADD FOREIGN KEY([test_id])
REFERENCES [dbo].[tests] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[training_session_simulation_tasks]  WITH CHECK ADD FOREIGN KEY([simulation_task_id])
REFERENCES [dbo].[simulation_tasks] ([id])
GO
ALTER TABLE [dbo].[training_session_simulation_tasks]  WITH CHECK ADD FOREIGN KEY([training_session_id])
REFERENCES [dbo].[training_sessions] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[training_sessions]  WITH CHECK ADD FOREIGN KEY([instructor_id])
REFERENCES [dbo].[instructors] ([user_id])
GO
ALTER TABLE [dbo].[training_sessions]  WITH CHECK ADD FOREIGN KEY([session_type_id])
REFERENCES [dbo].[session_types] ([id])
GO
ALTER TABLE [dbo].[users]  WITH CHECK ADD  CONSTRAINT [FK_users_roles] FOREIGN KEY([role_id])
REFERENCES [dbo].[roles] ([id])
GO
ALTER TABLE [dbo].[users] CHECK CONSTRAINT [FK_users_roles]
GO
ALTER TABLE [dbo].[course_definitions]  WITH CHECK ADD CHECK  (([duration]>(0)))
GO
ALTER TABLE [dbo].[course_enrollments]  WITH CHECK ADD CHECK  (([status]='failed' OR [status]='dropped' OR [status]='completed' OR [status]='enrolled'))
GO
ALTER TABLE [dbo].[course_prerequisites]  WITH CHECK ADD CHECK  (([course_id]<>[prerequisite_id]))
GO
ALTER TABLE [dbo].[courses]  WITH CHECK ADD CHECK  (([end_date]>=[start_date] OR [end_date] IS NULL))
GO
ALTER TABLE [dbo].[courses]  WITH CHECK ADD CHECK  (([status]='cancelled' OR [status]='completed' OR [status]='ongoing' OR [status]='planned'))
GO
ALTER TABLE [dbo].[instructors]  WITH CHECK ADD CHECK  (([years_experience]>=(0)))
GO
ALTER TABLE [dbo].[learner_question_answers]  WITH CHECK ADD CHECK  (([points]>=(0)))
GO
ALTER TABLE [dbo].[learner_simulation_tasks]  WITH CHECK ADD CHECK  (([status]='failed' OR [status]='completed' OR [status]='in_progress' OR [status]='not_started'))
GO
ALTER TABLE [dbo].[learner_simulation_tasks]  WITH CHECK ADD CHECK  (([completed_at]>=[started_at] OR [completed_at] IS NULL))
GO
ALTER TABLE [dbo].[learner_test_questions]  WITH CHECK ADD CHECK  (([order_index]>=(1)))
GO
ALTER TABLE [dbo].[learner_test_questions]  WITH CHECK ADD CHECK  (([points]>=(0)))
GO
ALTER TABLE [dbo].[learner_tests]  WITH CHECK ADD CHECK  (([attempt_number]>=(1)))
GO
ALTER TABLE [dbo].[learner_tests]  WITH CHECK ADD CHECK  (([score]>=(0)))
GO
ALTER TABLE [dbo].[learner_tests]  WITH CHECK ADD CHECK  (([status]='abandoned' OR [status]='completed' OR [status]='in_progress' OR [status]='started'))
GO
ALTER TABLE [dbo].[learner_tests]  WITH CHECK ADD CHECK  (([completed_at]>=[started_at] OR [completed_at] IS NULL))
GO
ALTER TABLE [dbo].[learners]  WITH CHECK ADD CHECK  (([enrollment_status]='suspended' OR [enrollment_status]='inactive' OR [enrollment_status]='active'))
GO
ALTER TABLE [dbo].[question_options]  WITH CHECK ADD CHECK  (([order_index]>=(1)))
GO
ALTER TABLE [dbo].[question_options]  WITH CHECK ADD CHECK  (([points]>=(0)))
GO
ALTER TABLE [dbo].[session_attendances]  WITH CHECK ADD CHECK  (([attendance_status]='Late' OR [attendance_status]='Absent' OR [attendance_status]='Present'))
GO
ALTER TABLE [dbo].[session_learners]  WITH CHECK ADD CHECK  (([enrollment_status]='Rejected' OR [enrollment_status]='Approved' OR [enrollment_status]='Applied'))
GO
ALTER TABLE [dbo].[session_schedules]  WITH CHECK ADD CHECK  (([end_time]>[start_time]))
GO
ALTER TABLE [dbo].[staff]  WITH CHECK ADD CHECK  (([employment_status]='on_leave' OR [employment_status]='inactive' OR [employment_status]='active'))
GO
ALTER TABLE [dbo].[test_questions]  WITH CHECK ADD CHECK  (([answer_quantity]>=(1)))
GO
ALTER TABLE [dbo].[test_questions]  WITH CHECK ADD CHECK  (([option_quantity]>=(2)))
GO
ALTER TABLE [dbo].[test_questions]  WITH CHECK ADD CHECK  (([points]>=(0)))
GO
ALTER TABLE [dbo].[test_questions]  WITH CHECK ADD CHECK  (([answer_quantity]<=[option_quantity]))
GO
ALTER TABLE [dbo].[tests]  WITH CHECK ADD CHECK  (([duration_minutes]>(0)))
GO
ALTER TABLE [dbo].[tests]  WITH CHECK ADD CHECK  (([total_points]>=(0)))
GO
ALTER TABLE [dbo].[training_sessions]  WITH CHECK ADD CHECK  (([end_date]>[start_date]))
GO
ALTER DATABASE [lssctc_db] SET  READ_WRITE 
GO
