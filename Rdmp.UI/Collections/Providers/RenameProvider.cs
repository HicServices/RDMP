// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Refreshing;
using MapsDirectlyToDatabaseTable;
using ReusableUIComponents.Dialogs;

namespace CatalogueManager.Collections.Providers
{
    public class RenameProvider
    {
        private readonly RefreshBus _refreshBus;
        private readonly ObjectListView _olv;
        private readonly OLVColumn _columnThatSupportsRenaming;
        
        public bool AllowRenaming {  
            get
            {
                return _columnThatSupportsRenaming.IsEditable;
            }
            set
            {
                _olv.CellEditActivation = value ? ObjectListView.CellEditActivateMode.SingleClick : ObjectListView.CellEditActivateMode.None;
                _columnThatSupportsRenaming.IsEditable = value;
            } }

        public RenameProvider(RefreshBus refreshBus, ObjectListView olv, OLVColumn columnThatSupportsRenaming)
        {
            _refreshBus = refreshBus;
            _olv = olv;
            _columnThatSupportsRenaming = columnThatSupportsRenaming;
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
            //it's not for our name column
            if (e.Column != _columnThatSupportsRenaming)
                return;

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
