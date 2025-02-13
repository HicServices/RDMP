// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.QueryCaching.Aggregation;
using Rdmp.Core.QueryCaching.Aggregation.Arguments;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Tests.QueryCaching;

public class CachedAggregateConfigurationResultsManagerTests : QueryCachingDatabaseTests
{
    private Catalogue _cata;
    private AggregateConfiguration _config;
    private CachedAggregateConfigurationResultsManager _manager;
    private DatabaseColumnRequest _myColSpecification = new("MyCol", "varchar(10)");


    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();

        _cata =
            new Catalogue(CatalogueRepository, "CachedAggregateConfigurationResultsManagerTests");

        _config
            =
            new AggregateConfiguration(CatalogueRepository, _cata, "CachedAggregateConfigurationResultsManagerTests");

        _manager = new CachedAggregateConfigurationResultsManager(QueryCachingDatabaseServer);
    }


    [Test]
    public void CommitResults_CreatesTablessuccessfully()
    {
        var dt = new DataTable();
        dt.Columns.Add("MyCol");

        dt.Rows.Add("0101010101");
        dt.Rows.Add("0201010101");
        dt.Rows.Add("0101310101");

        //commit it 3 times, should just overwrite
        _manager.CommitResults(new CacheCommitIdentifierList(_config, SomeComplexBitOfSqlCode, dt, _myColSpecification,
            30));
        _manager.CommitResults(new CacheCommitIdentifierList(_config, SomeComplexBitOfSqlCode, dt, _myColSpecification,
            30));
        _manager.CommitResults(new CacheCommitIdentifierList(_config, SomeComplexBitOfSqlCode, dt, _myColSpecification,
            30));

        var resultsTableName =
            _manager.GetLatestResultsTableUnsafe(_config, AggregateOperation.IndexedExtractionIdentifierList);


        Assert.That(resultsTableName.GetRuntimeName(), Is.EqualTo($"IndexedExtractionIdentifierList_AggregateConfiguration{_config.ID}"));

        var table = DataAccessPortal
            .ExpectDatabase(QueryCachingDatabaseServer, DataAccessContext.InternalDataProcessing)
            .ExpectTable(resultsTableName.GetRuntimeName());

        Assert.That(table.Exists());
        var col = table.DiscoverColumn("MyCol");

        Assert.That(col, Is.Not.Null);
        Assert.That(col.DataType.SQLType, Is.EqualTo("varchar(10)"));

        using (var con = DataAccessPortal
                   .ExpectServer(QueryCachingDatabaseServer, DataAccessContext.InternalDataProcessing).GetConnection())
        {
            con.Open();

            var dt2 = new DataTable();
            var da = new SqlDataAdapter($"Select * from {resultsTableName.GetFullyQualifiedName()}",
                (SqlConnection)con);
            da.Fill(dt2);

            Assert.That(dt2.Rows, Has.Count.EqualTo(dt.Rows.Count));

            con.Close();
        }

        Assert.Multiple(() =>
        {
            Assert.That(_manager.GetLatestResultsTable(_config, AggregateOperation.IndexedExtractionIdentifierList,
                    SomeComplexBitOfSqlCode), Is.Not.Null);
            Assert.That(_manager.GetLatestResultsTable(_config, AggregateOperation.IndexedExtractionIdentifierList,
                "select name,height,scalecount from fish"), Is.Null);
        });
    }


    [Test]
    public void Throws_BecauseItHasDuplicates()
    {
        var dt = new DataTable();
        dt.Columns.Add("MyCol");
        dt.Rows.Add("0101010101");
        dt.Rows.Add("0101010101");

        var ex = Assert.Throws<Exception>(() =>
            _manager.CommitResults(new CacheCommitIdentifierList(_config, "select * from fish", dt, _myColSpecification,
                30)));
        Assert.That(ex.Message, Does.Contain("primary key"));
    }

    [Test]
    public void Throws_BecauseInceptionCaching()
    {
        var dt = new DataTable();
        dt.Columns.Add("MyCol");
        dt.Rows.Add("0101010101");


        //If this unit test suddenly starts failing you might have changed the value of CachedAggregateConfigurationResultsManager.CachingPrefix (see sql variable below and make it match the const - the unit test is divorced because why would you want to change that eh!, 'Cached:' is very clear)

        //this is a cache fetch request that we are trying to inception recache
        const string sql = """
                           /*Cached:cic_65_People in DMPTestCatalogue*/
                           	select * from [cache]..[IndexedExtractionIdentifierList_AggregateConfiguration217]
                           """;

        var ex = Assert.Throws<NotSupportedException>(() =>
            _manager.CommitResults(new CacheCommitIdentifierList(_config, sql, dt, _myColSpecification, 30)));
        Assert.That(ex?.Message, Does.Contain("This is referred to as Inception Caching and isn't allowed"));
    }

    [Test]
    public void NullsAreDropped()
    {
        var dt = new DataTable();
        dt.Columns.Add("MyCol");
        dt.Rows.Add("0101010101");
        dt.Rows.Add(DBNull.Value);
        dt.Rows.Add("0101010102");

        _manager.CommitResults(
            new CacheCommitIdentifierList(_config, "select * from fish", dt, _myColSpecification, 30));

        var resultTable = _manager.GetLatestResultsTable(_config, AggregateOperation.IndexedExtractionIdentifierList,
            "select * from fish");

        var dt2 = new DataTable();

        using (var con = DataAccessPortal
                   .ExpectServer(QueryCachingDatabaseServer, DataAccessContext.InternalDataProcessing).GetConnection())
        {
            con.Open();

            var da = new SqlDataAdapter($"Select * from {resultTable.GetFullyQualifiedName()}",
                (SqlConnection)con);
            da.Fill(dt2);
        }

        Assert.That(dt2.Rows, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(dt2.Rows.Cast<DataRow>().Any(r => (string)r[0] == "0101010101"));
            Assert.That(dt2.Rows.Cast<DataRow>().Any(r => (string)r[0] == "0101010102"));
        });
    }

    private const string SomeComplexBitOfSqlCode =
        @"USE [QueryCachingDatabase]
GO

/****** Object:  Table [dbo].[CachedAggregateConfigurationResults]    Script Date: 24/03/2016 16:30:18 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[CachedAggregateConfigurationResults](
	[Committer] [varchar](500) NOT NULL,
	[Date] [datetime] NOT NULL,
	[AggregateConfiguration_ID] [nchar](10) NOT NULL,
	[SqlExecuted] [varchar](max) NOT NULL,
	[Operation] [varchar](50) NOT NULL,
	[TableName] [varchar](500) NOT NULL,
 CONSTRAINT [PK_CachedAggregateConfigurationResults] PRIMARY KEY CLUSTERED 
(
	[AggregateConfiguration_ID] ASC,
	[Operation] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

ALTER TABLE [dbo].[CachedAggregateConfigurationResults] ADD  CONSTRAINT [DF_CachedAggregateConfigurationResults_Date]  DEFAULT (getdate()) FOR [Date]
GO


";
}