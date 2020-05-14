/**
SELECT * FROM DbEntity

SELECT * FROM ColumnDef

SELECT * FROM PageTemplate

sp_help 'ColumnDef'

SELECT * FROM GridColumn


ALTER TABLE ColumnDef
ADD CONSTRAINT fk_PageTemplate_ColumnDef
    FOREIGN KEY (PageTemplateId)
    REFERENCES PageTemplate (PageTemplateId)
    ON DELETE CASCADE;

GO

ALTER TABLE PageTemplate
ADD CONSTRAINT fk_DbEntity_PageTemplate
    FOREIGN KEY (DbEntityId)
    REFERENCES DbEntity (DbEntityId)
    ON DELETE CASCADE;

GO

ALTER TABLE ColumnDef ADD IsIdentity bit not null default 0
ALTER TABLE ColumnDef ADD IsRequired bit not null default 0
ALTER TABLE ColumnDef ADD IsComputed bit not null default 0
ALTER TABLE ColumnDef ADD IsPrimaryKey bit not null default 0
ALTER TABLE ColumnDef ADD DataLength int not null default 0
ALTER TABLE ColumnDef ADD DefaultValue varchar(250) not null default ''
ALTER TABLE ColumnDef ADD DataType varchar(250) not null default ''

UPDATE DbEntity SET ConnectionString = 'data source=localhost;initial catalog=NetworkToolbox;persist security info=True;Trusted_Connection=Yes;MultipleActiveResultSets=True;' WHERE DbEntityId = 1
GO
UPDATE DbEntity SET ConnectionString = 'data source=localhost;initial catalog=NetworkCafe;persist security info=True;Trusted_Connection=Yes;MultipleActiveResultSets=True;' WHERE DbEntityId = 2
GO
UPDATE DbEntity SET ConnectionString = 'data source=localhost;initial catalog=f5data;persist security info=True;Trusted_Connection=Yes;MultipleActiveResultSets=True;' WHERE DbEntityId = 3
GO
UPDATE DbEntity SET ConnectionString = 'data source=localhost;initial catalog=CMDB;persist security info=True;Trusted_Connection=Yes;MultipleActiveResultSets=True;' WHERE DbEntityId = 6
GO


**/




/****** Tables ******/
IF OBJECT_ID('dbo.GridColumn', 'U') IS NOT NULL DROP TABLE dbo.GridColumn;
GO


IF OBJECT_ID('GetDisplayNameById') IS NOT NULL DROP FUNCTION GetDisplayNameById
GO
CREATE TABLE GridColumn (
	GridColumnId  int primary key identity(1,1),

	PageTemplateId  int not null,
	ColumnDefId  int not null,
	SortOrder  int not null
)

GO
ALTER TABLE GridColumn
ADD CONSTRAINT fk_PageTemplate_GridColumn
    FOREIGN KEY (PageTemplateId)
    REFERENCES PageTemplate (PageTemplateId)
    ON DELETE CASCADE;

GO


CREATE function [dbo].[GetDisplayNameById](@ColumnDefId int) returns varchar(500)
AS
BEGIN
  DECLARE @Ret varchar(500) = ISNULL((Select DisplayName FROM ColumnDef where ColumnDefId = @ColumnDefId), '')
  Return @Ret
END

GO
ALTER TABLE GridColumn ADD DisplayName AS dbo.GetDisplayNameById(ColumnDefId)

GO
ALTER TABLE GridColumn ADD ColumnName AS dbo.GetColumnNameById(ColumnDefId)

GO

DELETE FROM ColumnDef WHERE PageTemplateId IN (SELECT PageTemplateId FROM PageTemplate WHERE TemplateName = 'Abstract Table')
GO
DELETE FROM PageTemplate WHERE TemplateName = 'Abstract Table'
GO
DELETE FROM ColumnDef WHERE PageTemplateId NOT IN (SELECT PageTemplateId FROM PageTemplate)
GO




                        SELECT c.name AS ColumnName, ISNULL(c.column_id,0) AS ColumnOrder, CAST(ISNULL(c.max_length,0) AS int) AS DataLength,
                        CAST(ISNULL(CASE c.is_identity WHEN 1 THEN 1 ELSE 0 END, 0) AS Bit) AS IsIdentity,
                        CAST(ISNULL(CASE c.is_nullable WHEN 1 THEN 0 ELSE 1 END, 0) AS Bit) AS IsRequired,
                        CAST(ISNULL(CASE c.is_computed WHEN 1 THEN 1 ELSE 0 END, 0) AS Bit) AS IsComputed,
                        ISNULL(d.definition,'') AS DefaultValue,
                        CASE
	                        WHEN (system_type_id = 35)  THEN 'TEXT'
	                        WHEN (system_type_id = 36)  THEN 'TEXT'
	                        WHEN (system_type_id = 40)  THEN 'DATE'
	                        WHEN (system_type_id = 41)  THEN 'DATE'
	                        WHEN (system_type_id = 42)  THEN 'DATE'
	                        WHEN (system_type_id = 48)  THEN 'NUMBER'
	                        WHEN (system_type_id = 52)  THEN 'NUMBER'
	                        WHEN (system_type_id = 56)  THEN 'NUMBER'
	                        WHEN (system_type_id = 58)  THEN 'DATE'
	                        WHEN (system_type_id = 59)  THEN 'NUMBER'
	                        WHEN (system_type_id = 60)  THEN 'CURRENCY'
	                        WHEN (system_type_id = 61)  THEN 'DATETIME'
	                        WHEN (system_type_id = 62)  THEN 'NUMBER'
	                        WHEN (system_type_id = 98)  THEN 'TEXT'
	                        WHEN (system_type_id = 99)  THEN 'TEXT'
	                        WHEN (system_type_id = 104) THEN 'BOOLEAN'
	                        WHEN (system_type_id = 106) THEN 'DECIMAL'
	                        WHEN (system_type_id = 108) THEN 'NUMBER'
	                        WHEN (system_type_id = 122) THEN 'CURRENCY'
	                        WHEN (system_type_id = 127) THEN 'NUMBER'
	                        WHEN (system_type_id = 165) THEN 'TEXT'
	                        WHEN (system_type_id = 167) THEN 'TEXT'
	                        WHEN (system_type_id = 173) THEN ''
	                        WHEN (system_type_id = 175) THEN 'TEXT'
	                        WHEN (system_type_id = 189) THEN 'DATE'
	                        WHEN (system_type_id = 231) THEN 'TEXT'
	                        WHEN (system_type_id = 239) THEN 'TEXT'
	                        WHEN (system_type_id = 241) THEN 'TEXT'
                        END
                        AS DataType,
                        CAST(
                            CASE
	                            WHEN (i.COLUMN_NAME = c.name) THEN 1
	                            ELSE 0
                            END
                        AS Bit) 
                        AS IsPrimaryKey

                        FROM sys.columns c JOIN sys.objects o ON o.object_id = c.object_id AND o.type = 'U' 
                        LEFT JOIN sys.default_constraints d ON d.object_id = c.default_object_id
                        LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i ON i.TABLE_NAME = o.name 