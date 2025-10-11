

-- Drop Database

ALTER DATABASE [lssctc-db] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE [lssctc-db];

-- 1. Drop all Foreign Key Constraints
DECLARE @sql NVARCHAR(MAX) = N'';

SELECT @sql += N'
ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) 
    + ' DROP CONSTRAINT ' + QUOTENAME(fk.name) + ';'
FROM sys.foreign_keys AS fk
INNER JOIN sys.tables AS t ON fk.parent_object_id = t.object_id
INNER JOIN sys.schemas AS s ON t.schema_id = s.schema_id;

EXEC sp_executesql @sql;

-- 2. Drop all Tables
SET @sql = N'';

SELECT @sql += N'
DROP TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ';'
FROM sys.tables AS t
INNER JOIN sys.schemas AS s ON t.schema_id = s.schema_id;

EXEC sp_executesql @sql;




DROP TABLE [dbo].[simulation_component_types];
