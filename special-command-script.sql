-- Drop all foreign key constraints
DECLARE @sql NVARCHAR(MAX) = N'';

SELECT @sql += 'ALTER TABLE [' + s.name + '].[' + t.name + '] DROP CONSTRAINT [' + f.name + '];' + CHAR(13)
FROM sys.foreign_keys f
INNER JOIN sys.tables t ON f.parent_object_id = t.object_id
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id;

EXEC sp_executesql @sql;

-- Drop all tables
SET @sql = N'';

SELECT @sql += 'DROP TABLE [' + s.name + '].[' + t.name + '];' + CHAR(13)
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id;

EXEC sp_executesql @sql;
