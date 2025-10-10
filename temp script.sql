SELECT TOP (1000) [id]
      ,[name]
  FROM [dbo].[learning_material_types]


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




DELETE FROM [dbo].[learning_records];
DELETE FROM [dbo].[training_progresses];