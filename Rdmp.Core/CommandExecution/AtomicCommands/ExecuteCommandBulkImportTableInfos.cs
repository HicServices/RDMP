// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
/// Import references to many tables at once from a database as <see cref="TableInfo"/>.  Optionally importing descriptive metadata for them from <see cref="ShareDefinition"/> files
/// </summary>
public sealed class ExecuteCommandBulkImportTableInfos : BasicCommandExecution
{
    private readonly IExternalDatabaseServer _loggingServer;

    public ExecuteCommandBulkImportTableInfos(IBasicActivateItems activator) : base(activator)
    {
        _loggingServer = activator.ServerDefaults.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);

        if (_loggingServer == null)
            SetImpossible("There is no default logging server configured");

        UseTripleDotSuffix = true;
    }

    public override void Execute()
    {
        base.Execute();

        var db = SelectDatabase(false, "Import all Tables from Database...");

        if (db == null)
            return;


        var shareManager = new ShareManager(BasicActivator.RepositoryLocator, LocalReferenceGetter);

        var catalogues = new List<ICatalogue>();

        //don't do any double importing!
        var existing = BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<TableInfo>();

        if (YesNo("Would you also like to import ShareDefinitions (metadata)?", "Import Metadata From File(s)"))
        {
            var chosen = BasicActivator.SelectFiles("Share Definition Files", "Share Definitions", "*.sd");

            if (chosen != null)
                foreach (var f in chosen)
                {
                    using var stream = File.Open(f.FullName, FileMode.Open);
                    var newObjects = shareManager.ImportSharedObject(stream);

                    if (newObjects != null)
                        catalogues.AddRange(newObjects.OfType<ICatalogue>());
                }
        }

        var generateCatalogues = false;

        if (YesNo("Would you like to try to guess non-matching Catalogues by Name?", "Guess by name"))
            catalogues.AddRange(BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>());
        else if (YesNo("Would you like to generate empty Catalogues for non-matching tables instead?",
                     "Generate New Catalogues"))
            generateCatalogues = true;

        var married = new Dictionary<CatalogueItem, ColumnInfo>();

        ITableInfo anyNewTable = null;

        if (!BasicActivator.SelectObjects("Import", db.DiscoverTables(false)
                .Select(discoveredTable =>
                    new { discoveredTable, collide = existing.FirstOrDefault(t => t.Is(discoveredTable)) })
                .Where(static t1 => t1.collide == null)
                .Select(static t1 => t1.discoveredTable).ToArray(), out var selected))
            return;

        foreach (var discoveredTable in selected)
        {
            var importer = new TableInfoImporter(BasicActivator.RepositoryLocator.CatalogueRepository, discoveredTable);

            //import the table
            importer.DoImport(out var ti, out var cis);

            anyNewTable ??= ti;

            //find a Catalogue of the same name (possibly imported from Share Definition)
            var matchingCatalogues = catalogues
                .Where(c => c.Name.Equals(ti.GetRuntimeName(), StringComparison.CurrentCultureIgnoreCase)).ToArray();

            //if there's 1 Catalogue with the same name
            if (matchingCatalogues.Length == 1)
            {
                //we know we want to import all these ColumnInfos
                var unmatched = new List<ColumnInfo>(cis);

                //But hopefully most already have orphan CatalogueItems we can hook them together to
                foreach (var cataItem in matchingCatalogues[0].CatalogueItems)
                    if (cataItem.ColumnInfo_ID == null)
                    {
                        var matches = cataItem.GuessAssociatedColumn(cis, false).ToArray();

                        if (matches.Length == 1)
                        {
                            cataItem.SetColumnInfo(matches[0]);
                            unmatched.Remove(matches[0]); //we married them together
                            married.Add(cataItem, matches[0]);
                        }
                    }

                //is anyone unmarried? i.e. new ColumnInfos that don't have CatalogueItems with the same name
                foreach (var columnInfo in unmatched)
                {
                    var cataItem = new CatalogueItem(BasicActivator.RepositoryLocator.CatalogueRepository,
                        (Catalogue)matchingCatalogues[0], columnInfo.GetRuntimeName())
                    {
                        ColumnInfo_ID = columnInfo.ID
                    };
                    cataItem.SaveToDatabase();
                    married.Add(cataItem, columnInfo);
                }
            }
            else if (generateCatalogues)
            {
                new ForwardEngineerCatalogue(ti, cis).ExecuteForwardEngineering();
            }
        }

        if (married.Any() && YesNo($"Found {married.Count} columns, make them all extractable?", "Make Extractable"))
            foreach (var (catalogueItem, columnInfo) in married)
                // don't mark it extractable twice
                if (catalogueItem.ExtractionInformation == null)
                    //yup that's how we roll, the database is main memory!
                    _ = new ExtractionInformation(BasicActivator.RepositoryLocator.CatalogueRepository, catalogueItem,
                        columnInfo, columnInfo.Name);

        if (anyNewTable != null)
        {
            Publish(anyNewTable);
            Emphasise(anyNewTable);
        }
    }

    private int? LocalReferenceGetter(PropertyInfo property, RelationshipAttribute relationshipAttribute,
        ShareDefinition shareDefinition) =>
        property.Name.EndsWith("LoggingServer_ID", StringComparison.Ordinal)
            ? _loggingServer.ID
            : throw new SharingException($"Could not figure out a sensible value to assign to Property {property}");


    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.Database, OverlayKind.Import);
}