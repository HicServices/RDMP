--Version:1.35.0.0
--Description: Allows the agency to create a unique certificate for decrypting passwords with SimpleStringValueEncryption instead of using a hard coded one.
if not exists (select 1 from sys.tables where name = 'PasswordEncryptionKeyLocation')
begin
	
	CREATE TABLE [dbo].PasswordEncryptionKeyLocation(
	[Path] [varchar](max) NULL,

	--Ensure table can never have more than 1 record in it
	Lock char(1) not null,
    constraint PK_T1 PRIMARY KEY (Lock),
    constraint CK_T1_Locked CHECK (Lock='X')
	)

	ALTER TABLE ExternalDatabaseServer ALTER COLUMN [Password] varchar(max)

end
