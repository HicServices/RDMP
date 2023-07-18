// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Microsoft.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.EntityNaming;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Modules.Mutilators;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

public class BackfillTests : FromToDatabaseTests
{

    private ICatalogue _catalogue;


    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();

        BlitzMainDataTables();

        DeleteTables(From);
        DeleteTables(To);
    }


    [Test]
    public void Backfill_SingleTable_LoadContainsNewerUpdate()
    {
        SingleTableSetup();

        #region Insert test data
        // add To data
        using (var connection = (SqlConnection)To.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(
                $"INSERT INTO [Samples] (ID, SampleDate, Description, {SpecialFieldNames.ValidFrom}, {SpecialFieldNames.DataLoadRunID}) VALUES (10, '2016-01-10T12:00:00', 'Earlier than corresponding new data, should be updated', '2016-01-10T12:00:00', 1)", connection);
            cmd.ExecuteNonQuery();
        }

        // add From data
        using (var connection = (SqlConnection)From.Server.GetConnection())
        {
            connection.Open();

            // newer update
            var cmd = new SqlCommand("INSERT INTO [Samples] (ID, SampleDate, Description) VALUES " +
                                     "(10, '2016-01-11T12:00:00', 'Newer than in To, should update To')", connection);
            cmd.ExecuteNonQuery();
        }
        #endregion

        // databases are now represent state after push to From and before migration
        Mutilate($"[{DatabaseName}].[dbo].[Samples].[SampleDate]");

        // check that From contains the correct data
        using (var connection = (SqlConnection)From.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(@"SELECT COUNT(*) FROM Samples", connection);
            var numRows = cmd.ExecuteScalar();
            Assert.AreEqual(1, numRows, "Should still be 1 record, this would be migrated to To");

            cmd = new SqlCommand(@"SELECT Description FROM Samples", connection);
            var description = cmd.ExecuteScalar().ToString();
            Assert.AreEqual(description, "Newer than in To, should update To", "Description has been altered but is a valid update to To so should not have been touched.");
        }
    }

    private void SingleTableSetup()
    {
        CreateTables("Samples", "ID int NOT NULL, SampleDate DATETIME, Description varchar(1024)", "ID");

        // Set SetUp catalogue entities
        AddTableToCatalogue(DatabaseName, "Samples", "ID", out _, true);

        Assert.AreEqual(5, _catalogue.CatalogueItems.Length, "Unexpected number of items in catalogue");
    }

    private void Mutilate(string timeColumnName)
    {
        var mutilator = new StagingBackfillMutilator
        {
            TimePeriodicityField = CatalogueRepository.GetAllObjects<ColumnInfo>().Single(c=>c.Name.Equals(timeColumnName)),
            TestContext = true,
            TableNamingScheme = new IdentityTableNamingScheme()
        };

        mutilator.Initialize(From, LoadStage.AdjustStaging);
        mutilator.Check(ThrowImmediatelyCheckNotifier.Quiet);
        mutilator.Mutilate(new ThrowImmediatelyDataLoadJob(To.Server));
    }

    [Test]
    public void Backfill_SingleTable_LoadContainsOlderUpdate()
    {
        SingleTableSetup();

        #region Insert test data
        // add To data
        using (var connection = (SqlConnection)To.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(
                $"INSERT INTO [Samples] (ID, SampleDate, Description, {SpecialFieldNames.ValidFrom}, {SpecialFieldNames.DataLoadRunID}) VALUES (1, '2016-01-10T12:00:00', 'Later than corresponding new data, should not be updated', '2016-01-10T12:00:00', 1)", connection);
            cmd.ExecuteNonQuery();
        }

        // add From data
        using (var connection = (SqlConnection)From.Server.GetConnection())
        {
            connection.Open();

            // newer update
            var cmd = new SqlCommand("INSERT INTO [Samples] (ID, SampleDate, Description) VALUES " +
                                     "(1, '2016-01-09T12:00:00', 'Older than in To, should be deleted by the mutilator')", connection);
            cmd.ExecuteNonQuery();
        }
        #endregion

        // databases are now represent state after push to From and before migration
        Mutilate($"[{DatabaseName}].[dbo].[Samples].[SampleDate]");

        // check that From contains the correct data
        using (var connection = (SqlConnection)From.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(@"SELECT COUNT(*) FROM Samples", connection);
            var numRows = cmd.ExecuteScalar();
            Assert.AreEqual(0, numRows, "The record to be loaded is older than the corresponding record in To, should have been deleted");
        }
    }

    [Test]
    public void Backfill_SingleTable_LoadContainsInsert()
    {
        SingleTableSetup();

        #region Insert test data
        // add To data
        using (var connection = (SqlConnection)To.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand("INSERT INTO [Samples] (ID, SampleDate, Description, hic_validFrom, hic_dataLoadRunID) VALUES " +
                                     "(1, '2016-01-10T12:00:00', 'Later than corresponding new data, should not be updated', '2016-01-10T12:00:00', 1)", connection);
            cmd.ExecuteNonQuery();
        }

        // add From data
        using (var connection = (SqlConnection)From.Server.GetConnection())
        {
            connection.Open();

            // newer update
            var cmd = new SqlCommand("INSERT INTO [Samples] (ID, SampleDate, Description) VALUES " +
                                     "(2, '2016-01-09T12:00:00', 'Does not exist in To, should remain in From after mutilation.')", connection);
            cmd.ExecuteNonQuery();
        }
        #endregion

        // databases are now represent state after push to From and before migration
        Mutilate($"[{DatabaseName}].[dbo].[Samples].[SampleDate]");

        // check that From contains the correct data
        using (var connection = (SqlConnection)From.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(@"SELECT COUNT(*) FROM Samples", connection);
            var numRows = cmd.ExecuteScalar();
            Assert.AreEqual(1, numRows, "The record to be loaded is an insert should not have been deleted");

            cmd = new SqlCommand(@"SELECT Description FROM Samples", connection);
            var description = cmd.ExecuteScalar().ToString();
            Assert.AreEqual(description, "Does not exist in To, should remain in From after mutilation.", "Description has been altered but is a valid update to To so should not have been touched.");

        }
    }

    [Test]
    public void Backfill_SingleTable_Combined()
    {
        SingleTableSetup();

        #region Insert test data
        // add To data
        using (var connection = (SqlConnection)To.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand("INSERT INTO [Samples] (ID, SampleDate, Description, hic_validFrom, hic_dataLoadRunID) VALUES " +
                                     "(1, '2016-01-10T12:00:00', 'Earlier than corresponding new data, should be updated', '2016-01-10T12:00:00', 1)," +
                                     "(2, '2016-01-15T12:00:00', 'Later than corresponding new data, should not be updated', '2016-01-15T12:00:00', 2)", connection);
            cmd.ExecuteNonQuery();
        }

        // add From data
        using (var connection = (SqlConnection)From.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand("INSERT INTO [Samples] (ID, SampleDate, Description) VALUES " +
                                     "(1, '2016-01-12T12:00:00', 'Later than corresponding new data, should not be updated')," +
                                     "(2, '2016-01-12T12:00:00', 'Earlier than corresponding new data, should be updated')," +
                                     "(3, '2016-01-12T12:00:00', 'New data')", connection);
            cmd.ExecuteNonQuery();
        }
        #endregion

        Mutilate($"[{DatabaseName}].[dbo].[Samples].[SampleDate]");

        // todo: asserts
        using (var connection = (SqlConnection)From.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(@"SELECT COUNT(*) FROM Samples", connection);
            var numRows = cmd.ExecuteScalar();
            Assert.AreEqual(2, numRows, "Record 2 should have been deleted as it is an update to a record for which we have a later version.");
        }

    }

    private void TwoTableSetupWhereTimePeriodIsParent()
    {
        CreateTables("Samples", "ID int NOT NULL, SampleDate DATETIME, Description varchar(1024)", "ID");
        CreateTables("Results", "ID int NOT NULL, SampleID int NOT NULL, Result int", "ID", "CONSTRAINT [FK_Samples_Results] FOREIGN KEY (SampleID) REFERENCES Samples (ID)");

        // Set SetUp catalogue entities

        var tiSamples = AddTableToCatalogue(DatabaseName, "Samples", "ID", out var ciSamples, true);
        AddTableToCatalogue(DatabaseName, "Results", "ID", out var ciResults);

        _catalogue.Time_coverage = "[Samples].[SampleDate]";
        _catalogue.SaveToDatabase();

        tiSamples.IsPrimaryExtractionTable = true;
        tiSamples.SaveToDatabase();

        Assert.AreEqual(10, _catalogue.CatalogueItems.Length, "Unexpected number of items in catalogue");

        // Samples (1:M) Results join
        new JoinInfo(CatalogueRepository,ciResults.Single(info => info.GetRuntimeName().Equals("SampleID")),
            ciSamples.Single(info => info.GetRuntimeName().Equals("ID")),
            ExtractionJoinType.Left, "");
    }

    [Test]
    public void Backfill_TwoTables_TimePeriodParent_LoadContainsNewerUpdate()
    {
        TwoTableSetupWhereTimePeriodIsParent();

        #region Insert To test data
        const string liveSamplesSql = "INSERT INTO Samples (ID, SampleDate, Description, hic_validFrom, hic_dataLoadRunID) VALUES " +
                                      "(1, '2016-01-10T12:00:00', '', '2016-01-10T12:00:00', 1)";

        const string liveResultsSql = "INSERT INTO Results (ID, SampleID, Result, hic_validFrom, hic_dataLoadRunID) VALUES " +
                                      "(10, 1, 123, '2016-01-10T12:00:00', 1), " +
                                      "(11, 1, 234, '2016-01-10T12:00:00', 1)";

        using (var connection = (SqlConnection)To.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(liveSamplesSql, connection);
            cmd.ExecuteNonQuery();

            cmd = new SqlCommand(liveResultsSql, connection);
            cmd.ExecuteNonQuery();
        }
        #endregion

        #region Add From test data
        // add From data
        const string stagingSamplesSql = "INSERT INTO Samples (ID, SampleDate, Description) VALUES " +
                                         "(1, '2016-01-15T12:00:00', 'Sample is later than corresponding record in To, contains a child update (ID=11), child insert (ID=12) and this updated description')";

        const string stagingResultsSql = "INSERT INTO Results (ID, SampleID, Result) VALUES " +
                                         "(10, 1, 123), " +
                                         "(11, 1, 345), " +
                                         "(12, 1, 456)";

        using (var connection = (SqlConnection)From.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(stagingSamplesSql, connection);
            cmd.ExecuteNonQuery();

            cmd = new SqlCommand(stagingResultsSql, connection);
            cmd.ExecuteNonQuery();
        }


        #endregion

        Mutilate($"[{DatabaseName}].[dbo].[Samples].[SampleDate]");

        // From should be exactly the same as it was before mutilation as there is a single update
        using (var connection = (SqlConnection)From.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(@"SELECT COUNT(*) FROM Samples", connection);
            var numRows = cmd.ExecuteScalar();
            Assert.AreEqual(1, numRows);

            cmd = new SqlCommand(@"SELECT COUNT(*) FROM Results", connection);
            numRows = cmd.ExecuteScalar();
            Assert.AreEqual(3, numRows);
        }
    }

    [Test]
    public void Backfill_TwoTables_TimePeriodParent_LoadContainsOlderUpdate()
    {
        TwoTableSetupWhereTimePeriodIsParent();

        #region Insert To test data
        const string liveSamplesSql = "INSERT INTO Samples (ID, SampleDate, Description, hic_validFrom, hic_dataLoadRunID) VALUES " +
                                      "(1, '2016-01-10T12:00:00', '', '2016-01-10T12:00:00', 1)";

        const string liveResultsSql = "INSERT INTO Results (ID, SampleID, Result, hic_validFrom, hic_dataLoadRunID) VALUES " +
                                      "(10, 1, 123, '2016-01-10T12:00:00', 1), " +
                                      "(11, 1, 234, '2016-01-10T12:00:00', 1)";

        using (var connection = (SqlConnection)To.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(liveSamplesSql, connection);
            cmd.ExecuteNonQuery();

            cmd = new SqlCommand(liveResultsSql, connection);
            cmd.ExecuteNonQuery();
        }
        #endregion

        #region Add From test data
        // add From data
        const string stagingSamplesSql = "INSERT INTO Samples (ID, SampleDate, Description) VALUES " +
                                         "(1, '2016-01-09T12:00:00', 'Sample is earlier than corresponding record in To (also contains an item which has apparently been deleted in the set used for a later load)')";

        const string stagingResultsSql = "INSERT INTO Results (ID, SampleID, Result) VALUES " +
                                         "(10, 1, 123), " +
                                         "(11, 1, 345), " +
                                         "(12, 1, 456)";

        using (var connection = (SqlConnection)From.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(stagingSamplesSql, connection);
            cmd.ExecuteNonQuery();

            cmd = new SqlCommand(stagingResultsSql, connection);
            cmd.ExecuteNonQuery();
        }


        #endregion

        Mutilate($"[{DatabaseName}].[dbo].[Samples].[SampleDate]");

        // From should be exactly the same as it was before mutilation as there is a single update
        using (var connection = (SqlConnection)From.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(@"SELECT COUNT(*) FROM Samples", connection);
            var numRows = cmd.ExecuteScalar();
            Assert.AreEqual(1, numRows, "Item should still remain as there still should be a single result to insert.");

            cmd = new SqlCommand(@"SELECT * FROM Results", connection);
            using (var reader = cmd.ExecuteReader())
            {
                Assert.IsTrue(reader.HasRows);

                reader.Read();
                Assert.AreEqual(12, reader["ID"]);

                var hasMoreResults = reader.Read();
                Assert.IsFalse(hasMoreResults, "Should only be one Result row left in From");
            }

            cmd = new SqlCommand(@"SELECT * FROM Samples", connection);
            using (var reader = cmd.ExecuteReader())
            {
                Assert.IsTrue(reader.HasRows);

                reader.Read();
                Assert.AreEqual("", reader["Description"].ToString(), "The To sample had a blank description which should have been copied in to the earlier From record.");

                var hasMoreResults = reader.Read();
                Assert.IsFalse(hasMoreResults, "Should only be one Samples row in From");
            }
        }
    }

    [Test]
    public void Backfill_TwoTables_TimePeriodParent_LoadContainInsert()
    {
        TwoTableSetupWhereTimePeriodIsParent();

        #region Insert To test data
        const string liveSamplesSql = "INSERT INTO Samples (ID, SampleDate, Description, hic_validFrom, hic_dataLoadRunID) VALUES " +
                                      "(1, '2016-01-10T12:00:00', '', '2016-01-10T12:00:00', 1)";

        const string liveResultsSql = "INSERT INTO Results (ID, SampleID, Result, hic_validFrom, hic_dataLoadRunID) VALUES " +
                                      "(10, 1, 123, '2016-01-10T12:00:00', 1), " +
                                      "(11, 1, 234, '2016-01-10T12:00:00', 1)";

        using (var connection = (SqlConnection)To.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(liveSamplesSql, connection);
            cmd.ExecuteNonQuery();

            cmd = new SqlCommand(liveResultsSql, connection);
            cmd.ExecuteNonQuery();
        }
        #endregion

        #region Add From test data
        // add From data
        const string stagingSamplesSql = "INSERT INTO Samples (ID, SampleDate, Description) VALUES " +
                                         "(2, '2016-01-15T12:00:00', 'New sample')";

        const string stagingResultsSql = "INSERT INTO Results (ID, SampleID, Result) VALUES " +
                                         "(13, 2, 333), " +
                                         "(14, 2, 555), " +
                                         "(15, 2, 666)";

        using (var connection = (SqlConnection)From.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(stagingSamplesSql, connection);
            cmd.ExecuteNonQuery();

            cmd = new SqlCommand(stagingResultsSql, connection);
            cmd.ExecuteNonQuery();
        }


        #endregion

        Mutilate($"[{DatabaseName}].[dbo].[Samples].[SampleDate]");

        // From should be exactly the same as it was before mutilation as there is a single update
        using (var connection = (SqlConnection)From.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(@"SELECT COUNT(*) FROM Samples", connection);
            var numRows = cmd.ExecuteScalar();
            Assert.AreEqual(1, numRows, "This is an insert, no data should be deleted/altered.");

            cmd = new SqlCommand(@"SELECT COUNT(*) FROM Results", connection);
            numRows = cmd.ExecuteScalar();
            Assert.AreEqual(3, numRows, "This is an insert, no data should be deleted/altered.");
        }
    }

    private void TwoTableSetupWhereTimePeriodIsChild()
    {
        CreateTables("Headers", "ID int NOT NULL, Discipline varchar(32)", "ID");
        CreateTables("Samples",
            "ID int NOT NULL, HeaderID int NOT NULL, SampleDate DATETIME, Description varchar(1024)", "ID",
            "CONSTRAINT [FK_Headers_Samples] FOREIGN KEY (HeaderID) REFERENCES Headers (ID)");

        // Set SetUp catalogue entities

        var tiSamples = AddTableToCatalogue(DatabaseName, "Samples", "ID", out var ciSamples, true);
        AddTableToCatalogue(DatabaseName, "Headers", "ID", out var ciHeaders);

        _catalogue.Time_coverage = "[Samples].[SampleDate]";
        _catalogue.SaveToDatabase();

        tiSamples.IsPrimaryExtractionTable = true;
        tiSamples.SaveToDatabase();

        Assert.AreEqual(10, _catalogue.CatalogueItems.Length, "Unexpected number of items in catalogue");

        // Headers (1:M) Samples join
        new JoinInfo(CatalogueRepository,ciSamples.Single(info => info.GetRuntimeName().Equals("HeaderID")),
            ciHeaders.Single(info => info.GetRuntimeName().Equals("ID")),
            ExtractionJoinType.Left, "");
    }

    [Test]
    public void Backfill_TwoTables_TimePeriodChild_LoadContainsOlderUpdate()
    {
        TwoTableSetupWhereTimePeriodIsChild();

        #region Insert To test data
        const string liveHeaderSql = "INSERT INTO Headers (ID, Discipline, hic_validFrom, hic_dataLoadRunID) VALUES " +
                                     "(1, 'Biochemistry', '2016-01-10T12:00:00', 1)";

        const string liveSamplesSql = "INSERT INTO Samples (ID, HeaderID, SampleDate, Description, hic_validFrom, hic_dataLoadRunID) VALUES " +
                                      "(10, 1, '2016-01-10T12:00:00', '', '2016-01-10T12:00:00', 1)";


        using (var connection = (SqlConnection)To.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(liveHeaderSql, connection);
            cmd.ExecuteNonQuery();

            cmd = new SqlCommand(liveSamplesSql, connection);
            cmd.ExecuteNonQuery();
        }
        #endregion

        #region Add From test data
        // add From data
        const string stagingHeadersSql = "INSERT INTO Headers (ID, Discipline) VALUES " +
                                         "(1, 'Biochemistry')";

        const string stagingSamplesSql = "INSERT INTO Samples (ID, HeaderID, SampleDate, Description) VALUES " +
                                         "(10, 1, '2016-01-05T12:00:00', '')";

        using (var connection = (SqlConnection)From.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(stagingHeadersSql, connection);
            cmd.ExecuteNonQuery();

            cmd = new SqlCommand(stagingSamplesSql, connection);
            cmd.ExecuteNonQuery();
        }


        #endregion

        Mutilate($"[{DatabaseName}].[dbo].[Samples].[SampleDate]");

        // From should be exactly the same as it was before mutilation as there is a single update
        using (var connection = (SqlConnection)From.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(@"SELECT COUNT(*) FROM Samples", connection);
            var numRows = cmd.ExecuteScalar();
            Assert.AreEqual(0, numRows, "Sample should be deleted as it is older than corresponding row in To.");

            cmd = new SqlCommand(@"SELECT COUNT(*) FROM Headers", connection);
            numRows = cmd.ExecuteScalar();
            Assert.AreEqual(0, numRows, "Header should have been pruned as it no longer has any children in From.");
        }
    }

    /// <summary>
    /// This test has an 'old' child insert, i.e. the date of the insert is before the newest child entry in To.
    /// Also, the parent data in To is different from that in From, so we need to ensure the entry in From is updated before we migrate the data,
    /// otherwise we will overwrite To with old data
    /// </summary>
    [Test]
    public void Backfill_TwoTables_TimePeriodChild_LoadContainsOldInsert_WithOldParentData()
    {
        TwoTableSetupWhereTimePeriodIsChild();

        #region Insert To test data
        const string liveHeaderSql = "INSERT INTO Headers (ID, Discipline, hic_validFrom, hic_dataLoadRunID) VALUES " +
                                     "(1, 'Biochemistry', '2016-01-15T12:00:00', 1)";

        const string liveSamplesSql = "INSERT INTO Samples (ID, HeaderID, SampleDate, Description, hic_validFrom, hic_dataLoadRunID) VALUES " +
                                      "(11, 1, '2016-01-15T12:00:00', '', '2016-01-15T12:00:00', 1)";


        using (var connection = (SqlConnection)To.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(liveHeaderSql, connection);
            cmd.ExecuteNonQuery();

            cmd = new SqlCommand(liveSamplesSql, connection);
            cmd.ExecuteNonQuery();
        }
        #endregion

        #region Add From test data
        // add From data
        const string stagingHeadersSql = "INSERT INTO Headers (ID, Discipline) VALUES " +
                                         "(1, 'Haematology')"; // old and incorrect Discipline value

        const string stagingSamplesSql = "INSERT INTO Samples (ID, HeaderID, SampleDate, Description) VALUES " +
                                         "(10, 1, '2016-01-05T12:00:00', '')"; // 'old' insert, missing from loaded data. Can only be added by including the parent, so need to make parent correct before migration

        using (var connection = (SqlConnection)From.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(stagingHeadersSql, connection);
            cmd.ExecuteNonQuery();

            cmd = new SqlCommand(stagingSamplesSql, connection);
            cmd.ExecuteNonQuery();
        }

        #endregion

        Mutilate($"[{DatabaseName}].[dbo].[Samples].[SampleDate]");

        // From should be exactly the same as it was before mutilation as there is a single update
        using (var connection = (SqlConnection)From.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(@"SELECT COUNT(*) FROM Samples", connection);
            var numRows = cmd.ExecuteScalar();
            Assert.AreEqual(1, numRows, "Should still be 1 sample");

            cmd = new SqlCommand(@"SELECT COUNT(*) FROM Headers", connection);
            numRows = cmd.ExecuteScalar();
            Assert.AreEqual(1, numRows, "Header should still be there (shouldn't be able to delete it as there should be a FK constraint with Samples)");

            cmd = new SqlCommand(@"SELECT Discipline FROM Headers WHERE ID=1", connection);
            var discipline = cmd.ExecuteScalar().ToString();
            Assert.AreEqual("Biochemistry", discipline, "Header record in From be updated to reflect what is in To: the To record is authoritative as it contains at least one child from a later date.");
        }
    }

    [Test]
    public void Backfill_TwoTables_TimePeriodChild_LoadContainsNewInsert_WithNewParentData()
    {
        TwoTableSetupWhereTimePeriodIsChild();

        #region Insert To test data
        const string liveHeaderSql = "INSERT INTO Headers (ID, Discipline, hic_validFrom, hic_dataLoadRunID) VALUES " +
                                     "(1, 'Biochemistry', '2016-01-15T12:00:00', 1)";

        const string liveSamplesSql = "INSERT INTO Samples (ID, HeaderID, SampleDate, Description, hic_validFrom, hic_dataLoadRunID) VALUES " +
                                      "(10, 1, '2016-01-15T10:00:00', '', '2016-01-15T12:00:00', 1), " +
                                      "(11, 1, '2016-01-15T12:00:00', '', '2016-01-15T12:00:00', 1)";


        using (var connection = (SqlConnection)To.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(liveHeaderSql, connection);
            cmd.ExecuteNonQuery();

            cmd = new SqlCommand(liveSamplesSql, connection);
            cmd.ExecuteNonQuery();
        }
        #endregion

        #region Add From test data
        // add From data
        const string stagingHeadersSql = "INSERT INTO Headers (ID, Discipline) VALUES " +
                                         "(1, 'Haematology')"; // old and incorrect Discipline value

        const string stagingSamplesSql = "INSERT INTO Samples (ID, HeaderID, SampleDate, Description) VALUES " +
                                         "(12, 1, '2016-01-16T12:00:00', '')"; // 'new' insert, missing from loaded data. SampleDate is newer than any in To so this means that the updated parent data is 'correct'

        using (var connection = (SqlConnection)From.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(stagingHeadersSql, connection);
            cmd.ExecuteNonQuery();

            cmd = new SqlCommand(stagingSamplesSql, connection);
            cmd.ExecuteNonQuery();
        }

        #endregion

        Mutilate($"[{DatabaseName}].[dbo].[Samples].[SampleDate]");

        // From should be exactly the same as it was before mutilation as there is a single update
        using (var connection = (SqlConnection)From.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(@"SELECT COUNT(*) FROM Samples", connection);
            var numRows = cmd.ExecuteScalar();
            Assert.AreEqual(1, numRows, "Should still be 1 sample");

            cmd = new SqlCommand(@"SELECT COUNT(*) FROM Headers", connection);
            numRows = cmd.ExecuteScalar();
            Assert.AreEqual(1, numRows, "Header should still be there (shouldn't be able to delete it as there should be a FK constraint with Samples)");

            cmd = new SqlCommand(@"SELECT Discipline FROM Headers WHERE ID=1", connection);
            var discipline = cmd.ExecuteScalar().ToString();
            Assert.AreEqual("Haematology", discipline, "Header record in From should not be updated as it is 'correct'.");
        }
    }

    [Test]
    public void Backfill_TwoTables_TimePeriodChild_Combined()
    {
        TwoTableSetupWhereTimePeriodIsChild();

        #region Insert To test data
        const string liveHeaderSql = "INSERT INTO Headers (ID, Discipline, hic_validFrom, hic_dataLoadRunID) VALUES " +
                                     "(1, 'Haematology', '2016-01-15T12:00:00', 2), " +
                                     "(2, 'Haematology', '2016-01-05T12:00:00', 1), " +
                                     "(3, 'Biochemistry', '2016-01-15T12:00:00', 2), " +
                                     "(4, 'Haematology', '2016-01-15T12:00:00', 2)";

        const string liveSamplesSql = "INSERT INTO Samples (ID, HeaderID, SampleDate, Description, hic_validFrom, hic_dataLoadRunID) VALUES " +
                                      "(12, 1, '2016-01-15T12:00:00', '', '2016-01-15T12:00:00', 2), " +
                                      "(13, 1, '2016-01-15T12:00:00', '', '2016-01-15T12:00:00', 2), " +
                                      "(14, 2, '2016-01-05T12:00:00', '', '2016-01-05T12:00:00', 1), " +
                                      "(15, 3, '2016-01-15T12:00:00', '', '2016-01-15T12:00:00', 2), " +
                                      "(16, 4, '2016-01-15T12:00:00', '', '2016-01-15T12:00:00', 2)";


        using (var connection = (SqlConnection)To.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(liveHeaderSql, connection);
            cmd.ExecuteNonQuery();

            cmd = new SqlCommand(liveSamplesSql, connection);
            cmd.ExecuteNonQuery();
        }
        #endregion

        #region Add From test data
        // add From data
        const string stagingHeadersSql = "INSERT INTO Headers (ID, Discipline) VALUES " +
                                         "(1, 'Biochemistry'), " +
                                         "(2, 'Biochemistry'), " +
                                         "(3, 'Biochemistry'), " +
                                         "(5, 'Biochemistry')";

        const string stagingSamplesSql = "INSERT INTO Samples (ID, HeaderID, SampleDate, Description) VALUES " +
                                         "(11, 1, '2016-01-05T12:00:00', ''), " +
                                         "(13, 1, '2016-01-05T12:00:00', ''), " +
                                         "(14, 2, '2016-01-15T12:00:00', ''), " +
                                         "(15, 3, '2016-01-05T12:00:00', ''), " +
                                         "(17, 5, '2016-01-05T12:00:00', '')";

        using (var connection = (SqlConnection)From.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(stagingHeadersSql, connection);
            cmd.ExecuteNonQuery();

            cmd = new SqlCommand(stagingSamplesSql, connection);
            cmd.ExecuteNonQuery();
        }

        #endregion

        Mutilate($"[{DatabaseName}].[dbo].[Samples].[SampleDate]");

        using (var connection = (SqlConnection)From.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(@"SELECT * FROM Samples ORDER BY ID", connection);
            using (var reader = cmd.ExecuteReader())
            {
                reader.Read();
                Assert.AreEqual(11, reader["ID"]);
                Assert.AreEqual("2016-01-05T12:00:00", ((DateTime)reader["SampleDate"]).ToString("s"));

                reader.Read();
                Assert.AreEqual(14, reader["ID"]);
                Assert.AreEqual("2016-01-15T12:00:00", ((DateTime)reader["SampleDate"]).ToString("s"));

                reader.Read();
                Assert.AreEqual(17, reader["ID"]);
                Assert.AreEqual("2016-01-05T12:00:00", ((DateTime)reader["SampleDate"]).ToString("s"));

                Assert.IsFalse(reader.Read(), "Should only be three samples");
            }

            cmd = new SqlCommand(@"SELECT * FROM Headers ORDER BY ID", connection);
            using (var reader = cmd.ExecuteReader())
            {
                Assert.IsTrue(reader.HasRows);

                reader.Read();
                Assert.AreEqual(1, reader["ID"]);
                Assert.AreEqual("Haematology", reader["Discipline"]);

                reader.Read();
                Assert.AreEqual(2, reader["ID"]);
                Assert.AreEqual("Biochemistry", reader["Discipline"]);

                reader.Read();
                Assert.AreEqual(5, reader["ID"]);
                Assert.AreEqual("Biochemistry", reader["Discipline"]);

                Assert.IsFalse(reader.Read(), "Should only be three headers");
            }
        }
    }

    private void CreateTables(string tableName, string columnDefinitions, string pkColumn, string fkConstraintString = null)
    {
        // todo: doesn't do combo primary keys yet

        if (pkColumn == null || string.IsNullOrWhiteSpace(pkColumn))
            throw new InvalidOperationException("Primary Key column is required.");

        var pkConstraint = $"CONSTRAINT PK_{tableName} PRIMARY KEY ({pkColumn})";
        var stagingTableDefinition = $"{columnDefinitions}, {pkConstraint}";
        var liveTableDefinition =
            $"{columnDefinitions}, hic_validFrom DATETIME, hic_dataLoadRunID int, {pkConstraint}";

        if (fkConstraintString != null)
        {
            stagingTableDefinition += $", {fkConstraintString}";
            liveTableDefinition += $", {fkConstraintString}";
        }


        using (var con = (SqlConnection) From.Server.GetConnection())
        {
            con.Open();
            new SqlCommand($"CREATE TABLE {tableName} ({stagingTableDefinition})",con).ExecuteNonQuery();
        }

        using(var con = (SqlConnection)To.Server.GetConnection())
        {
            con.Open();
            new SqlCommand($"CREATE TABLE {tableName} ({liveTableDefinition})",con).ExecuteNonQuery();
        }
    }

    [Test, Ignore("Restructuring tests")]
    public void DeleteNewerCollisionsFromTable()
    {
        #region Set SetUp databases
        CreateTables("Header", "ID int NOT NULL, Discipline varchar(32) NOT NULL", "ID");

        CreateTables("Samples",
            "ID int NOT NULL, HeaderID int NOT NULL, SampleDate DATETIME, Description varchar(1024)",
            "CONSTRAINT FK_Header_Samples FOREIGN KEY (HeaderID) REFERENCES Header (ID)");

        CreateTables("Results", "ID int NOT NULL, SampleID int NOT NULL, Result int",
            "CONSTRAINT [FK_Samples_Results] FOREIGN KEY (SampleID) REFERENCES Samples (ID)");

        #endregion

        #region Set SetUp catalogue entities

        var tiSamples = AddSamplesTableToCatalogue(DatabaseName, out var ciSamples);
        var tiResults = AddResultsTableToCatalogue(DatabaseName, ciSamples);
        var tiHeaders = AddHeaderTableToCatalogue(DatabaseName, ciSamples);

        // should be all entities set SetUp now
        Assert.AreEqual(15, _catalogue.CatalogueItems.Length, "Unexpected number of items in catalogue");
        #endregion

        // add data
        #region Populate Tables
        var connection = (SqlConnection)To.Server.GetConnection();
        connection.Open();

        const string liveHeaderDataSql = "INSERT INTO Header (ID, Discipline, hic_validFrom, hic_dataLoadRunID) VALUES " +
                                         "(1, 'Biochemistry', '2016-01-15T15:00:00', 4), " +
                                         "(3, 'Haematology', '2016-01-15T12:00:00', 3)";
        var liveHeaderDataSqlCommand = new SqlCommand(liveHeaderDataSql, connection);
        liveHeaderDataSqlCommand.ExecuteNonQuery();

        const string liveSampleDataSql = "INSERT INTO Samples (ID, HeaderID, SampleDate, Description, hic_validFrom, hic_dataLoadRunID) VALUES " +
                                         "(10, 1, '2016-01-10T12:00:00', 'Earlier than corresponding new data, should be updated', '2016-01-10T12:00:00', 1), " +
                                         "(11, 1, '2016-01-15T15:00:00', 'Later than corresponding new data, should not be touched', '2016-01-15T15:00:00', 4), " +
                                         "(14, 3, '2016-01-15T12:00:00', 'Header data of newly loaded (but older) sample should *not* update this rows parent header', '2016-01-15T12:00:00', 3)";
        var liveSampleDataSqlCommand = new SqlCommand(liveSampleDataSql, connection);
        liveSampleDataSqlCommand.ExecuteNonQuery();

        const string liveResultDataSql = "INSERT INTO Results (ID, SampleID, Result, hic_validFrom, hic_dataLoadRunID) VALUES " +
                                         "(100, 10, 999, '2016-01-10T12:00:00', 1), " +
                                         "(101, 10, 888, '2016-01-10T12:00:00', 1), " +
                                         "(102, 11, 456, '2016-01-15T15:00:00', 4), " +
                                         "(103, 11, 654, '2016-01-15T15:00:00', 4), " +
                                         "(107, 14, 111, '2016-01-15T12:00:00', 3)";
        var liveResultDataSqlCommand = new SqlCommand(liveResultDataSql, connection);
        liveResultDataSqlCommand.ExecuteNonQuery();
        connection.Close();

        connection = (SqlConnection)From.Server.GetConnection();
        connection.Open();

        const string stagingHeaderDataSql = "INSERT INTO Header (ID, Discipline) VALUES " +
                                            "(1, 'Haematology')," +
                                            "(2, 'Biochemistry'), " +
                                            "(3, 'Biochemistry')";
        var stagingHeaderDataSqlCommand = new SqlCommand(stagingHeaderDataSql, connection);
        stagingHeaderDataSqlCommand.ExecuteNonQuery();

        const string stagingSampleDataSql = "INSERT INTO Samples (ID, HeaderID, SampleDate, Description) VALUES " +
                                            "(10, 1, '2016-01-12T13:00:00', 'Later than To data, represents an update and should overwrite To'), " +
                                            "(11, 1, '2016-01-12T13:00:00', 'Earlier than To data, should not overwrite To'), " +
                                            "(12, 2, '2016-01-12T13:00:00', 'New data that we did not have before')," +
                                            "(13, 3, '2016-01-14T12:00:00', 'New data that we did not have before, but parent header record is wrong and been corrected in an earlier load for a later timeperiod (is Biochemistry here but To value of Haematology is correct)')";
        var stagingSampleDataSqlCommand = new SqlCommand(stagingSampleDataSql, connection);
        stagingSampleDataSqlCommand.ExecuteNonQuery();


        const string stagingResultDataSql = "INSERT INTO Results (ID, SampleID, Result) VALUES " +
                                            "(100, 10, 777), " + // changed 999 to 777
                                            "(101, 10, 888), " + // unchanged
                                            "(104, 10, 666), " + // added this
                                            "(102, 11, 400), " + // earlier data (which is also wrong, 456 in To), To data is newer and corrected
                                            "(103, 11, 654), " +
                                            "(105, 12, 123), " +  // new result (from new sample)
                                            "(106, 13, 123)"; // new result (from new sample)
        var stagingResultDataSqlCommand = new SqlCommand(stagingResultDataSql, connection);
        stagingResultDataSqlCommand.ExecuteNonQuery();

        connection.Close();
        #endregion

        // databases are now represent state after push to From and before migration
        var mutilator = new StagingBackfillMutilator
        {
            TimePeriodicityField = CatalogueRepository.GetAllObjects<ColumnInfo>().Single(ci=>ci.Name ==
                $"[{DatabaseName}]..[Samples].[SampleDate]"),
            TestContext = true,
            TableNamingScheme = new IdentityTableNamingScheme()
        };

        mutilator.Initialize(From, LoadStage.AdjustStaging);
        mutilator.Check(ThrowImmediatelyCheckNotifier.Quiet);
        mutilator.Mutilate(new ThrowImmediatelyDataLoadJob());

        #region Assert
        // check that From contains the correct data
        // Sample ID=2 should have been deleted, along with corresponding results 102 and 103
        connection = (SqlConnection)From.Server.GetConnection();
        connection.Open();

        var cmd = new SqlCommand(@"SELECT COUNT(*) FROM Header", connection);
        var numRows = cmd.ExecuteScalar();
        Assert.AreEqual(3, numRows, "Should be 3 header records");

        cmd = new SqlCommand(@"SELECT Discipline FROM Header WHERE ID=1", connection);
        var discipline = cmd.ExecuteScalar();
        Assert.AreEqual("Biochemistry", discipline, "The mutilator should **NOT** have updated record 1 from Biochemistry to Haematology. Although the load updates one of the To samples, the most recent To sample is later than the most recent loaded sample so the parent data in To takes precedence over the parent data in From.");

        // Not convinced about this test case
        //cmd = new SqlCommand(@"SELECT Discipline FROM Header WHERE ID=3", connection);
        //discipline = cmd.ExecuteScalar();
        //Assert.AreEqual("Haematology", discipline, "The load should **not** have updated record 3 from Haematology to Biochemistry since the loaded record is from an earlier timeperiod.");

        cmd = new SqlCommand(@"SELECT COUNT(*) FROM Samples WHERE ID = 2", connection);
        numRows = cmd.ExecuteScalar();
        Assert.AreEqual(0, numRows, "Sample ID = 2 has not been deleted");

        cmd = new SqlCommand(@"SELECT COUNT(*) FROM Samples", connection);
        numRows = cmd.ExecuteScalar();
        Assert.AreEqual(2, numRows, "Sample ID = 2 has been deleted but something has happened to the other samples (should be untouched)");

        cmd = new SqlCommand(@"SELECT COUNT(*) FROM Results WHERE SampleID = 2", connection);
        numRows = cmd.ExecuteScalar();
        Assert.AreEqual(0, numRows, "Results belonging to Sample ID = 2 have not been deleted");

        cmd = new SqlCommand(@"SELECT COUNT(*) FROM Results", connection);
        numRows = cmd.ExecuteScalar();
        Assert.AreEqual(4, numRows, "Results belonging to Sample ID = 2 have been deleted but something has happeded to the other results (should be untouched)");

        connection.Close();
        #endregion

        tiSamples.DeleteInDatabase();
        tiResults.DeleteInDatabase();
        tiHeaders.DeleteInDatabase();
    }

    private ITableInfo AddSamplesTableToCatalogue(string databaseName, out ColumnInfo[] ciList)
    {
        var ti = AddTableToCatalogue(databaseName, "Samples", "ID", out ciList, true);
        _catalogue.Name = databaseName;

        // todo: what should this text actually look like
        _catalogue.Time_coverage = "[Samples].[SampleDate]";
        _catalogue.SaveToDatabase();
        return ti;
    }

    private ITableInfo AddResultsTableToCatalogue(string databaseName, ColumnInfo[] ciSamples)
    {
        var ti = AddTableToCatalogue(databaseName, "Results", "ID", out var ciList);

        // setup join infos
        new JoinInfo(CatalogueRepository,ciList.Single(info => info.GetRuntimeName().Equals("SampleID")),
            ciSamples.Single(info => info.GetRuntimeName().Equals("ID")),
            ExtractionJoinType.Left, "");
        return ti;
    }

    private ITableInfo AddHeaderTableToCatalogue(string databaseName, ColumnInfo[] ciSamples)
    {
        var ti = AddTableToCatalogue(databaseName, "Header", "ID", out var ciList);

        // setup join infos
        new JoinInfo(CatalogueRepository,ciSamples.Single(info => info.GetRuntimeName().Equals("HeaderID")),
            ciList.Single(info => info.GetRuntimeName().Equals("ID")),
            ExtractionJoinType.Left, "");

        ti.IsPrimaryExtractionTable = true;
        ti.SaveToDatabase();

        return ti;
    }

    private ITableInfo AddTableToCatalogue(string databaseName, string tableName, string pkName, out ColumnInfo[] ciList, bool createCatalogue = false)
    {
        var table = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(databaseName).ExpectTable(tableName);
        var resultsImporter = new TableInfoImporter(CatalogueRepository, table);

        resultsImporter.DoImport(out var ti, out ciList);

        var pkResult = ciList.Single(info => info.GetRuntimeName().Equals(pkName));
        pkResult.IsPrimaryKey = true;
        pkResult.SaveToDatabase();

        var forwardEngineer = new ForwardEngineerCatalogue(ti, ciList);
        if (createCatalogue)
        {
            forwardEngineer.ExecuteForwardEngineering(out _catalogue, out _, out _);
        }
        else
            forwardEngineer.ExecuteForwardEngineering(_catalogue);

        return ti;
    }


}

internal class IdentityTableNamingScheme : INameDatabasesAndTablesDuringLoads
{
    public string GetDatabaseName(string rootDatabaseName, LoadBubble convention)
    {
        return rootDatabaseName;
    }

    public string GetName(string tableName, LoadBubble convention)
    {
        return tableName;
    }

    public static bool IsNamedCorrectly(string tableName, LoadBubble convention)
    {
        return true;
    }
}