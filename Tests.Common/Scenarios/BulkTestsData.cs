// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Linq;
using SynthEHR;
using SynthEHR.Datasets;
using FAnsi.Discovery;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.Validation;
using Rdmp.Core.Validation.Constraints;
using Rdmp.Core.Validation.Constraints.Primary;
using TypeGuesser;

namespace Tests.Common.Scenarios;

/// <summary>
/// Class for creating a large table of data in a test Microsoft Sql Server database and importing a reference to it as a <see cref="Catalogue"/>
/// </summary>
public class BulkTestsData
{
    private readonly ICatalogueRepository _repository;

    /// <summary>
    /// The database in which to create the test data table
    /// </summary>
    public readonly DiscoveredDatabase BulkDataDatabase;

    /// <summary>
    /// The name of the test table that will be created
    /// </summary>
    public const string BulkDataTable = "BulkData";

    /// <summary>
    /// The number of rows that are to be created
    /// </summary>
    public readonly int ExpectedNumberOfRowsInTestData;

    /// <summary>
    /// Rdmp reference to the test table (<see cref="ImportAsCatalogue"/>)
    /// </summary>
    public ITableInfo tableInfo;

    /// <summary>
    /// Rdmp reference to the test table columns (<see cref="ImportAsCatalogue"/>)
    /// </summary>
    public ColumnInfo[] columnInfos;

    /// <summary>
    /// Rdmp reference to the test table (<see cref="ImportAsCatalogue"/>).  <see cref="Catalogue"/> is the descriptive element while <see cref="tableInfo"/> is the
    /// pointer to the underlying table.
    /// </summary>
    public ICatalogue catalogue;

    /// <summary>
    /// Rdmp reference to the test table columns (<see cref="ImportAsCatalogue"/>).  <see cref="CatalogueItem"/> is the descriptive element while <see cref="columnInfos"/> is the
    /// pointer to the underlying table columns.
    /// </summary>
    public CatalogueItem[] catalogueItems;

    /// <summary>
    /// Rdmp reference to which columns of the test table are considered extractable (<see cref="ImportAsCatalogue"/>)
    /// </summary>
    public ExtractionInformation[] extractionInformations;

    private Demography _dataGenerator;

    private Random r = new();

    /// <summary>
    /// the bulk test data created during <see cref="SetupTestData"/>
    /// </summary>
    public DiscoveredTable Table { get; private set; }

    /// <summary>
    /// Prepares to create a new table in the <paramref name="targetDatabase"/> of test data using <see cref="Demography"/>. To actually generate the data
    /// call <see cref="SetupTestData"/>
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="targetDatabase"></param>
    /// <param name="numberOfRows"></param>
    public BulkTestsData(ICatalogueRepository repository, DiscoveredDatabase targetDatabase, int numberOfRows = 10000)
    {
        _repository = repository;
        BulkDataDatabase = targetDatabase;
        ExpectedNumberOfRowsInTestData = numberOfRows;

        _dataGenerator = new Demography(new Random(500));
    }

    /// <summary>
    /// Creates the <see cref="BulkDataTable"/> in the <see cref="BulkDataDatabase"/> and uploads test data.  Use <see cref="ImportAsCatalogue"/> to get
    /// rdmp metadata objects pointing at the table.
    /// </summary>
    public void SetupTestData()
    {
        //make sure database exists
        if (!BulkDataDatabase.Exists())
            BulkDataDatabase.Create();

        //generate some people
        var people = new PersonCollection();
        people.GeneratePeople(5000, r);

        //generate the test data
        var dt = _dataGenerator.GetDataTable(people, ExpectedNumberOfRowsInTestData);

        var tbl = BulkDataDatabase.ExpectTable(BulkDataTable);

        if (tbl.Exists())
            tbl.Drop();

        //create the table but make sure the chi is a primary key and the correct data type and that we have a sensible primary key
        Table = BulkDataDatabase.CreateTable(BulkDataTable, dt, new DatabaseColumnRequest[]
        {
            new("chi", new DatabaseTypeRequest(typeof(string), 10)) { IsPrimaryKey = true },
            new("dtCreated", new DatabaseTypeRequest(typeof(DateTime))) { IsPrimaryKey = true },
            new("hb_extract", new DatabaseTypeRequest(typeof(string), 1)) { IsPrimaryKey = true }
        });
    }

    /// <summary>
    /// Returns up to <paramref name="numberOfRows"/> rows from the table
    /// </summary>
    /// <param name="numberOfRows"></param>
    /// <returns></returns>
    public DataTable GetDataTable(int numberOfRows) =>
        BulkDataDatabase.ExpectTable(BulkDataTable).GetDataTable(numberOfRows);

    /// <summary>
    /// Creates Rdmp metadata objects (<see cref="catalogue"/>, <see cref="tableInfo"/> etc) which point to the <see cref="BulkDataTable"/>
    /// </summary>
    /// <returns></returns>
    public ICatalogue ImportAsCatalogue()
    {
        var f = new TableInfoImporter(_repository, BulkDataDatabase.ExpectTable(BulkDataTable));
        f.DoImport(out tableInfo, out columnInfos);

        var forwardEngineer = new ForwardEngineerCatalogue(tableInfo, columnInfos);
        forwardEngineer.ExecuteForwardEngineering(out var c, out catalogueItems, out extractionInformations);
        catalogue = c;

        var chi = extractionInformations.Single(e => e.GetRuntimeName().Equals("chi"));
        chi.IsExtractionIdentifier = true;
        chi.SaveToDatabase();

        return catalogue;
    }

    /// <summary>
    /// Deletes the rdmp metadata objects (but not the actual <see cref="BulkDataTable"/>)
    /// </summary>
    public void DeleteCatalogue()
    {
        var creds = (DataAccessCredentials)tableInfo.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing);

        if (tableInfo.Exists())
            tableInfo.DeleteInDatabase();

        if (creds != null)
            try
            {
                creds.DeleteInDatabase();
            }
            catch (CredentialsInUseException e)
            {
                Console.WriteLine($"Ignored Potential Exception:{e}");
            }

        if (catalogue.Exists())
            catalogue.DeleteInDatabase();
    }

    public void SetupValidationOnCatalogue()
    {
        var v = new Validator();
        var iv = new ItemValidator("chi")
        {
            PrimaryConstraint = new Chi()
        };
        iv.PrimaryConstraint.Consequence = Consequence.Wrong;

        v.AddItemValidator(iv, "chi", typeof(string));
        catalogue.ValidatorXML = v.SaveToXml();

        catalogue.TimeCoverage_ExtractionInformation_ID =
            catalogue.GetAllExtractionInformation(ExtractionCategory.Any)
                .Single(e => e.GetRuntimeName().Equals("dtCreated")).ID;

        catalogue.SaveToDatabase();
    }

    /// <summary>
    /// Returns the <see cref="ColumnInfo"/> in <see cref="columnInfos"/> with the given <paramref name="colName"/>
    /// </summary>
    /// <param name="colName"></param>
    /// <returns></returns>
    public ColumnInfo GetColumnInfo(string colName)
    {
        return columnInfos.Single(c => c.GetRuntimeName().Equals(colName));
    }
}