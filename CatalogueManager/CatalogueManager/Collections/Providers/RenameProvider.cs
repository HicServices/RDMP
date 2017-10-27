using System;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Menus;
using CatalogueManager.Refreshing;
using MapsDirectlyToDatabaseTable;
using ReusableUIComponents;

namespace CatalogueManager.Collections.Providers
{
    public class RenameProvider
    {
        private readonly RefreshBus _refreshBus;
        private readonly ObjectListView _olv;
        private readonly OLVColumn _columnThatSupportsRenaming;
        private readonly Label _lblAdviceAboutRenaming;
        
        public bool AllowRenaming {  
            get
            {
                return _columnThatSupportsRenaming.IsEditable;
            }
            set
            {
                _olv.CellEditActivation = value ? ObjectListView.CellEditActivateMode.SingleClick : ObjectListView.CellEditActivateMode.None;
                _columnThatSupportsRenaming.IsEditable = value;
                _lblAdviceAboutRenaming.Visible = value;
            } }

        public RenameProvider(RefreshBus refreshBus, ObjectListView olv, OLVColumn columnThatSupportsRenaming, Label lblAdviceAboutRenaming)
        {
            _refreshBus = refreshBus;
            _olv = olv;
            _columnThatSupportsRenaming = columnThatSupportsRenaming;
            _lblAdviceAboutRenaming = lblAdviceAboutRenaming;
        }

        public void RegisterEvents()
        {
            _olv.CellEditStarting += OlvOnCellEditStarting;
            _olv.CellEditFinishing += OlvOnCellEditFinishing;

            _columnThatSupportsRenaming.CellEditUseWholeCell = true;
            _columnThatSupportsRenaming.AutoCompleteEditorMode = AutoCompleteMode.None;

            AllowRenaming = true;

        }
        private void OlvOnCellEditStarting(object sender, CellEditEventArgs e)
        {
            if (!(e.RowObject is INamed))
                e.Cancel = true;
        }

        void OlvOnCellEditFinishing(object sender, CellEditEventArgs e)
        {
            if(e.RowObject == null)
                return;
            
            if(e.Column != _columnThatSupportsRenaming)
                return;

            //don't let them rename things to blank names
            if (string.IsNullOrWhiteSpace((string) e.NewValue))
            {
                e.Cancel = true;
                return;
            }

            var name = e.RowObject as INamed;

            try
            {
                if (name != null)
                    new ExecuteCommandRename(_refreshBus, name, (string) e.NewValue).Execute();
            }
            catch (Exception exception)
            {
                e.Cancel = true;
                ExceptionViewer.Show(exception);

                //reset it to what it was before
                if (name != null)
                    name.Name = (string)e.Value;
            }
        }

    }
}
