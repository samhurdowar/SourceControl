SELECT * FROM DbEntity
SELECT * FROM PageTemplate WHERE PageTemplateId = 2126

SELECT * FROM ColumnDef WHERE PageTemplateId = 2126

SELECT * FROM GridColumn WHERE PageTemplateId = 2126 ORDER BY SortOrder

SELECT * FROM SiteSettings where SettingName = 'DownloadHPNAData'




UPDATE PageTemplate SET Layout = REPLACE(CAST(Layout AS varchar(max)), 'P' + CAST(PageTemplateId AS varchar(20))+ '_', TableName + '_')

UPDATE PageTemplate SET ViewLayout = REPLACE(CAST(ViewLayout AS varchar(max)), 'P' + CAST(PageTemplateId AS varchar(20))+ '_', TableName + '_')


<a href="javascript:CopyDeviceRMA()"><img src="/Images/ToolBar/clip_copy.png" /><span> RMA</span></a>

ALTER TABLE PageTemplate DROP COLUMN FormCommand

ALTER TABLE PageTemplate DROP COLUMN FormScript


ALTER TABLE PageTemplate ADD ViewFormStyle varchar(max) null
ALTER TABLE PageTemplate ADD ViewFormCommand varchar(max) null
ALTER TABLE PageTemplate ADD ViewFormBody varchar(max) null
ALTER TABLE PageTemplate ADD ViewFormScript varchar(max) null

ALTER TABLE PageTemplate ADD EditFormStyle varchar(max) null
ALTER TABLE PageTemplate ADD EditFormCommand varchar(max) null
ALTER TABLE PageTemplate ADD EditFormBody varchar(max) null
ALTER TABLE PageTemplate ADD EditFormScript varchar(max) null

ALTER TABLE PageTemplate ADD SearchFormStyle varchar(max) null
ALTER TABLE PageTemplate ADD SearchFormCommand varchar(max) null
ALTER TABLE PageTemplate ADD SearchFormBody varchar(max) null
ALTER TABLE PageTemplate ADD SearchFormScript varchar(max) null

ALTER TABLE PageTemplate DROP COLUMN ReportGridColumns
ALTER TABLE PageTemplate DROP COLUMN EntityName

update PageTemplate set ViewFormStyle = 'aaa',ViewFormCommand = 'bbb', ViewFormBody = 'ccc', ViewFormScript = 'ddd'


<div class="command-bar">
    <span id="cmdXXX" formname="FormXXX" class="command-disabled-span"><span class="command-icon fas fa-save">&nbsp;</span> Save</span>
    <span class="command-active-span" onclick="xxx()"><span class="command-icon fas fa-plus-square">&nbsp;</span> Add</span>
	<span class="command-active-span" onclick="xxx()"><span class="command-icon fas fa-edit">&nbsp;</span> Edit</span>
    <span class="command-active-span" onclick="xxx()"><span class="command-icon fas fa-trash-alt">&nbsp;</span> Delete</span>
	<span class="command-active-span" onclick="xxx()"><span class="command-icon fas fa-sync">&nbsp;</span> Refresh</span>
	<span class="command-active-span" onclick="xxx()"><span class="command-icon fas fa-window-close">&nbsp;</span> Cancel</span>
	<span class="command-active-span" onclick="xxx()"><span class="command-icon fas fa-file-download">&nbsp;</span> Download</span>
	<span class="command-active-span" onclick="xxx()"><span class="command-icon fas fa-search-minus">&nbsp;</span> Clear Filter</span>
	<span class="command-active-span" onclick="xxx()"><span class="command-icon fas fa-tasks">&nbsp;</span> Load Layout</span>
	<span class="command-active-span" onclick="xxx()"><span class="command-icon fas fa-copy">&nbsp;</span> Copy</span>
	<span class="command-active-span" onclick="xxx()"><span class="command-icon fas fa-search">&nbsp;</span> Search</span>


</div>

BindButtonToForm("cmdSaveViewLayout");


USE [CMDB]

truncate table FileAttachment
truncate table ChangeHistory
truncate table Note
truncate table DebugLog

create table CustomOption (
CustomOptionId int not null primary key identity(1,1),
ColumnDefId int not null default 0,
OptionText  varchar(500) null,
OptionValue varchar(500) null

)
Error - Length cannot be less than zero.  Parameter name: length  DataService.GetJsonObject  finalString=[ 

select * from PageTemplate

select distinct datatype from ColumnDef where PageTemplateId = 65

select * from DebugLog 

select * from FileAttachment

select * from Note

select * from ChangeHistory

delete from DebugLog where DebugLogId = 82

Error - Incorrect syntax near '2018'.  DataService.GetJsonObject  finalString=

update DebugLog set DateOnly = getdate()



--  drop table InventoryImport
select * from InventoryImport


delete from InventoryImport where InventoryImportId =  634

select HostName, count(1) from InventoryImport group by HostName having count(1) > 1

select * from InventoryImport where SerialNumber = 'f5-weca-sofo'

select DeviceModel, count(1) from InventoryImport group by DeviceModel
select DeviceType, count(1) from InventoryImport group by DeviceType



truncate table Host
truncate table ChangeHistory1065
insert into Host(StatusId, History, HostName, DeviceIp,SerialNumber_) select 1, 0, HostName, DeviceIp,SerialNumber  from InventoryImport
UPDATE T1 SET T1.SerialNumberId = T2.SerialNumberId  FROM Host T1 JOIN SerialNumber T2 ON T1.SerialNumber_ = T2.SerialNumber
select * from Host
update host set history = 0
create function [dbo].[GetSerialNumberById](@SerialNumberId int) returns varchar(250)
AS
BEGIN
  DECLARE @Ret varchar(250) = ISNULL((Select SerialNumber FROM SerialNumber where SerialNumberId = @SerialNumberId), '')
  Return @Ret
END

alter table host add SerialNumber as [dbo].[GetSerialNumberById](SerialNumberId)





truncate table DeviceModel
insert into DeviceModel(DeviceModel) select distinct DeviceModel from InventoryImport where LEN(DeviceModel) > 2
select * from DeviceModel


CREATE TABLE DeviceType (
	DeviceTypeId int not null primary key identity (1,1),
	DeviceType  varchar(250) null
)

truncate table DeviceType
insert into DeviceType(DeviceType) select distinct DeviceType from InventoryImport where LEN(DeviceType) > 2
select * from DeviceType


truncate table Vendor
insert into Vendor(VendorSupportContact) select distinct DeviceVendor from InventoryImport where LEN(DeviceVendor) > 1
select * from Vendor


truncate table SerialNumber
insert into SerialNumber(StatusId, SerialNumber, DeviceType, DeviceModel, DeviceVendor) select 1, SerialNumber, DeviceType, DeviceModel, DeviceVendor from InventoryImport
delete from SerialNumber where LEN(SerialNumber) = 0
select * from SerialNumber --where VendorId = 0

UPDATE T1 SET T1.VendorId = T2.VendorId  FROM SerialNumber T1 JOIN Vendor T2 ON T1.DeviceVendor = T2.VendorSupportContact
UPDATE T1 SET T1.DeviceModelId = T2.DeviceModelId  FROM SerialNumber T1 JOIN DeviceModel T2 ON T1.DeviceModel_ = T2.DeviceModel
UPDATE T1 SET T1.DeviceTypeId = T2.DeviceTypeId  FROM SerialNumber T1 JOIN DeviceType T2 ON T1.DeviceType_ = T2.DeviceType

UPDATE T1 SET T1.HostId = T2.HostId  FROM SerialNumber T1 JOIN Host T2 ON T1.SerialNumber = T2.SerialNumber_



create function [dbo].[GetDeviceModelById](@DeviceModelId int) returns varchar(250)
AS
BEGIN
  DECLARE @Ret varchar(250) = ISNULL((Select DeviceModel FROM DeviceModel where DeviceModelId = @DeviceModelId), '')
  Return @Ret
END

alter table SerialNumber add DeviceModel as [dbo].[GetDeviceModelById](DeviceModelId)


create function [dbo].[GetDeviceTypeById](@DeviceTypeId int) returns varchar(250)
AS
BEGIN
  DECLARE @Ret varchar(250) = ISNULL((Select DeviceType FROM DeviceType where DeviceTypeId = @DeviceTypeId), '')
  Return @Ret
END

alter table SerialNumber add DeviceType as [dbo].[GetDeviceTypeById](DeviceTypeId)


create function [dbo].[GetVendorById](@VendorId int) returns varchar(250)
AS
BEGIN
  DECLARE @Ret varchar(250) = ISNULL((Select VendorSupportContact FROM Vendor where VendorId = @VendorId), '')
  Return @Ret
END

alter table SerialNumber add Vendor as [dbo].[GetVendorById](VendorId)







update SerialNumber set DeviceType = '1', DeviceModel = '1'

update [ColumnDef] set ElementTYpe = 'Textbox', ElementWidth = 150 where pagetemplateid = 1285

update PageTemplate set TemplateName = 'Status', TableName = 'Status' where PageTemplateId = 1068

update PageTemplate set TemplateName = 'Serial Number', TableName = 'SerialNumber' where PageTemplateId = 1066

update ColumnDef set ColumnName = 'HostNoteId', DisplayName = 'HostNoteId' where ColumnDefId = 1290


delete from columndef where ColumnDefId in (1282,1284)

select top 24 * from DOMS

select MountTYpe,city,Building,Room,count(1) from DOMS group by MountTYpe,city,Building,Room

select city,count(1) from DOMS group by City

select Building,count(1) from DOMS group by Building

select Room,count(1) from DOMS group by Room


select * from HostNote

select * from Role


update ColumnDef set AddBlankOption = 1, ColumnName = 'DeviceTypeId', DisplayName = 'DeviceTypeId', ElementType = 'DropdownSimple', LookupTable = 'DeviceType', ValueField = 'DeviceTypeId', TextField = 'DeviceType', OrderField = 'DeviceType ASC'  where ColumnDefId = 1303
update ColumnDef set AddBlankOption = 1, ColumnName = 'DeviceModelId', DisplayName = 'DeviceModelId', ElementType = 'DropdownSimple', LookupTable = 'DeviceModel', ValueField = 'DeviceModelId', TextField = 'DeviceModel', OrderField = 'DeviceModel ASC' where ColumnDefId = 1304

update ColumnDef set AddBlankOption = 1 where ColumnDefId = 1413


select * from ColumnDef where PageTemplateId = 1065

select * from PageTemplate

select * from Status

select * from Host

select * from SerialNumber where SerialNumberId = 362

update SerialNumber set ContractEndDate = null, InServiceDate = null, OutOfServiceDate = null


alter table Note add InsertByName as [dbo].[GetUserFullNameById](InsertBy)


SELECT * FROM ChangeHistory

update ChangeHistory set PageTemplateId = 1065

CREATE TABLE AnsibleGroup(
	AnsibleGroupId int not null primary key identity(1,1),
	AnsibleGroup  varchar(250) null
)

INSERT INTO Test(UserIds,UserIds_Text) VALUES('2,5,8','A, B, C')

update Host set ActiveInNNMi = '1'
update Host set ActiveInNA = '1'
update Host set ActiveInNetBrain = '1'
update Host set ActiveInAssetManager = '1'
update Host set ActiveInNetMRI = '1'

update SerialNumber set site = '5'

truncate table FileAttachment

SELECT * FROM FileAttachment

update FileAttachment set PageTemplateId = 1065, RecordId = 362

WITH MAIN AS (SELECT DebugLog.DebugLogId FROM DebugLog    ORDER BY DebugLog.DebugLogId DESC   OFFSET 0 ROWS FETCH NEXT 100 ROWS ONLY ) 


SELECT ISNULL(CONVERT(VARCHAR(25), DebugLog.LogDate, 22),'') FROM DebugLog 

SELECT ISNULL(CONVERT(VARCHAR(25), DebugLog.LogDate, 101),'') FROM DebugLog 




INNER JOIN MAIN ON DebugLog.DebugLogId = MAIN.DebugLogId  ORDER BY DebugLog.DebugLogId DESC   





WITH MAIN AS (SELECT Host.HostId, OptionText AS StatusId_lco  FROM Host  LEFT JOIN CustomOption ON Host.StatusId = CustomOption.OptionValue    ORDER BY CustomOption.OptionText asc  OFFSET 0 ROWS FETCH NEXT 100 ROWS ONLY ) SELECT  '{ "HostId": "' + CAST(ISNULL(Host.HostId,'') AS varchar(50)) + '", "Hostname": "' + CAST(ISNULL(Host.Hostname,'') AS varchar(250)) + '", "StatusId_lco": "' + CAST(ISNULL(MAIN.StatusId_lco,'') AS varchar(50)) + '", "DateChanged": "' + ISNULL(CONVERT(VARCHAR(25), Host.DateChanged, 22),'') + '", "LocationSite": "' + CAST(ISNULL(Host.LocationSite,'') AS varchar(250)) + '", "SerialNumber": "' + CAST(ISNULL(Host.SerialNumber,'') AS varchar(250)) + '", "DeviceIp": "' + CAST(ISNULL(Host.DeviceIp,'') AS varchar(100)) + '", "AnsibleGroup_Text": "' + CAST(ISNULL(Host.AnsibleGroup_Text,'') AS varchar(500)) + '" }, '  FROM Host INNER JOIN MAIN ON Host.HostId = MAIN.HostId  ORDER BY MAIN.StatusId_lco asc  


