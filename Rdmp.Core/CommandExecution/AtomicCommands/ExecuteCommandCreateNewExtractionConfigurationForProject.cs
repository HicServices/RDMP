// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    /// <summary>
    /// Creates a new <see cref="ExtractionConfiguration"/> under a given <see cref="Project"/>
    /// </summary>
    public class ExecuteCommandCreateNewExtractionConfigurationForProject:BasicCommandExecution,IAtomicCommand
    {
        private readonly Project _project;
        private readonly string _name;

        /// <summary>
        /// True to prompt the user to pick an <see cref="ExtractableCohort"/> after creating the <see cref="ExtractionConfiguration"/>
        /// </summary>
        public bool PromptForCohort { get; set; } = true;

        /// <summary>
        /// True to prompt the user to pick some <see cref="ExtractableDataSet"/> after creating the <see cref="ExtractionConfiguration"/>
        /// </summary>
        public bool PromptForDatasets { get; set; } = true;

        [UseWithObjectConstructor]
        public ExecuteCommandCreateNewExtractionConfigurationForProject(IBasicActivateItems activator,

            [DemandsInitialization("The Project under which to create the new ExtractionConfiguration")]
            Project project,
            [DemandsInitialization("The name for the new ExtractionConfiguration")]
            string name = "") : base(activator)
        {
            _project = project;
            this._name = name;
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

            string name = _name;
            
            // if we don't have a name and we are running in interactive mode
            if(string.IsNullOrWhiteSpace(name) && BasicActivator.IsInteractive)
            {
                if (!BasicActivator.TypeText(new DialogArgs
                {
                    WindowTitle = "New Extraction Configuration",
                    TaskDescription = "Enter a name for the new Extraction Configuration",
                    EntryLabel = "Name",
                    //Add default project name "PROJ_NUMBER YYYY/MM Extraction"
                }, 255, null, out name, false))
                    return;
            }

            // create the new config
            var newConfig = new ExtractionConfiguration(BasicActivator.RepositoryLocator.DataExportRepository, p, name);

            var chooseCohort = new ExecuteCommandChooseCohort(BasicActivator, newConfig);
            if (PromptForCohort && BasicActivator.IsInteractive && !chooseCohort.IsImpossible)
            {
                chooseCohort.Execute();
            }
            
            var chooseDatasetsCommand = new ExecuteCommandAddDatasetsToConfiguration(BasicActivator, newConfig);

            if (PromptForDatasets && BasicActivator.IsInteractive && !chooseDatasetsCommand.IsImpossible)
            {
                chooseDatasetsCommand.Execute();
            }

            //refresh the project
            Publish(p);
            Activate(newConfig);
            Emphasise(newConfig);
        }
    }
}
