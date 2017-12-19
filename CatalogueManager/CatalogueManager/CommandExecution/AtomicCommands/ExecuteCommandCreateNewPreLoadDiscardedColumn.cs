using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewPreLoadDiscardedColumn:BasicUICommandExecution,IAtomicCommand
    {
        private readonly TableInfo _tableInfo;
        private ColumnInfo[] _prototypes;

        public ExecuteCommandCreateNewPreLoadDiscardedColumn(IActivateItems activator,TableInfo tableInfo) : base(activator)
        {
            _tableInfo = tableInfo;
        }

        public ExecuteCommandCreateNewPreLoadDiscardedColumn(IActivateItems activator, TableInfo tableInfo, ColumnInfoCommand sourceColumnInfoCommand):this(activator,tableInfo)
        {
            _prototypes = sourceColumnInfoCommand.ColumnInfos;

            var existing = tableInfo.PreLoadDiscardedColumns;
            foreach (ColumnInfo prototype in _prototypes)
            {
                var alreadyExists = existing.Any(c => c.GetRuntimeName().Equals(prototype.GetRuntimeName()));

                if (alreadyExists)
                    SetImpossible("There is already a PreLoadDiscardedColumn called '" + prototype.GetRuntimeName() + "'");
            }
          
        }

        public override void Execute()
        {
            base.Execute();

            string name = null;
            string dataType = null;

            if(_prototypes == null)
            {

                var textDialog = new TypeTextOrCancelDialog("Column Name","Enter name for column (this should NOT include any qualifiers e.g. database name)", 300);
                if (textDialog.ShowDialog() == DialogResult.OK)
                    name = textDialog.ResultText;
                else
                    return;

                textDialog = new TypeTextOrCancelDialog("Column DataType", "Enter data type for column (e.g. 'varchar(10)')", 300);
                if (textDialog.ShowDialog() == DialogResult.OK)
                    dataType = textDialog.ResultText;
                else
                    return;

                Create(name, dataType);
                Publish();

            }
            else
            {
                foreach (ColumnInfo prototype in _prototypes)
                    Create(prototype.GetRuntimeName(), prototype.Data_type);

                Publish();
            }
        }

        private void Publish()
        {
            Publish(_tableInfo);
        }

        private void Create(string name, string dataType)
        {
            var discCol = new PreLoadDiscardedColumn(Activator.RepositoryLocator.CatalogueRepository, _tableInfo, name);
            discCol.SqlDataType = dataType;
            discCol.SaveToDatabase();
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
