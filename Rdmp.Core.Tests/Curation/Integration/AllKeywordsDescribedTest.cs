// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration;

[TestFixture]
public class AllKeywordsDescribedTest : DatabaseTests
{
    [OneTimeSetUp]
    protected override void OneTimeSetUp()
    {
        base.OneTimeSetUp();

        CatalogueRepository.CommentStore.ReadComments(TestContext.CurrentContext.TestDirectory);
    }

    [Test]
    public void AllTablesDescribed()
    {
        //ensures the DQERepository gets a chance to add its help text
        new DQERepository(CatalogueRepository);

        var databaseTypes = typeof(Catalogue).Assembly.GetTypes().Where(t =>
            typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract &&
            !t.Name.StartsWith("Spontaneous") && !t.Name.Contains("Proxy")).ToArray();

        var problems = databaseTypes
            .Select(type => new
            {
                type,
                docs = CatalogueRepository.CommentStore[type.Name] ??
                       CatalogueRepository.CommentStore[$"I{type.Name}"]
            })
            .Where(t => string.IsNullOrWhiteSpace(t.docs))
            .Select(t =>
                $"Type {t.type.Name} does not have an entry in the help dictionary (maybe the class doesn't have documentation? - try adding /// <summary> style comments to the class)")
            .ToList();
        foreach (var problem in problems)
            Console.WriteLine($"Fatal Problem:{problem}");

        Assert.That(problems, Is.Empty);
    }

    [Test]
    public void AllForeignKeysDescribed()
    {
        var allKeys = new List<string>();

        //ensures the DQERepository gets a chance to add its help text
        new DQERepository(CatalogueRepository);

        allKeys.AddRange(GetForeignKeys(CatalogueTableRepository.DiscoveredServer));
        allKeys.AddRange(GetForeignKeys(DataExportTableRepository.DiscoveredServer));
        allKeys.AddRange(GetForeignKeys(new DiscoveredServer(DataQualityEngineConnectionString)));

        var problems = allKeys.Where(fkName => !CatalogueRepository.CommentStore.ContainsKey(fkName))
            .Select(fkName => $"{fkName} is a foreign Key (which does not CASCADE) but does not have any HelpText")
            .ToList();

        foreach (var problem in problems)
            Console.WriteLine($"Fatal Problem:{problem}");

        Assert.That(problems, Is.Empty, @"Add a description for each of these to KeywordHelp.txt");
    }

    [Test]
    public void AllUserIndexesDescribed()
    {
        var allIndexes = new List<string>();

        //ensures the DQERepository gets a chance to add its help text
        new DQERepository(CatalogueRepository);

        allIndexes.AddRange(GetIndexes(CatalogueTableRepository.DiscoveredServer));
        allIndexes.AddRange(GetIndexes(DataExportTableRepository.DiscoveredServer));
        allIndexes.AddRange(GetIndexes(new DiscoveredServer(DataQualityEngineConnectionString)));

        var problems = new List<string>();
        foreach (var idx in allIndexes)
            if (!CatalogueRepository.CommentStore.ContainsKey(idx))
                problems.Add($"{idx} is an index but does not have any HelpText");

        foreach (var problem in problems)
            Console.WriteLine($"Fatal Problem:{problem}");

        Assert.That(problems, Is.Empty, @"Add a description for each of these to KeywordHelp.txt");
    }

    private static IEnumerable<string> GetForeignKeys(DiscoveredServer server)
    {
        using var con = server.GetConnection();
        con.Open();
        var r = server.GetCommand(@"select name from sys.foreign_keys where delete_referential_action = 0", con)
            .ExecuteReader();

        while (r.Read())
            yield return (string)r["name"];
    }

    private static IEnumerable<string> GetIndexes(DiscoveredServer server)
    {
        using var con = server.GetConnection();
        con.Open();
        var r = server.GetCommand(@"select si.name from sys.indexes si 
  JOIN sys.objects so ON si.[object_id] = so.[object_id]
  WHERE
  so.type = 'U'  AND is_primary_key = 0
  and si.name is not null
and so.name <> 'sysdiagrams'", con).ExecuteReader();

        while (r.Read())
            yield return (string)r["name"];
    }
}