// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.ANOEngineering;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.Curation.Anonymisation;

public class ForwardEngineerANOCatalogueTwoTableTests : TestsRequiringANOStore
{
    private ITableInfo t1;
    private ColumnInfo[] c1;

    private ITableInfo t2;
    private ColumnInfo[] c2;

    private ICatalogue cata1;
    private ICatalogue cata2;

    private CatalogueItem[] cataItems1;
    private CatalogueItem[] cataItems2;

    private ExtractionInformation[] eis1;
    private ExtractionInformation[] eis2;
    private ANOTable _anoTable;
    private Catalogue _comboCata;
    private DiscoveredDatabase _destinationDatabase;

    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();

        const string sql = @"CREATE TABLE [dbo].[Tests](
	[chi] [varchar](10) NULL,
	[Date] [datetime] NULL,
	[hb_extract] [varchar](1) NULL,
	[TestId] [int] NOT NULL,
 CONSTRAINT [PK_Tests] PRIMARY KEY CLUSTERED 
(
	[TestId] ASC
)
) 

GO

CREATE TABLE [dbo].[Results](
	[TestId] [int] NOT NULL,
	[Measure] [varchar](10) NOT NULL,
	[Value] [int] NULL,
 CONSTRAINT [PK_Results] PRIMARY KEY CLUSTERED 
(
	[TestId] ASC,
	[Measure] ASC
)
)

GO

ALTER TABLE [dbo].[Results]  WITH CHECK ADD  CONSTRAINT [FK_Results_Tests] FOREIGN KEY([TestId])
REFERENCES [dbo].[Tests] ([TestId])
GO";

        var server = From.Server;
        using (var con = server.GetConnection())
        {
            con.Open();
            UsefulStuff.ExecuteBatchNonQuery(sql, con);
        }

        var importer1 = new TableInfoImporter(CatalogueRepository, From.ExpectTable("Tests"));
        var importer2 = new TableInfoImporter(CatalogueRepository, From.ExpectTable("Results"));

        importer1.DoImport(out t1, out c1);

        importer2.DoImport(out t2, out c2);

        var engineer1 = new ForwardEngineerCatalogue(t1, c1);
        var engineer2 = new ForwardEngineerCatalogue(t2, c2);

        engineer1.ExecuteForwardEngineering(out cata1, out cataItems1, out eis1);
        engineer2.ExecuteForwardEngineering(out cata2, out cataItems2, out eis2);

        new JoinInfo(CatalogueRepository,
            c1.Single(e => e.GetRuntimeName().Equals("TestId")),
            c2.Single(e => e.GetRuntimeName().Equals("TestId")),
            ExtractionJoinType.Left, null);

        _anoTable = new ANOTable(CatalogueRepository, ANOStore_ExternalDatabaseServer, "ANOTes", "T")
        {
            NumberOfCharactersToUseInAnonymousRepresentation = 10
        };
        _anoTable.SaveToDatabase();
        _anoTable.PushToANOServerAsNewTable("int", ThrowImmediatelyCheckNotifier.Quiet);

        _comboCata = new Catalogue(CatalogueRepository, "Combo Catalogue");

        //pk
        var ciTestId = new CatalogueItem(CatalogueRepository, _comboCata, "TestId");
        var colTestId = c1.Single(c => c.GetRuntimeName().Equals("TestId"));
        ciTestId.ColumnInfo_ID = colTestId.ID;
        ciTestId.SaveToDatabase();

        //Measure
        var ciMeasure = new CatalogueItem(CatalogueRepository, _comboCata, "Measuree");
        var colMeasure = c2.Single(c => c.GetRuntimeName().Equals("Measure"));
        ciMeasure.ColumnInfo_ID = colMeasure.ID;
        ciMeasure.SaveToDatabase();

        //Date
        var ciDate = new CatalogueItem(CatalogueRepository, _comboCata, "Dat");

        var colDate = c1.Single(c => c.GetRuntimeName().Equals("Date"));
        ciDate.ColumnInfo_ID = colDate.ID;
        ciDate.SaveToDatabase();

        _destinationDatabase = To;
    }


    [Test]
    public void TestAnonymisingJoinKey()
    {
        //Create a plan for the first Catalogue (Tests) - single Table dataset
        var plan1 = new ForwardEngineerANOCataloguePlanManager(RepositoryLocator, cata1);
        var testIdHeadPlan = plan1.GetPlanForColumnInfo(c1.Single(c => c.GetRuntimeName().Equals("TestId")));
        plan1.TargetDatabase = _destinationDatabase;

        //the plan is that the column TestId should be anonymised - where its name will become ANOTestId
        testIdHeadPlan.Plan = Plan.ANO;
        testIdHeadPlan.ANOTable = _anoTable;

        plan1.Check(ThrowImmediatelyCheckNotifier.Quiet);

        var engine1 = new ForwardEngineerANOCatalogueEngine(RepositoryLocator, plan1);
        engine1.Execute();

        var plan1ExtractionInformationsAtDestination =
            engine1.NewCatalogue.GetAllExtractionInformation(ExtractionCategory.Any);

        var ei1 = plan1ExtractionInformationsAtDestination.Single(e => e.GetRuntimeName().Equals("ANOTestId"));
        Assert.That(ei1.Exists());

        //Now create a plan for the combo Catalogue which contains references to both tables (Tests and Results).  Remember Tests has already been migrated as part of plan1
        var plan2 = new ForwardEngineerANOCataloguePlanManager(RepositoryLocator, _comboCata);

        //tell it to skip table 1 (Tests) and only anonymise Results
        plan2.SkippedTables.Add(t1);
        plan2.TargetDatabase = _destinationDatabase;
        plan2.Check(ThrowImmediatelyCheckNotifier.Quiet);

        //Run the anonymisation
        var engine2 = new ForwardEngineerANOCatalogueEngine(RepositoryLocator, plan2);
        engine2.Execute();

        //Did it successfully pick SetUp the correct ANO column
        var plan2ExtractionInformationsAtDestination =
            engine2.NewCatalogue.GetAllExtractionInformation(ExtractionCategory.Any);

        var ei2 = plan2ExtractionInformationsAtDestination.Single(e => e.GetRuntimeName().Equals("ANOTestId"));
        Assert.That(ei2.Exists());

        //and can the query be executed successfully
        var qb = new QueryBuilder(null, null);
        qb.AddColumnRange(plan2ExtractionInformationsAtDestination);

        using (var con = _destinationDatabase.Server.GetConnection())
        {
            con.Open();

            var cmd = _destinationDatabase.Server.GetCommand(qb.SQL, con);

            Assert.DoesNotThrow(() => cmd.ExecuteNonQuery());
        }

        Console.WriteLine($"Final migrated combo dataset SQL was:{qb.SQL}");

        Assert.Multiple(() =>
        {
            Assert.That(_comboCata.CatalogueItems.Any(ci => ci.Name.Equals("Measuree")));
            Assert.That(engine2.NewCatalogue.CatalogueItems.Any(ci => ci.Name.Equals("Measuree")),
                "ANO Catalogue did not respect the original CatalogueItem Name");
        });
    }
}