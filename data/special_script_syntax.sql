


DROP TABLE [dbo].[simulation_component_types];


ALTER TABLE [sim_settings]
ADD [source_url] NVARCHAR(1000) null

UPDATE dbo.final_exam_partials
SET exam_code = UPPER(LEFT(REPLACE(CONVERT(NVARCHAR(36), NEWID()), '-', ''), 8));


  ALTER TABLE practices
DROP CONSTRAINT DF__practices__pract__03FB8544;

ALTER TABLE practices
ADD CONSTRAINT DF_practices_practice_code
DEFAULT('NO PRACTICE CODE ASSIGNED') FOR practice_code;

update [final_exams]
set [total_marks] = 10
where [total_marks] = 100


INSERT INTO [dbo].[course_certificates] ([course_id], [certificate_id], [passing_score], [is_active])
VALUES (2, 1, 5.0, 1);

DELETE qaq
FROM dbo.quiz_attempt_questions qaq
JOIN dbo.quiz_attempts qa2
    ON qaq.quiz_attempt_id = qa2.id
WHERE qa2.max_score <> 10
   OR qa2.max_score IS NULL;





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
