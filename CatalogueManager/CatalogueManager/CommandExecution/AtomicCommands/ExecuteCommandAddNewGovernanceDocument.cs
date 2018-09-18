using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CatalogueLibrary.Data.Governance;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandAddNewGovernanceDocument : BasicUICommandExecution,IAtomicCommand
    {
        private readonly GovernancePeriod _period;
        private FileInfo _file;

        public ExecuteCommandAddNewGovernanceDocument(IActivateItems activator,GovernancePeriod period) : base(activator)
        {
            _period = period;
        }

        public ExecuteCommandAddNewGovernanceDocument(IActivateItems activator, GovernancePeriod period,FileInfo file): base(activator)
        {
            _period = period;
            _file = file;
        }

        public override void Execute()
        {
            base.Execute();

            if (_file == null)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                if(ofd.ShowDialog() == DialogResult.OK)
                    _file = new FileInfo(ofd.FileName);
            }

            if(_file == null)
                return;

            var doc = new GovernanceDocument(Activator.RepositoryLocator.CatalogueRepository, _period, _file);
            Publish(_period);
            Emphasise(doc);
            Activate(doc);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.GovernanceDocument, OverlayKind.Add);
        }
    }
}
