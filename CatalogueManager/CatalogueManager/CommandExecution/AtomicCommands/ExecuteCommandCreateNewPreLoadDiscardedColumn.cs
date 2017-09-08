using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using ReusableUIComponents;
using ReusableUIComponents.Copying;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewPreLoadDiscardedColumn:BasicCommandExecution,IAtomicCommand
    {
        private readonly IActivateItems _activator;
        private readonly TableInfo _tableInfo;

        public ExecuteCommandCreateNewPreLoadDiscardedColumn(IActivateItems activator,TableInfo tableInfo)
        {
            _activator = activator;
            _tableInfo = tableInfo;
        }

        public override void Execute()
        {
            base.Execute();
            
            var textDialog = new TypeTextOrCancelDialog("Column Name","Enter name for column (this should NOT include any qualifiers e.g. database name)", 300);
            if(textDialog.ShowDialog() == DialogResult.OK)
            {
                new PreLoadDiscardedColumn(_activator.RepositoryLocator.CatalogueRepository, _tableInfo,textDialog.ResultText);
                _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_tableInfo));
            }
        }

        public override string GetCommandName()
        {
            return "Add New Load Discarded Column";
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.PreLoadDiscardedColumn, OverlayKind.Add);
        }
    }
}
