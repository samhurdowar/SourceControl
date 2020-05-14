--SELECT * FROM DbEntity
--INSERT INTO DbEntity(EntityName,DisplayName,ConnectionString) VALUES ('CmdbEntities','CMDB Database','')


USE CMDB
GO


declare @SQL nvarchar(max)

set @SQL = 
  (
  select 'DROP TABLE  [' + SCHEMA_NAME(schema_id) + '].[' + name +'];'
  from sys.tables --where SCHEMA_NAME(schema_id) not like '%dbo%'
  for xml path('')
  )

exec (@SQL)


GO
CREATE TABLE Device (
	DeviceId       int primary key identity(1,1),
	HostName       varchar(250) null,
	MfgId          int not null default 0,
	Serial         varchar(250) null,
	DeviceSiteId   int not null default 0,
	PoInfoId       int not null default 0,
	MgtIp          varchar(250) null,
	DeviceModelId   int not null default 0,
	DeviceTypeId    int not null default 0,
	RackId         int not null default 0,
	DoB            date null,
	IsVirtual       bit not null default 0,
	Wlp             varchar(250) null,
	DeviceStatusId   int not null default 0,
	RuNumber         int null,
	DoD              date null,
	FismaTag          varchar(500) null,
	ProvisionNnmi      bit not null default 0,
	ProvisionNetworkAutomation   bit not null default 0,
	ProvisionSolarWind           bit not null default 0,
	ProvisionAlgosecFiremon      bit not null default 0,
	ProvisionNetBrain            bit not null default 0,
	DeviceNote                   varchar(500) null
)
GO

CREATE TABLE Mfg (
	MfgId           int primary key identity(1,1),
	MfgName         varchar(250) null
)
GO
INSERT INTO Mfg(MfgName) VALUES 
('Cisco'),
('Juniper'),
('Trend Micro'),
('Citrix'),
('Check Point'),
('Raritan'),
('F5'),
('Gigamon'),
('Viavi'),
('NetOptics'),
('Bluecoat'),
('Safenet'),
('Plixer'),
('Infoblox')


GO
CREATE TABLE DeviceStatus (
	DeviceStatusId           int primary key identity(1,1),
	Status                 varchar(250) null
)
GO
INSERT INTO DeviceStatus(Status) VALUES
('In Production'),
('Decomissioned'),
('Pre-Production'),
('Lab'),
('RMA''d'),
('Storage')

GO
CREATE TABLE DeviceType (
	DeviceTypeId           int primary key identity(1,1),
	TypeDesc                 varchar(250) null
)
GO
INSERT INTO DeviceType(TypeDesc) VALUES
('Router'),
('Switch'),
('Firewall'),
('Packet Capture'),
('Packet Broker'),
('DNS'),
('Tools'),
('Tap'),
('HSM')

GO  
CREATE TABLE DeviceSite (
	DeviceSiteId           int primary key identity(1,1),
	SiteCode               varchar(100) null,
	SiteName               varchar(250) null,
	SiteAddress            varchar(250) null,
	City            varchar(250) null,
	State            varchar(50) null,
	Zip            varchar(50) null,
	SupportPhone            varchar(50) null,
	SupportEmail            varchar(500) null,
	SiteNote   varchar(max) null
)
GO
INSERT INTO DeviceSite(SiteCode,SiteName,SiteAddress,City,State,Zip,SupportPhone,SupportEmail) VALUES
('CDC','Cherokee Data Center','7400 N Lakewood Ave','Tulsa','OK','74117','1.918.600.8780','cdcsitesupport2@dxc.com'),
('CXC','Colorado Springs','311 S. Rockrimmon Blvd','Colorado Springs','CO','80919','1-719-323-3528','dc-facilities-colorado@dxc.com')
GO


CREATE TABLE PoInfo (
	PoInfoId           int primary key identity(1,1),
	PoNumber               varchar(250) null,
	Entitlement            varchar(250) null,
	PoNote   varchar(max) null
)
GO
INSERT INTO PoInfo(PoNumber,Entitlement) VALUES
('PoNumber One','Entitlement One'),
('PoNumber Two','Entitlement Two')
GO

CREATE TABLE DeviceGroup (
	DeviceGroupId       int primary key identity(1,1),
	GroupName               varchar(250) null,
	GroupType               varchar(250) null,
	GroupAttribute               varchar(500) null
)

GO

INSERT INTO DeviceGroup(GroupName,GroupType,GroupAttribute) VALUES
('GroupName One','GroupType One','GroupAttribute One'),
('GroupName Two','GroupType Two','GroupAttribute Two'),
('GroupName Three','GroupType Three','GroupAttribute Three')

GO

CREATE TABLE DeviceGroupAssign (
	DeviceGroupAssignId       int primary key identity(1,1),
	DeviceId          int not null default 0,
	DeviceGroupId         int not null default 0
)

GO

CREATE TABLE DeviceModel (
	DeviceModelId       int primary key identity(1,1),
	MfgName               varchar(250) null,
	Model               varchar(250) null,
	EolDate    datetime null
)

GO

INSERT INTO DeviceModel(MfgName,Model,EolDate) VALUES
('MfgName One','Model One','4/8/2020'),
('MfgName Two','Model Two','4/8/2020'),
('MfgName Three','Model Three','4/8/2020')

GO

CREATE TABLE Rack (
	RackId       int primary key identity(1,1),
	DeviceSiteId        int not null default 0,
	Tile               varchar(250) null,
	Ru               varchar(250) null,
	RackType               varchar(250) null
)

GO
INSERT INTO Rack(DeviceSiteId,Tile,Ru,RackType) VALUES
(1,'Tile One','Ru One','RackType One'),
(2,'Tile Two','Ru Two','RackType Two')
GO

--CREATE function dbo.GetDeviceSiteName(@Id int) returns varchar(250)
--AS
--BEGIN
--  DECLARE @ret varchar(250) = ISNULL((SELECT SiteName FROM DeviceSite  WHERE DeviceSiteId = @Id), '')

--  return @ret
--END

--GO
   
--ALTER TABLE Rack ADD SiteName AS dbo.GetDeviceSiteName(DeviceSiteId)
--GO










