USE [lssctc-db]


SELECT TOP (1000) [id]
      ,[username]
      ,[password]
      ,[email]
      ,[fullname]
      ,[role]
      ,[phone_number]
      ,[avatar_url]
      ,[is_active]
      ,[is_deleted]
  FROM [dbo].[users]


SELECT TOP (1000) [id]
      ,[name]
  FROM [dbo].[learning_material_types]



  SELECT TOP (1000) [id]
      ,[name]
      ,[start_date]
      ,[end_date]
      ,[capacity]
      ,[program_course_id]
      ,[class_code_id]
      ,[description]
      ,[status]
  FROM [dbo].[classes]

  SELECT TOP (1000) [id]
      ,[class_id]
      ,[instructor_id]
      ,[position]
  FROM [dbo].[class_instructors]


SELECT TOP (1000) [id]
      ,[name]
      ,[description]
      ,[classes_id]
      ,[syllabus_section_id]
      ,[duration_minutes]
      ,[order]
      ,[start_date]
      ,[end_date]
      ,[status]
FROM [dbo].[sections]

SELECT TOP (1000) [id]
      ,[section_partition_id]
      ,[practice_id]
      ,[custom_deadline]
      ,[custom_description]
      ,[status]
      ,[is_active]
      ,[is_deleted]
  FROM [dbo].[section_practices]


SELECT TOP (1000) [id]
      ,[description]
      ,[name]
      ,[pass_criteria]
      ,[is_action_required]
  FROM [dbo].[section_partition_types]


SELECT TOP (1000) [id]
      ,[section_id]
      ,[name]
      ,[training_progress_id]
      ,[section_name]
      ,[is_completed]
      ,[is_trainee_attended]
      ,[progress]
FROM [dbo].[learning_records]

SELECT TOP (1000) [id]
      ,[section_partition_id]
      ,[name]
      ,[learning_record_id]
      ,[description]
      ,[updated_at]
      ,[completed_at]
      ,[started_at]
      ,[is_complete]
      ,[record_partition_order]
  FROM [dbo].[learning_record_partitions]

  SELECT TOP (1000) [id]
      ,[name]
      ,[pass_score_criteria]
      ,[created_at]
      ,[updated_at]
      ,[timelimit_minute]
      ,[total_score]
      ,[description]
  FROM [dbo].[quizzes]

SELECT TOP (1000) [id]
    ,[course_member_id]
    ,[status]
    ,[progress_percentage]
    ,[start_date]
    ,[last_updated]
    ,[name]
    ,[description]
FROM [dbo].[training_progresses]


SELECT TOP (1000) [id]
      ,[trainee_id]
      ,[class_id]
      ,[assigned_date]
      ,[status]
FROM [dbo].[class_members]


SELECT TOP (1000) [id]
    ,[name]
    ,[description]
    ,[category_id]
    ,[level_id]
    ,[price]
    ,[is_active]
    ,[is_deleted]
    ,[image_url]
    ,[duration_hours]
    ,[course_code_id]
FROM [dbo].[courses]



SELECT TOP (1000) [id]
      ,[name]
      ,[description]
      ,[classes_id]
      ,[syllabus_section_id]
      ,[duration_minutes]
      ,[order]
      ,[start_date]
      ,[end_date]
      ,[status]
FROM [dbo].[sections]



SELECT TOP (1000) [id]
      ,[syllabus_id]
      ,[section_title]
      ,[section_description]
      ,[section_order]
      ,[estimated_duration_minutes]
      ,[is_deleted]
FROM [dbo].[syllabus_sections]

SELECT TOP (1000) [id]
      ,[name]
      ,[course_name]
      ,[course_code]
      ,[description]
      ,[is_active]
      ,[is_deleted]
  FROM [dbo].[syllabuses]


  SELECT TOP (1000) [id]
      ,[section_quiz_id]
      ,[name]
      ,[attempt_score]
      ,[learning_record_partition_id]
      ,[max_score]
      ,[quiz_attempt_date]
      ,[status]
      ,[attempt_order]
      ,[is_pass]
  FROM [dbo].[section_quiz_attempts]


  SELECT TOP (1000) [id]
      ,[section_practice_id]
      ,[learning_record_partition_id]
      ,[score]
      ,[attempt_date]
      ,[attempt_status]
      ,[description]
      ,[is_pass]
      ,[is_deleted]
  FROM [dbo].[section_practice_attempts]

  SELECT TOP (1000) [id]
      ,[attempt_id]
      ,[practice_step_id]
      ,[score]
      ,[description]
      ,[is_pass]
      ,[is_deleted]
  FROM [dbo].[section_practice_attempt_steps]




  ------------------------------------------------------------------ SIMULATION

  SELECT TOP (1000) [id]
      ,[name]
      ,[description]
      ,[image_url]
      ,[is_active]
      ,[created_date]
      ,[is_deleted]
  FROM [dbo].[simulation_components]

  SELECT TOP (1000) [id]
      ,[name]
      ,[description]
      ,[action_key]
      ,[is_active]
      ,[is_deleted]
  FROM [dbo].[sim_actions]


  SELECT TOP (1000) [id]
      ,[practice_name]
      ,[practice_description]
      ,[estimated_duration_minutes]
      ,[difficulty_level]
      ,[max_attempts]
      ,[created_date]
      ,[is_active]
      ,[is_deleted]
  FROM [dbo].[practices]


SELECT TOP (1000) [id]
    ,[practice_id]
    ,[step_name]
    ,[step_description]
    ,[expected_result]
    ,[step_order]
    ,[is_deleted]
FROM [dbo].[practice_steps]

SELECT TOP (1000) [id]
      ,[step_id]
      ,[component_id]
      ,[component_order]
      ,[is_deleted]
  FROM [dbo].[practice_step_components]


  SELECT TOP (1000) [id]
      ,[step_id]
      ,[action_id]
      ,[name]
      ,[description]
      ,[is_deleted]
  FROM [dbo].[practice_step_actions]





DELETE FROM [dbo].[learning_records];
DELETE FROM [dbo].[training_progresses];