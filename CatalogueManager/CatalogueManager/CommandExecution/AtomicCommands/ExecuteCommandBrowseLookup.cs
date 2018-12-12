using System.Drawing;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueManager.ExtractionUIs.JoinsAndLookups;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandBrowseLookup : BasicUICommandExecution,IAtomicCommand
    {
        private Lookup _lookup;

        public ExecuteCommandBrowseLookup(IActivateItems activator, Lookup lookup):base(activator)
        {
            _lookup = lookup;
        }

        public ExecuteCommandBrowseLookup(IActivateItems activator, IFilter filter) : base(activator)
        {
            var colInfo = filter.GetColumnInfoIfExists();

            if (colInfo != null)
                _lookup = 
                    colInfo.GetAllLookupForColumnInfoWhereItIsA(LookupType.AnyKey).FirstOrDefault() ??
                    colInfo.GetAllLookupForColumnInfoWhereItIsA(LookupType.Description).FirstOrDefault();

            if (_lookup == null)
                SetImpossible("No Lookups found");
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Lookup,OverlayKind.Shortcut);
        }

        public override void Execute()
        {
            base.Execute();
            
            Activator.Activate<LookupBrowserUI, Lookup>(_lookup);
        }
    }
}