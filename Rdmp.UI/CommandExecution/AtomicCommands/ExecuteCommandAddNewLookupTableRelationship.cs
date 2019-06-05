// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.Dialogs;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandAddNewLookupTableRelationship : BasicUICommandExecution,IAtomicCommand
    {
        private readonly Catalogue _catalogueIfKnown;
        private readonly TableInfo _lookupTableInfoIfKnown;

        public ExecuteCommandAddNewLookupTableRelationship(IActivateItems activator, Catalogue catalogueIfKnown, TableInfo lookupTableInfoIfKnown) : base(activator)
        {
            _catalogueIfKnown = catalogueIfKnown;
            _lookupTableInfoIfKnown = lookupTableInfoIfKnown;

            if(catalogueIfKnown == null && lookupTableInfoIfKnown == null)
                throw new NotSupportedException("You must know either the lookup table or the Catalogue you want to configure it on");
        }

        public override string GetCommandHelp()
        {
            return "Tells RDMP that a table contains code/description mappings for one of the columns in your dataset and that you (may) want them linked in when extracting the dataset";
        }

        public override void Execute()
        {
            base.Execute();
        
            if (_catalogueIfKnown == null)  
                PickCatalogueAndLaunchForTableInfo(_lookupTableInfoIfKnown);
            else
                Activator.ActivateLookupConfiguration(this, _catalogueIfKnown, _lookupTableInfoIfKnown);
        }

        private void PickCatalogueAndLaunchForTableInfo(TableInfo tbl)
        {
            try
            {
                var dr = MessageBox.Show(
@"You have chosen to make '" + tbl + @"' a Lookup Table (e.g T = Tayside, F=Fife etc).  In order to do this you will need to pick which Catalogue the column
provides a description for (a given TableInfo can be a Lookup for many columns in many datasets)."
                    , "Create Lookup", MessageBoxButtons.OKCancel);

                if (dr == DialogResult.OK)
                {
                    Catalogue cata;
                    if(SelectOne(tbl.Repository,out cata))
                        Activator.ActivateLookupConfiguration(this, cata, tbl);
                }
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show("Error creating Lookup", exception);
            }
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Lookup, OverlayKind.Add);
        }
    }
}