// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using FAnsi.Naming;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.QueryCaching.Aggregation;
using Rdmp.Core.QueryCaching.Aggregation.Arguments;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Tests.QueryCaching;

public class ExtractableAggregateCachingTests : QueryCachingDatabaseTests
{
    private Catalogue _cata;
    private AggregateConfiguration _config;
    private CachedAggregateConfigurationResultsManager _manager;
    private TableInfo _table;
    private ColumnInfo _columnInfo;
    private CatalogueItem _catalogueItem;
    private ExtractionInformation _extractionInformation;

    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();

        _cata =
            new Catalogue(CatalogueRepository, "ExtractableAggregateCachingTests");

        _table = new TableInfo(CatalogueRepository, "ExtractableAggregateCachingTests");
        _columnInfo = new ColumnInfo(CatalogueRepository, "Col1", "varchar(1000)", _table);

        _catalogueItem = new CatalogueItem(CatalogueRepository, _cata, "Col1");


        _extractionInformation = new ExtractionInformation(CatalogueRepository, _catalogueItem, _columnInfo, "Col1");


        _config
            =
            new AggregateConfiguration(CatalogueRepository, _cata, "ExtractableAggregateCachingTests");

        _manager = new CachedAggregateConfigurationResultsManager(QueryCachingDatabaseServer);
    }


    [Test]
    public void BasicCase()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            _manager.CommitResults(new CacheCommitExtractableAggregate(_config, "I've got a lovely bunch of coconuts",
                new DataTable(), 30)));


        Assert.IsTrue(ex.Message.StartsWith(
            "The DataTable that you claimed was an ExtractableAggregateResults had zero columns and therefore cannot be cached"));

        var dt = new DataTable();
        dt.Columns.Add("Col1");
        dt.Rows.Add("fishy!");

        var ex2 = Assert.Throws<NotSupportedException>(() =>
            _manager.CommitResults(
                new CacheCommitExtractableAggregate(_config, "I've got a lovely bunch of coconuts", dt, 30)));

        Assert.IsTrue(
            ex2.Message.StartsWith(
                "Aggregate ExtractableAggregateCachingTests is not marked as IsExtractable therefore cannot be cached"));


        _config.IsExtractable = true;
        _config.SaveToDatabase();


        //make the underlying column an is extraction identifier
        _extractionInformation.IsExtractionIdentifier = true;
        _extractionInformation.SaveToDatabase();

        var dim = new AggregateDimension(CatalogueRepository, _extractionInformation, _config);
        _config.ClearAllInjections();

        var ex3 = Assert.Throws<NotSupportedException>(() =>
            _manager.CommitResults(
                new CacheCommitExtractableAggregate(_config, "I've got a lovely bunch of coconuts", dt, 30)));

        Assert.IsTrue(
            ex3.Message.StartsWith(
                "Aggregate ExtractableAggregateCachingTests contains dimensions marked as IsExtractionIdentifier or HashOnDataRelease (Col1)"));

        _extractionInformation.IsExtractionIdentifier = false;
        _extractionInformation.SaveToDatabase();
        _config.ClearAllInjections();

        Assert.DoesNotThrow(() =>
            _manager.CommitResults(
                new CacheCommitExtractableAggregate(_config, "I've got a lovely bunch of coconuts", dt, 30)));

        dim.DeleteInDatabase();


        using (var con = DataAccessPortal
                   .ExpectServer(QueryCachingDatabaseServer, DataAccessContext.InternalDataProcessing).GetConnection())
        {
            var table = _manager.GetLatestResultsTableUnsafe(_config, AggregateOperation.ExtractableAggregateResults);

            con.Open();
            using (var cmd = DatabaseCommandHelper.GetCommand($"Select * from {table.GetFullyQualifiedName()}", con))
            using (var r = cmd.ExecuteReader())
            {
                Assert.IsTrue(r.Read());
                Assert.AreEqual("fishy!", r["Col1"]);
            }
        }
    }
}