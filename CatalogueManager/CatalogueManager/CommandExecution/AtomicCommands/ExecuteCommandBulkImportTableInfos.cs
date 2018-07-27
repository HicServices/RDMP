using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Data.ImportExport.Exceptions;
using CatalogueLibrary.Data.Serialization;
using CatalogueLibrary.DataHelper;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTable.Attributes;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandBulkImportTableInfos : BasicUICommandExecution,IAtomicCommand
    {
        private IExternalDatabaseServer _loggingServer;

        public ExecuteCommandBulkImportTableInfos(IActivateItems activator):base(activator)
        {

            var defaults = new ServerDefaults(Activator.RepositoryLocator.CatalogueRepository);
            _loggingServer = defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID);

            if(_loggingServer == null)
                SetImpossible("There is no default logging server configured");
        }

        public override void Execute()
        {
            base.Execute();

            var db = SelectDatabase("Import all Tables form Database...");

            ShareManager shareManager = new ShareManager(Activator.RepositoryLocator,LocalReferenceGetter);

            List<ICatalogue> catalogues = new List<ICatalogue>();

            //don't do any double importing!
            var existing = Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<TableInfo>();
            var ignoredTables = new List<TableInfo>();

            if (MessageBox.Show("Would you also like to import ShareDefinitions (metadata)?", "Import Metadata From File(s)", MessageBoxButtons.YesNo) == DialogResult.Yes)
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
            
            if(!catalogues.Any())
                if(MessageBox.Show("You have not imported any Share Definitions for Catalogues, would you like to try to guess Catalogues by Name?","Guess by name",MessageBoxButtons.YesNo) == DialogResult.Yes)
                    catalogues.AddRange(Activator.RepositoryLocator.CatalogueRepository.GetAllCatalogues());

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
            }

            if (married.Any() &&MessageBox.Show("Found " + married.Count + " columns, make them all extractable?", "Make Extractable", MessageBoxButtons.YesNo) == DialogResult.Yes)
                foreach (var kvp in married)
                {
                    //yup thats how we roll, the database is main memory!
                    var ei = new ExtractionInformation(Activator.RepositoryLocator.CatalogueRepository, kvp.Key, kvp.Value, kvp.Value.Name);
                }

            if(ignoredTables.Any())
                WideMessageBox.Show("Ignored " + ignoredTables.Count + " tables because they already existed as TableInfos:" + string.Join(Environment.NewLine,ignoredTables.Select(ti=>ti.GetRuntimeName())));

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


        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Database, OverlayKind.Import);
        }
    }
}