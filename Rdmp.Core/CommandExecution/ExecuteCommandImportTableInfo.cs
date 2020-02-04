using System;
using System.Collections.Generic;
using System.Text;
using FAnsi.Discovery;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.CommandExecution
{
    class ExecuteCommandImportTableInfo : BasicCommandExecution
    {
        private readonly DiscoveredTable _table;
        private readonly bool _createCatalogue;

        public ExecuteCommandImportTableInfo(IBasicActivateItems activator, 
            
            [DemandsInitialization("The table or view you want to reference from RDMP.  See PickTable for syntax")]
            DiscoveredTable table, 
            [DemandsInitialization("True to create a Catalogue as well as a TableInfo")]
            bool createCatalogue): base(activator)
        {
            _table = table;
            _createCatalogue = createCatalogue;
        }

        public override void Execute()
        {
            base.Execute();

            Catalogue c = null;

            var importer = new TableInfoImporter(BasicActivator.RepositoryLocator.CatalogueRepository, _table);
            importer.DoImport(out TableInfo ti, out ColumnInfo[] cis);
            
            BasicActivator.Show($"Successfully imported new TableInfo { ti.Name} with ID {ti.ID}");

            if (_createCatalogue)
            {
                var forwardEngineer = new ForwardEngineerCatalogue(ti, cis, true);
                forwardEngineer.ExecuteForwardEngineering(out c,out _,out _);

                BasicActivator.Show($"Successfully imported new Catalogue { c.Name} with ID {c.ID}");
            }
                
            Publish((DatabaseEntity) c ?? ti);
        }
    }
}
