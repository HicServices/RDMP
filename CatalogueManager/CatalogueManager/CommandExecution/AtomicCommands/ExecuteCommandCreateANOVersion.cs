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
using ReusableUIComponents.CommandExecution.AtomicCommands;
using ReusableUIComponents.Icons.IconProvision;

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
            return iconProvider.GetImage(RDMPConcept.ANOTable,OverlayKind.Execute);
        }

        public override void Execute()
        {
            base.Execute();

            Activator.Activate<ForwardEngineerANOVersionOfCatalogueUI, Catalogue>(_catalogue);
        }
    }
}
