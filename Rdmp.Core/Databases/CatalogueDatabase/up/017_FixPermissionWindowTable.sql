--Version:1.13.0.0
--Description: Fix PermissionWindow table

-- If ID is an identity column then this script has already run (providing someone hasn't been manually interfering with the database) 
IF (SELECT columnproperty(object_id('PermissionWindow'), 'ID', 'IsIdentity')) = 0
BEGIN
	--only drop the PK if it exists
	IF EXISTS (SELECT 1 FROM sys.objects WHERE is_ms_shipped = 0 AND type_desc LIKE '%_CONSTRAINT' AND name = 'PK_PermissionWindow')
	begin
		-- Drop all constraints on the existing PermissionWindow table
		ALTER TABLE PermissionWindow DROP CONSTRAINT [PK_PermissionWindow]
	end

	IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_PermissionWindow_RequiresSynchronousAccess')
	BEGIN 
		ALTER TABLE PermissionWindow DROP CONSTRAINT [DF_PermissionWindow_RequiresSynchronousAccess]
	END

	IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_PermissionWindow_SoftwareVersion')
	BEGIN 
		ALTER TABLE PermissionWindow DROP CONSTRAINT [DF_PermissionWindow_SoftwareVersion]
	END

	-- Create new PermissionWindow table
	CREATE TABLE [dbo].[TempPermissionWindow](
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[PermissionPeriodConfig] [varchar](max) NULL,
		[RequiresSynchronousAccess] [bit] NOT NULL CONSTRAINT [DF_PermissionWindow_RequiresSynchronousAccess] DEFAULT ((1)),
		[SoftwareVersion] [nvarchar](50) NOT NULL CONSTRAINT [DF_PermissionWindow_SoftwareVersion] DEFAULT ([dbo].[GetSoftwareVersion]()),
	 CONSTRAINT [PK_PermissionWindow] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
	)

	if ((select count(*) from PermissionWindow) > 0)
		begin
			RaisError(N'Expected Permission Window to be empty',16,1)
		end
	else
		begin
			-- Remove old table and rename new
			DROP TABLE PermissionWindow
			EXEC sp_rename 'TempPermissionWindow', 'PermissionWindow'
		end
	
END
