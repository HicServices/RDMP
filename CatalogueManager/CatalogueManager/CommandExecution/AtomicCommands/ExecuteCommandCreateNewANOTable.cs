using System.Drawing;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableUIComponents;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewANOTable : BasicUICommandExecution,IAtomicCommand
    {
        private IExternalDatabaseServer _anoStoreServer;

        public ExecuteCommandCreateNewANOTable(IActivateItems activator) : base(activator)
        {
            ServerDefaults defaults = new ServerDefaults(activator.RepositoryLocator.CatalogueRepository);
            _anoStoreServer = defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.ANOStore);

            if(_anoStoreServer == null)
                SetImpossible("No default ANOStore has been set");
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ANOTable, OverlayKind.Add);
        }

        public override void Execute()
        {
            base.Execute();

            var name = new TypeTextOrCancelDialog("ANO Concept Name", "Name", 500,"ANOConceptName");
            if (name.ShowDialog() == DialogResult.OK)
            {
                var suffix = new TypeTextOrCancelDialog("Type Concept Suffix", "Suffix", 5, "_X");
                if (suffix.ShowDialog() == DialogResult.OK)
                {
                    var n = name.ResultText;

                    if(!n.StartsWith("ANO"))
                        n = "ANO" + n;

                    var s = suffix.ResultText.Trim('_');

                    var anoTable = new ANOTable(Activator.RepositoryLocator.CatalogueRepository, (ExternalDatabaseServer) _anoStoreServer,n,s);
                    Publish(anoTable);
                    Activate(anoTable);
                }
            }
        }
    }
}