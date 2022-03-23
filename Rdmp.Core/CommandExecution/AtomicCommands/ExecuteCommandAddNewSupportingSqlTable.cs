// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandAddNewSupportingSqlTable : BasicCommandExecution,IAtomicCommand
    {
        private readonly Catalogue _catalogue;
        private readonly string _name;

        public ExecuteCommandAddNewSupportingSqlTable(IBasicActivateItems activator, Catalogue catalogue, string name = null) : base(activator)
        {
            _catalogue = catalogue;
            _name = name;
        }

        public override string GetCommandHelp()
        {
            return "Allows you to specify some freeform SQL that helps understand / interact with a dataset.  Optionally this SQL can be run and the results provided in project extractions.";
        }

        public override void Execute()
        {
            base.Execute();


            var c = _catalogue;
            var name = _name;

            if (c == null)
            {
                if (BasicActivator.SelectObject(new DialogArgs()
                {
                    WindowTitle = "Add Supporting SQL Table",
                    TaskDescription = "Select which Catalogue you want to add the Supporting SQL Table to."

                }, BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>(), out var selected))
                {
                    c = selected;
                }
                else
                {
                    // user cancelled selecting a Catalogue
                    return;
                }
            }

            if (name == null && BasicActivator.IsInteractive)
            {
                if (!BasicActivator.TypeText(new DialogArgs
                {
                    WindowTitle = "Supporting SQL Table Name",
                    TaskDescription = "Enter a name that describes what data the query will show.  This is a human readable name not a table name.",
                    EntryLabel = "Name"
                }, 255, null, out name, false))
                {
                    // user cancelled typing a name
                    return;
                }
            }

            var newSqlTable = new SupportingSQLTable((ICatalogueRepository)c.Repository, c, name ?? "New Supporting SQL Table " + Guid.NewGuid());

            Activate(newSqlTable);
            Publish(c);
        }
        
        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.SupportingSQLTable, OverlayKind.Add);
        }
    }
}