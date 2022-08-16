// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;
using System.Linq;

namespace Rdmp.UI.Menus
{
    internal class ExecuteCommandViewCommits : BasicUICommandExecution
    {
        private IMapsDirectlyToDatabaseTable _o;

        public ExecuteCommandViewCommits(IActivateItems activator, IMapsDirectlyToDatabaseTable o):base(activator)
        {
            _o = o;
            OverrideCommandName = "View History";

            if(
                !activator.RepositoryLocator.CatalogueRepository
                .GetAllObjectsWhere<Memento>(nameof(Memento.ReferencedObjectID), o.ID)
                .Where((m) => m.IsReferenceTo(o))
                .Any())
            {
                SetImpossible("No commits have been made yet");
            }
        }

        public override void Execute()
        {
            base.Execute();

            var ui = new CommitsUI(Activator, _o);
            ui.Show();
        }
    }
}