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
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

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
        private readonly DiscoveredDatabase _targetDatabase;

        private string _releaseIdentifierFieldName = "ReleaseId";
        private string _definitionTableForeignKeyField = "cohortDefinition_id";


        public CreateNewCohortDatabaseWizard(DiscoveredDatabase targetDatabase,CatalogueRepository catalogueRepository, IDataExportRepository dataExportRepository)
        {
            _catalogueRepository = catalogueRepository;
            _dataExportRepository = dataExportRepository;
            _targetDatabase = targetDatabase;
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

        public ExternalCohortTable CreateDatabase(PrivateIdentifierPrototype privateIdentifierPrototype, ICheckNotifier notifier)
        {
            if (!_targetDatabase.Exists())
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Did not find database "+_targetDatabase +" on server so creating it",CheckResult.Success));
                _targetDatabase.Create();
            }
            
            try
            {
                var definitionTable = _targetDatabase.CreateTable("CohortDefinition", new[]
                    {
                         new DatabaseColumnRequest("id",new DatabaseTypeRequest(typeof(int))){AllowNulls = false,AutoIncrement = true,IsPrimaryKey = true}, 
                         new DatabaseColumnRequest("projectNumber",new DatabaseTypeRequest(typeof(int))){AllowNulls =  false}, 
                         new DatabaseColumnRequest("version",new DatabaseTypeRequest(typeof(int))){AllowNulls =  false}, 
                         new DatabaseColumnRequest("description",new DatabaseTypeRequest(typeof(string),3000)){AllowNulls =  false},
                         new DatabaseColumnRequest("dtCreated",new DatabaseTypeRequest(typeof(DateTime))){AllowNulls =  false}
                    });

                
                var idColumn = definitionTable.DiscoverColumn("id");
                var foreignKey = new DatabaseColumnRequest(_definitionTableForeignKeyField,new DatabaseTypeRequest(typeof (int)), false) {IsPrimaryKey = true};
                

                var cohortTable = _targetDatabase.CreateTable("Cohort",new []
                {
                 new DatabaseColumnRequest(privateIdentifierPrototype.RuntimeName,privateIdentifierPrototype.DataType,false){IsPrimaryKey = true},
                 new DatabaseColumnRequest(_releaseIdentifierFieldName,new DatabaseTypeRequest(typeof(string),300)), 
                 foreignKey
                }
                ,
                //foreign key between id and cohortDefinition_id
                new Dictionary<DatabaseColumnRequest, DiscoveredColumn>() { { foreignKey,idColumn } },true);

                
                notifier.OnCheckPerformed(new CheckEventArgs("About to create pointer to the source", CheckResult.Success));
                var pointer = new ExternalCohortTable(_dataExportRepository, "")
                {
                    Server = _targetDatabase.Server.Name,
                    Database = _targetDatabase.GetRuntimeName(),
                    Username = _targetDatabase.Server.ExplicitUsernameIfAny,
                    Password = _targetDatabase.Server.ExplicitPasswordIfAny,
                    Name = _targetDatabase.GetRuntimeName(),
                    TableName = cohortTable.GetRuntimeName(),
                    PrivateIdentifierField = privateIdentifierPrototype.RuntimeName,
                    ReleaseIdentifierField = _releaseIdentifierFieldName,
                    DefinitionTableForeignKeyField = _definitionTableForeignKeyField,
                    DefinitionTableName = definitionTable.GetRuntimeName(),
                    DatabaseType = _targetDatabase.Server.DatabaseType
                };

                pointer.SaveToDatabase();

                notifier.OnCheckPerformed(new CheckEventArgs("successfully created reference to cohort source in data export manager", CheckResult.Success));

                notifier.OnCheckPerformed(new CheckEventArgs("About to run post creation checks", CheckResult.Success));
                pointer.Check(notifier);

                notifier.OnCheckPerformed(new CheckEventArgs("Finished", CheckResult.Success));

                return pointer;
                
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
}
