// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Tests.Common;

public class TestableTableValuedFunction
{
    public ITableInfo TableInfoCreated;
    public ColumnInfo[] ColumnInfosCreated;
    public string CreateFunctionSQL;
    public ICatalogue Cata;
    public CatalogueItem[] CataItems;
    public ExtractionInformation[] ExtractionInformations;


    public void Create(DiscoveredDatabase databaseICanCreateRandomTablesIn, ICatalogueRepository catalogueRepository)
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
        var importer = new TableValuedFunctionImporter(catalogueRepository, tbl);
        importer.DoImport(out TableInfoCreated, out ColumnInfosCreated);

        importer.ParametersCreated[0].Value = "5";
        importer.ParametersCreated[0].SaveToDatabase();

        importer.ParametersCreated[1].Value = "10";
        importer.ParametersCreated[1].SaveToDatabase();

        importer.ParametersCreated[2].Value = "'fish'";
        importer.ParametersCreated[2].SaveToDatabase();


        var forwardEngineerCatalogue = new ForwardEngineerCatalogue(TableInfoCreated, ColumnInfosCreated);
        forwardEngineerCatalogue.ExecuteForwardEngineering(out Cata, out CataItems, out ExtractionInformations);
    }

    public void Destroy()
    {
        var credentials = (DataAccessCredentials)TableInfoCreated.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing);

        TableInfoCreated.DeleteInDatabase();

        credentials?.DeleteInDatabase();

        Cata.DeleteInDatabase();
    }
}