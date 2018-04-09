using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueManager.ANOEngineeringUIs;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateANOVersion:BasicUICommandExecution,IAtomicCommand
    {
        private readonly Catalogue _catalogue;

        public ExecuteCommandCreateANOVersion(IActivateItems activator,Catalogue catalogue) : base(activator)
        {
            _catalogue = catalogue;
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ANOTable);
        }

        public override void Execute()
        {
            base.Execute();

            Activator.Activate<ForwardEngineerANOCatalogueUI, Catalogue>(_catalogue);
        }
    }
}
