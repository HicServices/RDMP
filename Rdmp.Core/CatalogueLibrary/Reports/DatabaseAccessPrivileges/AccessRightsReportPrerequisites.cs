// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace CatalogueLibrary.Reports.DatabaseAccessPrivileges
{
    /// <summary>
    /// Hacky string container class that holds a single variable: the script needed to run on a Microsoft Sql Server to support WordAccessRightsByUser and 
    /// WordAccessRightsByDatabase classes.  This SQL creates a database called Audit and a stored proceedure that longitudinally snapshots database access
    /// permissions (not something Sql Server handles normally) so that you have a record of who had access to what and when. 
    /// </summary>
    public class AccessRightsReportPrerequisites
    {
        /// <summary>
        /// Sql to create the Audit database in an Sql Server (allows longitudinal tracking of what users have access to what tables/databases).
        /// </summary>
        public const string SQL = @"
CREATE DATABASE [Audit]
GO

USE [Audit]
GO
/****** Object:  StoredProcedure [dbo].[UpdatePrivilegesAudit]    Script Date: 02/11/2015 12:39:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create procedure [dbo].[UpdatePrivilegesAudit] as
BEGIN
	--this is where we will record the live state, we will then update the real table and move old records into the _Archive
	truncate table [Audit].dbo.tt_LoginsLive
	truncate table [Audit].dbo.tt_DatabasePermissionsLive

	set nocount on
	--Do logins
	INSERT INTO [Audit].dbo.tt_LoginsLive
	select createdate,name,dbname,denylogin,hasaccess,isntname,isntgroup,isntuser,sysadmin,securityadmin,serveradmin,setupadmin,processadmin,diskadmin,dbcreator,bulkadmin,loginname from master..syslogins

	--Cursor for all the databases in the Server
	DECLARE dbCursor cursor FOR select name from master.sys.databases where state = 0
	DECLARE @dbName as varchar(5000)
	DECLARE @sql as nvarchar(4000) --maximum size

	OPEN dbCursor

	FETCH NEXT FROM dbCursor INTO @dbName

	WHILE @@FETCH_STATUS = 0
	BEGIN
	
		SET @sql = N'use [' + @dbName + N'];' + N' INSERT INTO [Audit].dbo.tt_DatabasePermissionsLive  ([DatabaseName],[UserName],[UserType],[DatabaseUserName],[Role],[PermissionType],[PermissionState],[ObjectType],[ObjectName],[ColumnName])  	  	SELECT  '''+@dbName+N''', 		[UserName] = CASE princ.[type]  						WHEN ''S'' THEN princ.[name] 						WHEN ''U'' THEN ulogin.[name] COLLATE Latin1_General_CI_AI 					 END, 		[UserType] = CASE princ.[type] 						WHEN ''S'' THEN ''SQL User'' 						WHEN ''U'' THEN ''Windows User'' 					 END,   		[DatabaseUserName] = princ.[name],        		[Role] = null,       		[PermissionType] = perm.[permission_name],        		[PermissionState] = perm.[state_desc],        		[ObjectType] = obj.type_desc,  		[ObjectName] = OBJECT_NAME(perm.major_id), 		[ColumnName] = col.[name] 	FROM     		  		sys.database_principals princ   	LEFT JOIN 		  		sys.login_token ulogin on princ.[sid] = ulogin.[sid] 	LEFT JOIN         		  		sys.database_permissions perm ON perm.[grantee_principal_id] = princ.[principal_id] 	LEFT JOIN 		  		sys.columns col ON col.[object_id] = perm.major_id  						AND col.[column_id] = perm.[minor_id] 	LEFT JOIN 		sys.objects obj ON perm.[major_id] = obj.[object_id] 	WHERE  		princ.[type] in (''S'',''U'') 	UNION 	  	SELECT   '''+@dbName+N''',		[UserName] = CASE memberprinc.[type]  						WHEN ''S'' THEN memberprinc.[name] 						WHEN ''U'' THEN ulogin.[name] COLLATE Latin1_General_CI_AI 					 END, 		[UserType] = CASE memberprinc.[type] 						WHEN ''S'' THEN ''SQL User'' 						WHEN ''U'' THEN ''Windows User'' 					 END,  		[DatabaseUserName] = memberprinc.[name],    		[Role] = roleprinc.[name],       		[PermissionType] = perm.[permission_name],        		[PermissionState] = perm.[state_desc],        		[ObjectType] = obj.type_desc,  		[ObjectName] = OBJECT_NAME(perm.major_id), 		[ColumnName] = col.[name] 	FROM     		  		sys.database_role_members members 	JOIN 		  		sys.database_principals roleprinc ON roleprinc.[principal_id] = members.[role_principal_id] 	JOIN 		  		sys.database_principals memberprinc ON memberprinc.[principal_id] = members.[member_principal_id] 	LEFT JOIN 		  		sys.login_token ulogin on memberprinc.[sid] = ulogin.[sid] 	LEFT JOIN         		  		sys.database_permissions perm ON perm.[grantee_principal_id] = roleprinc.[principal_id] 	LEFT JOIN 		  		sys.columns col on col.[object_id] = perm.major_id  						AND col.[column_id] = perm.[minor_id] 	LEFT JOIN 		sys.objects obj ON perm.[major_id] = obj.[object_id] 	UNION 	  	SELECT   '''+@dbName+N''',		[UserName] = ''{All Users}'', 		[UserType] = ''{All Users}'',  		[DatabaseUserName] = ''{All Users}'',        		[Role] = roleprinc.[name],       		[PermissionType] = perm.[permission_name],        		[PermissionState] = perm.[state_desc],        		[ObjectType] = obj.type_desc,  		[ObjectName] = OBJECT_NAME(perm.major_id), 		[ColumnName] = col.[name] 	FROM     		  		sys.database_principals roleprinc 	LEFT JOIN         		  		sys.database_permissions perm ON perm.[grantee_principal_id] = roleprinc.[principal_id] 	LEFT JOIN 		  		sys.columns col on col.[object_id] = perm.major_id  						AND col.[column_id] = perm.[minor_id]                    	JOIN  		  		sys.objects obj ON obj.[object_id] = perm.[major_id] 	WHERE 		  		roleprinc.[type] = ''R'' AND 		  		roleprinc.[name] = ''public'' AND 		  		obj.is_ms_shipped = 0 	ORDER BY 		princ.[Name], 		OBJECT_NAME(perm.major_id), 		col.[name], 		perm.[permission_name], 		perm.[state_desc], 		obj.type_desc'
	
	EXEC sp_executesql @sql
		
	FETCH NEXT FROM dbCursor INTO @dbName

	END

	CLOSE dbCursor;
	DEALLOCATE dbCursor;

    --Also add the owners of the databases
    INSERT INTO [Audit].dbo.tt_DatabasePermissionsLive
  (DatabaseName,UserName,UserType,DatabaseUserName,Role,PermissionType)
      select name,suser_sname(owner_sid),null,suser_sname(owner_sid),'Owner','Owner'  from sys.databases

	set nocount off

	--///////////////////////////////////////////////////////////////////////////////////////// UPDATE LOGINS TABLE /////////////////////////////////////////////////////////////////////////////////

	  --/////////////////////////////////////////////////////Things in the OLD state that are not in the NEW state////////////////////////////////////////
  
	  --/////////////////////////////////////////////////////Move them into the Archive////////////////////////////////////////
	  INSERT INTO [Audit].dbo.Logins_Archive 
	  select *,GETDATE()
	   FROM [Audit].dbo.Logins oldState
	where 
	not exists (select 1 from [Audit].dbo.tt_LoginsLive currentState
	 where
	  currentState.bulkadmin = oldState.bulkadmin AND
	  currentState.createdate = oldState.createdate AND
	  currentState.dbcreator = oldState.dbcreator AND
	currentState.dbname = oldState.dbname AND
	currentState.denylogin = oldState.denylogin AND
	currentState.diskadmin = oldState.diskadmin AND
	currentState.hasaccess = oldState.hasaccess AND
	  currentState.isntgroup = oldState.isntgroup AND
		currentState.isntname = oldState.isntname AND
		currentState.isntuser = oldState.isntuser AND
		currentState.loginname = oldState.loginname AND
		currentState.name = oldState.name AND
		currentState.processadmin = oldState.processadmin AND
		currentState.serveradmin = oldState.serveradmin AND
		currentState.setupadmin = oldState.setupadmin AND
		currentState.sysadmin = oldState.sysadmin
	  )
		--/////////////////////////////////////////////////////Delete them from the Logins table////////////////////////////////////////
	  DELETE FROM  [Audit].dbo.Logins 
	where 
	not exists (select 1 from [Audit].dbo.tt_LoginsLive currentState
	 where
	  currentState.bulkadmin = [Audit].dbo.Logins.bulkadmin AND
	  currentState.createdate = [Audit].dbo.Logins.createdate AND
	  currentState.dbcreator = [Audit].dbo.Logins.dbcreator AND
	currentState.dbname = [Audit].dbo.Logins.dbname AND
	currentState.denylogin = [Audit].dbo.Logins.denylogin AND
	currentState.diskadmin = [Audit].dbo.Logins.diskadmin AND
	currentState.hasaccess = [Audit].dbo.Logins.hasaccess AND
	  currentState.isntgroup = [Audit].dbo.Logins.isntgroup AND
		currentState.isntname = [Audit].dbo.Logins.isntname AND
		currentState.isntuser = [Audit].dbo.Logins.isntuser AND
		currentState.loginname = [Audit].dbo.Logins.loginname AND
		currentState.name = [Audit].dbo.Logins.name AND
		currentState.processadmin = [Audit].dbo.Logins.processadmin AND
		currentState.serveradmin = [Audit].dbo.Logins.serveradmin AND
		currentState.setupadmin = [Audit].dbo.Logins.setupadmin AND
		currentState.sysadmin = [Audit].dbo.Logins.sysadmin
	  )
  
	  --/////////////////////////////////////////////////////Things that are in the NEW state that are not in the OLD state////////////////////////////////////////

	  --/////////////////////////////////////////////////////Insert them into the Logins table////////////////////////////////////////
	INSERT INTO [Audit].dbo.Logins
	select *,GetDate() from [Audit].dbo.tt_LoginsLive currentState
	where 
	not exists (select 1 from [Audit].dbo.Logins oldState
	 where
	  currentState.bulkadmin = oldState.bulkadmin AND
	  currentState.createdate = oldState.createdate AND
	  currentState.dbcreator = oldState.dbcreator AND
	currentState.dbname = oldState.dbname AND
	currentState.denylogin = oldState.denylogin AND
	currentState.diskadmin = oldState.diskadmin AND
	currentState.hasaccess = oldState.hasaccess AND
	  currentState.isntgroup = oldState.isntgroup AND
		currentState.isntname = oldState.isntname AND
		currentState.isntuser = oldState.isntuser AND
		currentState.loginname = oldState.loginname AND
		currentState.name = oldState.name AND
		currentState.processadmin = oldState.processadmin AND
		currentState.serveradmin = oldState.serveradmin AND
		currentState.setupadmin = oldState.setupadmin AND
		currentState.sysadmin = oldState.sysadmin
	  )
  


	  --///////////////////////////////////////////////////////////////////////////////////////// UPDATE DATABASE PERMISSIONS TABLE /////////////////////////////////////////////////////////////////////////////////
  
		--/////////////////////////////////////////////////////Things in the OLD state that are not in the NEW state////////////////////////////////////////
  
	  --/////////////////////////////////////////////////////Move them into the Archive////////////////////////////////////////
	  INSERT INTO [Audit].dbo.DatabasePermissions_Archive 
	  select *,GETDATE()
	   FROM [Audit].dbo.DatabasePermissions oldState
	where 
	not exists (select 1 from [Audit].dbo.tt_DatabasePermissionsLive currentState
	 where
	ISNULL(currentState.ColumnName,'SQLIsStupid') =  ISNULL(oldState.ColumnName ,'SQLIsStupid') AND                    --SQL throws a wobbly if you try to ask if NULL = NULL and returns false for some stupid reason try running SELECT NULL=NULL
	  ISNULL(currentState.DatabaseName ,'SQLIsStupid') =  ISNULL(oldState.DatabaseName ,'SQLIsStupid') AND
	  ISNULL(currentState.DatabaseUserName ,'SQLIsStupid') =  ISNULL(oldState.DatabaseUserName ,'SQLIsStupid') AND
	ISNULL(currentState.ObjectName ,'SQLIsStupid') =  ISNULL(oldState.ObjectName ,'SQLIsStupid') AND
	ISNULL(currentState.ObjectType ,'SQLIsStupid') =  ISNULL(oldState.ObjectType ,'SQLIsStupid') AND
	ISNULL(currentState.PermissionState ,'SQLIsStupid') =  ISNULL(oldState.PermissionState ,'SQLIsStupid') AND
	  ISNULL(currentState.PermissionType ,'SQLIsStupid') =  ISNULL(oldState.PermissionType ,'SQLIsStupid') AND
		ISNULL(currentState.[Role] ,'SQLIsStupid') =   ISNULL(oldState.[Role] ,'SQLIsStupid') AND
		ISNULL(currentState.UserName ,'SQLIsStupid') =  ISNULL(oldState.UserName ,'SQLIsStupid') AND
		ISNULL(currentState.UserType ,'SQLIsStupid') =  ISNULL(oldState.UserType,'SQLIsStupid')
	  )
  
		--/////////////////////////////////////////////////////Delete them from the DatabasePermissions table////////////////////////////////////////
	DELETE FROM [Audit].dbo.DatabasePermissions
	where 
	not exists (select 1 from [Audit].dbo.tt_DatabasePermissionsLive currentState
	 where
	ISNULL(currentState.ColumnName,'SQLIsStupid') =  ISNULL([Audit].dbo.DatabasePermissions.ColumnName ,'SQLIsStupid') AND
	  ISNULL(currentState.DatabaseName ,'SQLIsStupid') =  ISNULL([Audit].dbo.DatabasePermissions.DatabaseName ,'SQLIsStupid') AND
	  ISNULL(currentState.DatabaseUserName ,'SQLIsStupid') =  ISNULL([Audit].dbo.DatabasePermissions.DatabaseUserName ,'SQLIsStupid') AND
	ISNULL(currentState.ObjectName ,'SQLIsStupid') =  ISNULL([Audit].dbo.DatabasePermissions.ObjectName ,'SQLIsStupid') AND
	ISNULL(currentState.ObjectType ,'SQLIsStupid') =  ISNULL([Audit].dbo.DatabasePermissions.ObjectType ,'SQLIsStupid') AND
	ISNULL(currentState.PermissionState ,'SQLIsStupid') =  ISNULL([Audit].dbo.DatabasePermissions.PermissionState ,'SQLIsStupid') AND
	  ISNULL(currentState.PermissionType ,'SQLIsStupid') =  ISNULL([Audit].dbo.DatabasePermissions.PermissionType ,'SQLIsStupid') AND
		ISNULL(currentState.[Role] ,'SQLIsStupid') =   ISNULL([Audit].dbo.DatabasePermissions.[Role] ,'SQLIsStupid') AND
		ISNULL(currentState.UserName ,'SQLIsStupid') =  ISNULL([Audit].dbo.DatabasePermissions.UserName ,'SQLIsStupid') AND
		ISNULL(currentState.UserType ,'SQLIsStupid') =  ISNULL([Audit].dbo.DatabasePermissions.UserType,'SQLIsStupid')
	  )

		--/////////////////////////////////////////////////////Things that are in the NEW state that are not in the OLD state////////////////////////////////////////

	  --/////////////////////////////////////////////////////Insert them into the DatabasePermissions table////////////////////////////////////////
	  INSERT INTO [Audit].dbo.DatabasePermissions
	select *,GetDate() from [Audit].dbo.tt_DatabasePermissionsLive currentState
	where 
	not exists (select 1 from [Audit].dbo.DatabasePermissions oldState
	 where
	ISNULL(currentState.ColumnName,'SQLIsStupid') =  ISNULL(oldState.ColumnName ,'SQLIsStupid') AND
	  ISNULL(currentState.DatabaseName ,'SQLIsStupid') =  ISNULL(oldState.DatabaseName ,'SQLIsStupid') AND
	  ISNULL(currentState.DatabaseUserName ,'SQLIsStupid') =  ISNULL(oldState.DatabaseUserName ,'SQLIsStupid') AND
	ISNULL(currentState.ObjectName ,'SQLIsStupid') =  ISNULL(oldState.ObjectName ,'SQLIsStupid') AND
	ISNULL(currentState.ObjectType ,'SQLIsStupid') =  ISNULL(oldState.ObjectType ,'SQLIsStupid') AND
	ISNULL(currentState.PermissionState ,'SQLIsStupid') =  ISNULL(oldState.PermissionState ,'SQLIsStupid') AND
	  ISNULL(currentState.PermissionType ,'SQLIsStupid') =  ISNULL(oldState.PermissionType ,'SQLIsStupid') AND
		ISNULL(currentState.[Role] ,'SQLIsStupid') =   ISNULL(oldState.[Role] ,'SQLIsStupid') AND
		ISNULL(currentState.UserName ,'SQLIsStupid') =  ISNULL(oldState.UserName ,'SQLIsStupid') AND
		ISNULL(currentState.UserType ,'SQLIsStupid') =  ISNULL(oldState.UserType,'SQLIsStupid')
	  )

	
	truncate table [Audit].dbo.tt_LoginsLive
	truncate table [Audit].dbo.tt_DatabasePermissionsLive
end
GO
/****** Object:  Table [dbo].[DatabasePermissions]    Script Date: 02/11/2015 12:39:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DatabasePermissions](
	[DatabaseName] [nvarchar](128) NULL,
	[UserName] [nvarchar](128) NULL,
	[UserType] [varchar](12) NULL,
	[DatabaseUserName] [sysname] NOT NULL,
	[Role] [nvarchar](60) NULL,
	[PermissionType] [nvarchar](128) NULL,
	[PermissionState] [nvarchar](60) NULL,
	[ObjectType] [nvarchar](60) NULL,
	[ObjectName] [nvarchar](128) NULL,
	[ColumnName] [sysname] NULL,
	[validFrom] [date] NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[DatabasePermissions_Archive]    Script Date: 02/11/2015 12:39:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DatabasePermissions_Archive](
	[DatabaseName] [nvarchar](128) NULL,
	[UserName] [nvarchar](128) NULL,
	[UserType] [varchar](12) NULL,
	[DatabaseUserName] [sysname] NOT NULL,
	[Role] [nvarchar](60) NULL,
	[PermissionType] [nvarchar](128) NULL,
	[PermissionState] [nvarchar](60) NULL,
	[ObjectType] [nvarchar](60) NULL,
	[ObjectName] [nvarchar](128) NULL,
	[ColumnName] [sysname] NULL,
	[validFrom] [date] NULL,
	[validTo] [date] NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Logins]    Script Date: 02/11/2015 12:39:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Logins](
	[createdate] [datetime] NOT NULL,
	[name] [sysname] NOT NULL,
	[dbname] [sysname] NULL,
	[denylogin] [int] NULL,
	[hasaccess] [int] NULL,
	[isntname] [int] NULL,
	[isntgroup] [int] NULL,
	[isntuser] [int] NULL,
	[sysadmin] [int] NULL,
	[securityadmin] [int] NULL,
	[serveradmin] [int] NULL,
	[setupadmin] [int] NULL,
	[processadmin] [int] NULL,
	[diskadmin] [int] NULL,
	[dbcreator] [int] NULL,
	[bulkadmin] [int] NULL,
	[loginname] [sysname] NOT NULL,
	[validFrom] [date] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Logins_Archive]    Script Date: 02/11/2015 12:39:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Logins_Archive](
	[createdate] [datetime] NOT NULL,
	[name] [sysname] NOT NULL,
	[dbname] [sysname] NULL,
	[denylogin] [int] NULL,
	[hasaccess] [int] NULL,
	[isntname] [int] NULL,
	[isntgroup] [int] NULL,
	[isntuser] [int] NULL,
	[sysadmin] [int] NULL,
	[securityadmin] [int] NULL,
	[serveradmin] [int] NULL,
	[setupadmin] [int] NULL,
	[processadmin] [int] NULL,
	[diskadmin] [int] NULL,
	[dbcreator] [int] NULL,
	[bulkadmin] [int] NULL,
	[loginname] [sysname] NOT NULL,
	[validFrom] [date] NULL,
	[validTo] [date] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ProjectArchiverAudit]    Script Date: 02/11/2015 12:39:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ProjectArchiverAudit](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[username] [varchar](100) NOT NULL,
	[dtCreated] [datetime] NOT NULL,
	[project] [varchar](5000) NOT NULL,
	[projectNumber] [int] NOT NULL,
	[action] [varchar](100) NOT NULL,
	[parameter1] [varchar](1000) NOT NULL,
	[parameter2] [varchar](1000) NULL,
	[parameter3] [varchar](1000) NULL,
	[md5] [varchar](64) NULL,
	[exception] [varchar](5000) NULL,
	[comment] [varchar](1000) NULL,
	[rows] [int] NULL,
 CONSTRAINT [PK_ProjectArchiverAudit] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tt_DatabasePermissionsLive]    Script Date: 02/11/2015 12:39:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tt_DatabasePermissionsLive](
	[DatabaseName] [nvarchar](128) NULL,
	[UserName] [nvarchar](128) NULL,
	[UserType] [varchar](12) NULL,
	[DatabaseUserName] [sysname] NOT NULL,
	[Role] [nvarchar](60) NULL,
	[PermissionType] [nvarchar](128) NULL,
	[PermissionState] [nvarchar](60) NULL,
	[ObjectType] [nvarchar](60) NULL,
	[ObjectName] [nvarchar](128) NULL,
	[ColumnName] [sysname] NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tt_LoginsLive]    Script Date: 02/11/2015 12:39:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tt_LoginsLive](
	[createdate] [datetime] NOT NULL,
	[name] [sysname] NOT NULL,
	[dbname] [sysname] NULL,
	[denylogin] [int] NULL,
	[hasaccess] [int] NULL,
	[isntname] [int] NULL,
	[isntgroup] [int] NULL,
	[isntuser] [int] NULL,
	[sysadmin] [int] NULL,
	[securityadmin] [int] NULL,
	[serveradmin] [int] NULL,
	[setupadmin] [int] NULL,
	[processadmin] [int] NULL,
	[diskadmin] [int] NULL,
	[dbcreator] [int] NULL,
	[bulkadmin] [int] NULL,
	[loginname] [sysname] NOT NULL
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[DatabasePermissions] ADD  CONSTRAINT [DF_DatabasePermissions_dtCreated]  DEFAULT (getdate()) FOR [validFrom]
GO
ALTER TABLE [dbo].[ProjectArchiverAudit] ADD  CONSTRAINT [DF_ProjectArchiverAudit_dtCreated]  DEFAULT (getdate()) FOR [dtCreated]
GO

";
    }
}
