// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using System.Linq;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewExtractionConfigurationForProject:BasicCommandExecution,IAtomicCommand
    {
        private readonly Project _project;

        public ExecuteCommandCreateNewExtractionConfigurationForProject(IBasicActivateItems activator,Project project) : base(activator)
        {
            _project = project;
        }
        
        public ExecuteCommandCreateNewExtractionConfigurationForProject(IBasicActivateItems activator) : base(activator)
        {
            if(!activator.GetAll<IProject>().Any())
                SetImpossible("You do not have any projects yet");
        }

        public override string GetCommandHelp()
        {
            return "Starts a new extraction for the project containing one or more datasets linked against a given cohort";
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ExtractionConfiguration, OverlayKind.Add);
        }

        public override void Execute()
        {
            base.Execute();

            var p = _project ?? SelectOne<Project>(BasicActivator.RepositoryLocator.DataExportRepository);

            if(p == null)
                return;

            string name = "";
            if (!BasicActivator.TypeText(new DialogArgs
            {
                WindowTitle = "New Extraction Configuration",
                TaskDescription = "Enter a name for the new Extraction Configuration",
                EntryLabel = "Name",
                //Add default project name "PROJ_NUMBER YYYY/MM Extraction"
            }, 255, null, out name, false))
                return;

            var newConfig = new ExtractionConfiguration(BasicActivator.RepositoryLocator.DataExportRepository, p, name);

            if (p.GetAssociatedCohortIdentificationConfigurations().Any())
            {
                var chooseCohortCommand = new ExecuteCommandChooseCohort(BasicActivator, newConfig);
                chooseCohortCommand.Execute();
            }

            var chooseDatasetsCommand = new ExecuteCommandAddDatasetsToConfiguration(BasicActivator, newConfig);
            chooseDatasetsCommand.Execute();

            //refresh the project
            Publish(p);
            Activate(newConfig);
            Emphasise(newConfig);
        }
    }
}
