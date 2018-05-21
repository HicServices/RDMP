using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace DataExportLibrary.CohortDatabaseWizard
{
    /// <summary>
    /// Creates an ExternalCohortTable database implementation.  The implementation will be based on your live IsExtractionIdentifier columns 
    /// (PrivateIdentifierPrototype) and a release identifier strategy (ReleaseIdentifierAssignmentStrategy) e.g. varchar(10) private patient identifier
    /// gets mapped to a new GUID.
    /// 
    /// <para>This implementation is intended to be a basic solution only and lacks advanced features such having the same release identifier for the same primary
    /// key in subsequent versions of the same cohort (generally you want 1 - m private identifiers because you don't want people to be able to link patients
    /// across project extracts they are working on).</para>
    /// 
    /// <para>See UserManual.docx for more information on how to tailor the resulting database to fit your needs.</para>
    /// </summary>
    public class CreateNewCohortDatabaseWizard
    {
        private readonly CatalogueRepository _catalogueRepository;
        private readonly IDataExportRepository _dataExportRepository;

        public CreateNewCohortDatabaseWizard(CatalogueRepository catalogueRepository, IDataExportRepository dataExportRepository)
        {
            _catalogueRepository = catalogueRepository;
            _dataExportRepository = dataExportRepository;
        }

        public PrivateIdentifierPrototype[] GetPrivateIdentifierCandidates()
        {
            //get the extraction identifiers
            var extractionInformations = _catalogueRepository.GetAllObjects<ExtractionInformation>().Where(ei => ei.IsExtractionIdentifier);

            //name + datatype, ideally we want to find 30 fields called 'PatientIndex' in 30 datasets all as char(10) fields but more likely we will get a slew of different spellings and dodgy datatypes (varchar(max) etc)
            var toReturn = new List<PrivateIdentifierPrototype>();

            //for each extraction identifier get the name of the column and give the associated data type
            foreach (var extractionInformation in extractionInformations)
            {
                //do not process ExtractionInformations when the ColumnInfo is COLUMNINFO_MISSING
                if(extractionInformation .ColumnInfo == null)
                    continue;

                var match = toReturn.SingleOrDefault(prototype => prototype.IsCompatible(extractionInformation));

                if(match != null)
                    match.MatchingExtractionInformations.Add(extractionInformation);
                else
                    toReturn.Add(new PrivateIdentifierPrototype(extractionInformation));
            }
                
            return toReturn.ToArray();
        }

        public string GetReleaseIdentifierNameAndTypeAsSqlString(ReleaseIdentifierAssignmentStrategy strategy)
        {
            switch (strategy)
            {
                case ReleaseIdentifierAssignmentStrategy.Unspecified:
                    return "";
                case ReleaseIdentifierAssignmentStrategy.Autonum:
                    return "ReleaseId INT IDENTITY(1,1) NOT NULL";
                case ReleaseIdentifierAssignmentStrategy.Guid:
                    return "ReleaseId varchar(255) NOT NULL default CONVERT(varchar(255),newid())";
                case ReleaseIdentifierAssignmentStrategy.LeaveBlank:
                    return "ReleaseId varchar(500) NOT NULL";
                default:
                    throw new ArgumentOutOfRangeException("strategy");
            }
        }

        public ExternalCohortTable CreateDatabase(PrivateIdentifierPrototype privateIdentifierPrototype, ReleaseIdentifierAssignmentStrategy strategy, SqlConnectionStringBuilder builder, string database, string nameForTheNewCohortSource, ICheckNotifier notifier)
        {
            if(strategy == ReleaseIdentifierAssignmentStrategy.Unspecified)
                throw new NotSupportedException("Cannot create unspecified strategy, use LeaveBlank if you want a placeholder release identifier that the user will ALTER/Create it himself");

            try
            {
            
                var server = new DiscoveredServer(builder);
                bool createDatabase = !server.ExpectDatabase(database).Exists();

                string username = builder.UserID;
                string password = builder.Password;

                using(var con = new SqlConnection(builder.ConnectionString))
                {

                    try
                    {
                        con.Open();
                        notifier.OnCheckPerformed(new CheckEventArgs("successfully to server " + builder.DataSource, CheckResult.Success));
                    }
                    catch (Exception e)
                    {
                        notifier.OnCheckPerformed(new CheckEventArgs("Could not connect to the server", CheckResult.Fail, e));
                        return null;
                    }

                    if(createDatabase)
                    {
                        notifier.OnCheckPerformed(new CheckEventArgs("Did not find database "+database +" on server so creating it",CheckResult.Success));
                        new SqlCommand("CREATE DATABASE " + database,con).ExecuteNonQuery();
                        notifier.OnCheckPerformed(new CheckEventArgs("successfully created empty database "+database +" on " + builder.DataSource,CheckResult.Success));
                    }


                    con.ChangeDatabase(database);
                    notifier.OnCheckPerformed(new CheckEventArgs("Switched connection from master to database " + database,CheckResult.Success));
                    
                    
                    string sql =
                        string.Format(
@"
CREATE TABLE [dbo].[Cohort](
       {0},
       {2},
       [cohortDefinition_id] [int] NOT NULL,
CONSTRAINT [PK_Cohort] PRIMARY KEY CLUSTERED 
(
       [cohortDefinition_id] ASC,
       {1} ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[CohortDefinition](
       [id] [int] IDENTITY(1,1) NOT NULL,
       [projectNumber] [int] NOT NULL,
       [version] [int] NOT NULL,
       [description] [varchar](4000) NOT NULL,
       [dtCreated] [date] NOT NULL,
CONSTRAINT [PK_CohortDefinition] PRIMARY KEY NONCLUSTERED 
(
       [id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[CohortDefinition] ADD  CONSTRAINT [DF_CohortDefinition_dtCreated]  DEFAULT (getdate()) FOR [dtCreated]
GO
ALTER TABLE [dbo].[Cohort]  WITH CHECK ADD  CONSTRAINT [FK_Cohort_CohortDefinition] FOREIGN KEY([cohortDefinition_id])
REFERENCES [dbo].[CohortDefinition] ([id])
GO
ALTER TABLE [dbo].[Cohort] CHECK CONSTRAINT [FK_Cohort_CohortDefinition]
GO

",
//{0}
privateIdentifierPrototype.GetDeclarationSql(),
//{1}
privateIdentifierPrototype.RuntimeName,
//{2}
GetReleaseIdentifierNameAndTypeAsSqlString(strategy),
//{3}
"ReleaseId"
);


                    notifier.OnCheckPerformed(new CheckEventArgs("Decided SQL was:" + sql, CheckResult.Success));
                    try
                    {
                        UsefulStuff.ExecuteBatchNonQuery(sql, con);
                        notifier.OnCheckPerformed(new CheckEventArgs("successfully created tables (your database should be useable now, all that remains is creating in a pointer to it in the RDMP database)", CheckResult.Success));
                    }
                    catch (Exception e)
                    {
                        notifier.OnCheckPerformed(new CheckEventArgs("Creation of tables SQL failed", CheckResult.Fail,e));
                        return null;
                    }

                    notifier.OnCheckPerformed(new CheckEventArgs("About to create pointer to the source" + sql, CheckResult.Success));
                    var pointer = new ExternalCohortTable(_dataExportRepository, "")
                    {
                        Server = builder.DataSource,
                        Database = database,
                        Username = username,
                        Password = password,
                        Name = nameForTheNewCohortSource,
                        TableName = "Cohort",
                        PrivateIdentifierField = privateIdentifierPrototype.RuntimeName,
                        ReleaseIdentifierField = "ReleaseId",
                        DefinitionTableForeignKeyField = "cohortDefinition_id",
                        DefinitionTableName = "CohortDefinition",
                    };

                    pointer.SaveToDatabase();

                    notifier.OnCheckPerformed(new CheckEventArgs("successfully created reference to cohort source in data export manager", CheckResult.Success));

                    notifier.OnCheckPerformed(new CheckEventArgs("About to run post creation checks", CheckResult.Success));
                    pointer.Check(notifier);

                    notifier.OnCheckPerformed(new CheckEventArgs("Finished", CheckResult.Success));

                    return pointer;
                }
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(
                    new CheckEventArgs("Entire setup failed with exception (double click to find out why)",
                        CheckResult.Fail, e));
                return null;
            }
        }
    }

    public enum ReleaseIdentifierAssignmentStrategy
    {
        Unspecified,

        Autonum,
        Guid,
        LeaveBlank
    }
}
