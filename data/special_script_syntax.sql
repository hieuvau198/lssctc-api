


DROP TABLE [dbo].[simulation_component_types];


ALTER TABLE [dbo].[course_sections]
ADD [section_order] INT NOT NULL DEFAULT 0;


SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'practices'
  AND COLUMN_NAME = 'practice_code';

  ALTER TABLE practices
DROP CONSTRAINT DF__practices__pract__03FB8544;

ALTER TABLE practices
ADD CONSTRAINT DF_practices_practice_code
DEFAULT('NO PRACTICE CODE ASSIGNED') FOR practice_code;

update [final_exams]
set [exam_code] = 'LSSCTC_FE_565'


INSERT INTO [dbo].[course_certificates] ([course_id], [certificate_id], [passing_score], [is_active])
VALUES (2, 1, 5.0, 1);



BEGIN TRY
    BEGIN TRANSACTION;

    -- 1. Delete quiz_attempt_answers
    DELETE qaa
    FROM dbo.quiz_attempt_answers qaa
    INNER JOIN dbo.quiz_attempt_questions qaq
        ON qaa.quiz_attempt_question_id = qaq.id
    INNER JOIN dbo.quiz_attempts qa
        ON qaq.quiz_attempt_id = qa.id
    WHERE qa.max_score <> 10
      AND qa.is_current = 0;

    -- 2. Delete quiz_attempt_questions
    DELETE qaq
    FROM dbo.quiz_attempt_questions qaq
    INNER JOIN dbo.quiz_attempts qa
        ON qaq.quiz_attempt_id = qa.id
    WHERE qa.max_score <> 10
      AND qa.is_current = 0;

    -- 3. Delete quiz_attempts
    DELETE qa
    FROM dbo.quiz_attempts qa
    WHERE qa.max_score <> 10
      AND qa.is_current = 0;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;

    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR (@ErrorMessage, 16, 1);
END CATCH;


BEGIN TRY
    BEGIN TRANSACTION;

    -- 1. Delete practice_attempt_tasks
    DELETE pat
    FROM dbo.practice_attempt_tasks pat
    INNER JOIN dbo.practice_attempts pa
        ON pat.practice_attempt_id = pa.id
    WHERE pa.is_current = 0;

    -- 2. Delete practice_attempts
    DELETE pa
    FROM dbo.practice_attempts pa
    WHERE pa.is_current = 0;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;

    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR (@ErrorMessage, 16, 1);
END CATCH;
