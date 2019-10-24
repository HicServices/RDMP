// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandOverrideRawServer:BasicUICommandExecution,IAtomicCommand,IAtomicCommandWithTarget
    {
        private readonly LoadMetadata _loadMetadata;
        private ExternalDatabaseServer _server;
        private ExternalDatabaseServer[] _available;

        public ExecuteCommandOverrideRawServer(IActivateItems activator,LoadMetadata loadMetadata) : base(activator)
        {
            _loadMetadata = loadMetadata;
            _available =
                activator.CoreChildProvider.AllExternalServers.Where(s => string.IsNullOrWhiteSpace(s.CreatedByAssembly)).ToArray();

            if(!_available.Any())
                SetImpossible("There are no compatible servers");
        }

        public override void Execute()
        {
            base.Execute();

            if (_server == null)
            {
                var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(_available,true, false);

                if (dialog.ShowDialog() == DialogResult.OK)
                    _server = dialog.Selected as ExternalDatabaseServer;
                else
                    return;
            }

            _loadMetadata.OverrideRAWServer_ID = _server == null ? null : (int?)_server.ID;
            _loadMetadata.SaveToDatabase();

            Publish(_loadMetadata);
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ExternalDatabaseServer, OverlayKind.Link);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            var candidate = target as ExternalDatabaseServer;

            if (candidate != null && _available.Contains(candidate))
                _server = candidate;

            return this;
        }
    }
}