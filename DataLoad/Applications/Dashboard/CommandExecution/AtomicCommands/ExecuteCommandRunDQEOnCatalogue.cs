using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.DataQualityUIs;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.Icons.IconProvision;

namespace Dashboard.CommandExecution.AtomicCommands
{
    public class ExecuteCommandRunDQEOnCatalogue:BasicCommandExecution,IAtomicCommandWithTarget
    {
        private readonly IActivateItems _itemActivator;
        private Catalogue _catalogue;

        public ExecuteCommandRunDQEOnCatalogue(IActivateItems itemActivator)
        {
            _itemActivator = itemActivator;
        }
        
        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.DQE, OverlayKind.Execute);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            _catalogue = (Catalogue) target;
            return this;
        }

        public override void Execute()
        {
            base.Execute();
            _itemActivator.Activate<DQEExecutionControl, Catalogue>(_catalogue);
        }
    }
}
