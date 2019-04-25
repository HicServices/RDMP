// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel.Composition;
using System.Drawing;
using Rdmp.Core.CatalogueLibrary.CommandExecution.AtomicCommands;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.DataExport.Data.DataTables;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.CommandExecution.AtomicCommands.WindowArranging
{
    public class ExecuteCommandEditDataExtractionProject : BasicUICommandExecution, IAtomicCommandWithTarget
    {
        public Project Project { get; set; }

        [ImportingConstructor]
        public ExecuteCommandEditDataExtractionProject(IActivateItems activator, Project project) : base(activator)
        {
            Project = project;
        }

        public ExecuteCommandEditDataExtractionProject(IActivateItems activator) : base(activator)
        {
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Project, OverlayKind.Edit);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            Project = (Project) target;
            return this;
        }

        public override void Execute()
        {
            if (Project == null)
                SetImpossible("You must choose a Data Extraction Project to edit.");

            base.Execute();
            Activator.WindowArranger.SetupEditDataExtractionProject(this, Project);
        }

        public override string GetCommandHelp()
        {
            return
                "This will take you to the Data Extraction Projects list and allow you to Run the selected project.\r\n" +
                "You must choose a Project from the list before proceeding.";
        }
    }
}