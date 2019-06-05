// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewANOTable : BasicUICommandExecution,IAtomicCommand
    {
        private IExternalDatabaseServer _anoStoreServer;

        public ExecuteCommandCreateNewANOTable(IActivateItems activator) : base(activator)
        {
            _anoStoreServer = Activator.ServerDefaults.GetDefaultFor(PermissableDefaults.ANOStore);

            if(_anoStoreServer == null)
                SetImpossible("No default ANOStore has been set");
        }

        public override string GetCommandHelp()
        {
            return "Create a table for storing anonymous identifier mappings for a given type of code e.g. 'PatientId' / 'GP Codes' etc";
        }

        public override Image GetImage(IIconProvider iconProvider)
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