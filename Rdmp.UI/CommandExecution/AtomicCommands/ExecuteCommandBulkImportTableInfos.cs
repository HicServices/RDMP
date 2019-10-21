// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Icons.IconProvision;

using WideMessageBox = Rdmp.UI.SimpleDialogs.WideMessageBox;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandBulkImportTableInfos : BasicUICommandExecution,IAtomicCommand
    {
        private IExternalDatabaseServer _loggingServer;

        public ExecuteCommandBulkImportTableInfos(IActivateItems activator):base(activator)
        {
            _loggingServer = Activator.ServerDefaults.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);

            if(_loggingServer == null)
                SetImpossible("There is no default logging server configured");

            UseTripleDotSuffix = true;
        }

        public override void Execute()
        {
            base.Execute();

            var db = SelectDatabase(false,"Import all Tables form Database...");

            if(db == null)
                return;


            ShareManager shareManager = new ShareManager(Activator.RepositoryLocator,LocalReferenceGetter);

            List<ICatalogue> catalogues = new List<ICatalogue>();

            //don't do any double importing!
            var existing = Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<TableInfo>();
            var ignoredTables = new List<TableInfo>();

            if (YesNo("Would you also like to import ShareDefinitions (metadata)?", "Import Metadata From File(s)"))
            {
                OpenFileDialog ofd = new OpenFileDialog() { Multiselect = true };
                ofd.Filter = "Share Definitions|*.sd";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    foreach (var f in ofd.FileNames)
                        using (var stream = File.Open(f, FileMode.Open))
                        {
                            var newObjects = shareManager.ImportSharedObject(stream);

                            if(newObjects != null)
                                catalogues.AddRange(newObjects.OfType<ICatalogue>());
                        }
                }
            }

            bool generateCatalogues = false;

            if (YesNo("Would you like to try to guess non-matching Catalogues by Name?", "Guess by name"))
                catalogues.AddRange(Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>());
            else if(YesNo("Would you like to generate empty Catalogues for non-matching tables instead?","Generate New Catalogues"))
                generateCatalogues = true;
            
            var married = new Dictionary<CatalogueItem, ColumnInfo>();

            TableInfo anyNewTable = null;

            foreach (DiscoveredTable discoveredTable in db.DiscoverTables(includeViews: false))
            {
                var collide = existing.FirstOrDefault(t => t.Is(discoveredTable));
                if (collide != null)
                {
                    ignoredTables.Add(collide);
                    continue;
                }

                var importer = new TableInfoImporter(Activator.RepositoryLocator.CatalogueRepository, discoveredTable);
                TableInfo ti;
                ColumnInfo[] cis;

                //import the table
                importer.DoImport(out ti, out cis);

                anyNewTable = anyNewTable ?? ti;
                     
                //find a Catalogue of the same name (possibly imported from Share Definition)
                var matchingCatalogues = catalogues.Where(c => c.Name.Equals(ti.GetRuntimeName(), StringComparison.CurrentCultureIgnoreCase)).ToArray();

                //if theres 1 Catalogue with the same name
                if (matchingCatalogues.Length == 1)
                {
                    //we know we want to import all these ColumnInfos
                    var unmatched = new List<ColumnInfo>(cis);
                    
                    //But hopefully most already have orphan CatalogueItems we can hook them together to
                    foreach (var cataItem in matchingCatalogues[0].CatalogueItems)
                        if (cataItem.ColumnInfo_ID == null)
                        {
                            var matches = cataItem.GuessAssociatedColumn(cis, allowPartial: false).ToArray();

                            if (matches.Length == 1)
                            {   
                                cataItem.SetColumnInfo(matches[0]);
                                unmatched.Remove(matches[0]); //we married them together
                                married.Add(cataItem,matches[0]);
                            }
                        }

                    //is anyone unmarried? i.e. new ColumnInfos that don't have CatalogueItems with the same name
                    foreach (ColumnInfo columnInfo in unmatched)
                    {
                        var cataItem = new CatalogueItem(Activator.RepositoryLocator.CatalogueRepository, (Catalogue)matchingCatalogues[0], columnInfo.GetRuntimeName());
                        cataItem.ColumnInfo_ID = columnInfo.ID;
                        cataItem.SaveToDatabase();
                        married.Add(cataItem,columnInfo);
                    }
                }
                else if (generateCatalogues)
                    new ForwardEngineerCatalogue(ti, cis).ExecuteForwardEngineering();
            }

            if (married.Any() && YesNo("Found " + married.Count + " columns, make them all extractable?", "Make Extractable"))
                foreach (var kvp in married)
                {
                    //yup thats how we roll, the database is main memory!
                    var ei = new ExtractionInformation(Activator.RepositoryLocator.CatalogueRepository, kvp.Key, kvp.Value, kvp.Value.Name);
                }

            if(ignoredTables.Any())
                WideMessageBox.Show("Ignored some tables","Ignored " + ignoredTables.Count + " tables because they already existed as TableInfos:" + string.Join(Environment.NewLine,ignoredTables.Select(ti=>ti.GetRuntimeName())));

            if (anyNewTable != null)
            {
                Publish(anyNewTable);
                Emphasise(anyNewTable);
            }
        }

        private int? LocalReferenceGetter(PropertyInfo property, RelationshipAttribute relationshipattribute, ShareDefinition sharedefinition)
        {
            if (property.Name.EndsWith("LoggingServer_ID"))
                return _loggingServer.ID;


            throw new SharingException("Could not figure out a sensible value to assign to Property " + property);
        }


        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Database, OverlayKind.Import);
        }
    }
}