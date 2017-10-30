using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using DataExportManager.Collections.Providers;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using ReusableUIComponents.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandChangeExtractability:BasicUICommandExecution,IAtomicCommand
    {
        private readonly Catalogue _catalogue;
        private bool _isExtractable;

        public ExecuteCommandChangeExtractability(IActivateItems activator, Catalogue catalogue) : base(activator)
        {
            _catalogue = catalogue;
            if(!catalogue.GetIsExtractabilityKnown())
            {
                SetImpossible("We don't know whether Catalogue is extractable or not (possibly no Data Export database is available)");
                return;
            }

            _isExtractable = catalogue.GetIsExtractable();

        }

        public override string GetCommandName()
        {
            return _isExtractable?"Mark Not Extractable":"Mark Extractable";
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ExtractableDataSet, _isExtractable?OverlayKind.Delete:OverlayKind.Add);
        }

        public override void Execute()
        {
            base.Execute();

            if (_isExtractable)
            {
                var extractabilityRecord = ((DataExportChildProvider) Activator.CoreChildProvider).ExtractableDataSets.Single(ds => ds.Catalogue_ID == _catalogue.ID);
                extractabilityRecord.DeleteInDatabase();
                Publish(_catalogue);
            }
            else
            {
                 new ExtractableDataSet(Activator.RepositoryLocator.DataExportRepository, _catalogue);
                Publish(_catalogue);

            }
        }
    }
}
