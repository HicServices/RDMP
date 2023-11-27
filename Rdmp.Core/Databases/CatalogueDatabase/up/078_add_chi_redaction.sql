--Version:8.1.1
--Description:Adds support for redacting CHI values on data load

if not exists (select 1 from sys.tables where name = 'RedactedCHI')
begin

CREATE TABLE [dbo].RedactedCHI(
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PotentialCHI] [varchar](500) NOT NULL,
	[CHIContext][nvarchar](50) NOT NULL,
	CHILocation[nvarchar](500) NOT NULL,
 CONSTRAINT [PK_RedactedCHI] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
end
