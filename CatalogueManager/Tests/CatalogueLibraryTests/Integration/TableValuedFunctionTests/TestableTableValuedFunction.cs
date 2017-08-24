using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Tests.Common;

namespace CatalogueLibraryTests.Integration.TableValuedFunctionTests
{
    public class TestableTableValuedFunction
    {
        public TableInfo TableInfoCreated;
        public ColumnInfo[] ColumnInfosCreated;
        public string CreateFunctionSQL;
        public Catalogue Cata;
        public CatalogueItem[] CataItems;
        public ExtractionInformation[] ExtractionInformations;


        public void Create(DiscoveredDatabase databaseICanCreateRandomTablesIn, CatalogueRepository catalogueRepository)
        {

            CreateFunctionSQL = @"
if exists (select 1 from sys.objects where name = 'MyAwesomeFunction')
    drop function MyAwesomeFunction
GO

CREATE FUNCTION MyAwesomeFunction
(	
	-- Add the parameters for the function here
	@startNumber int ,
	@stopNumber int,
	@name varchar(50)
)
RETURNS
@ReturnTable TABLE 
(
	-- Add the column definitions for the TABLE variable here
	Number int, 
	Name varchar(50)
)
AS
BEGIN
	-- Fill the table variable with the rows for your result set
	DECLARE @i int;
	set @i = @startNumber

	while(@i < @stopNumber)
		begin
		INSERT INTO @ReturnTable(Name,Number) VALUES (@name,@i);
		set @i = @i + 1;
		end

	RETURN 
END
";
            using (var con = databaseICanCreateRandomTablesIn.Server.GetConnection())
            {
                con.Open();
                UsefulStuff.ExecuteBatchNonQuery(CreateFunctionSQL, con);
            }
            var tbl = databaseICanCreateRandomTablesIn.ExpectTableValuedFunction("MyAwesomeFunction");
            TableValuedFunctionImporter importer = new TableValuedFunctionImporter(catalogueRepository, tbl);
            importer.DoImport(out TableInfoCreated, out ColumnInfosCreated);

            importer.ParametersCreated[0].Value = "5";
            importer.ParametersCreated[0].SaveToDatabase();

            importer.ParametersCreated[1].Value = "10";
            importer.ParametersCreated[1].SaveToDatabase();

            importer.ParametersCreated[2].Value = "'fish'";
            importer.ParametersCreated[2].SaveToDatabase();


            ForwardEngineerCatalogue forwardEngineerCatalogue = new ForwardEngineerCatalogue(TableInfoCreated, ColumnInfosCreated, true);
            forwardEngineerCatalogue.ExecuteForwardEngineering(out Cata, out CataItems, out ExtractionInformations);
        }

        public void Destroy()
        {
            var credentials = TableInfoCreated.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing);

            TableInfoCreated.DeleteInDatabase();

            if(credentials != null)
                credentials.DeleteInDatabase();

            Cata.DeleteInDatabase();
        }
    }
}
