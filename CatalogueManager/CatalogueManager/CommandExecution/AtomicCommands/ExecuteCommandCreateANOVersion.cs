using System;
using System.Drawing;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.ANOEngineeringUIs;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateANOVersion:BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private Catalogue _catalogue;

        public ExecuteCommandCreateANOVersion(IActivateItems activator,Catalogue catalogue) : base(activator)
        {
            _catalogue = catalogue;
            UseTripleDotSuffix = true;
        }

        public ExecuteCommandCreateANOVersion(IActivateItems activator) : base(activator)
        {
            UseTripleDotSuffix = true;
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ANOTable);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            _catalogue = (Catalogue) target;
            return this;
        }

        public override string GetCommandHelp()
        {
            return "Create an anonymous version of the dataset.  This will be an initially empty anonymous schema and a load configuration for migrating the data.";
        }

        public override void Execute()
        {
            base.Execute();

            if (_catalogue == null)
                _catalogue = SelectOne<Catalogue>(Activator.CoreChildProvider.AllCatalogues);

            if(_catalogue == null)
                return;

            Activator.Activate<ForwardEngineerANOCatalogueUI, Catalogue>(_catalogue);
        }
    }
}
