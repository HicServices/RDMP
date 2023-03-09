// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using SixLabors.ImageSharp;
using System.Linq;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.DataRelease;
using Rdmp.UI.ItemActivation;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandRelease: BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private Project _project;
        private ExtractionConfiguration _configuration;
        private ISelectedDataSets _selectedDataSet;

        public ExecuteCommandRelease(IActivateItems activator) : base(activator)
        {
            OverrideCommandName = "Run Release...";
        }

        public override string GetCommandHelp()
        {
            return "Gather all the extracted files into one releasable package and freeze the extraction configuration";
        }

        public override Image<Rgba32> GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Release);
        }

        /// <summary>
        /// Sets the thing being released, valid targets are <see cref="Project"/>, <see cref="ExtractionConfiguration"/> and <see cref="ISelectedDataSets"/>.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            _project =  target as Project;
            _configuration = target as ExtractionConfiguration;
            _selectedDataSet = target as ISelectedDataSets;

            if (_project != null && _project.ExtractionConfigurations.All(ec => ec.IsReleased))
                SetImpossible("There are no unreleased ExtractionConfigurations in Project");

            if (_configuration != null)
            {

                _project = (Project)_configuration.Project;
                
                if (_configuration.IsReleased)
                    SetImpossible("ExtractionConfiguration has already been Released");

                if(_configuration.Cohort_ID == null)
                    SetImpossible("No Cohort Defined");

                if (!_configuration.SelectedDataSets.Any())
                    SetImpossible("No datasets configured");

            }
            if (_selectedDataSet != null)
            {
                _configuration = (ExtractionConfiguration) _selectedDataSet.ExtractionConfiguration;
                _project = (Project) _configuration.Project;

                if(_selectedDataSet.ExtractionConfiguration.IsReleased)
                    SetImpossible("This dataset is part of an ExtractionConfiguration that has already been Released");

                if (_selectedDataSet.ExtractionConfiguration.Cohort_ID == null)
                    SetImpossible("This dataset is part of an ExtractionConfiguration with no Cohort defined");
            }

            return this;
        }

        public override void Execute()
        {
            base.Execute();

            var p = _project;

            if (p == null)
                p = SelectOne(Activator.RepositoryLocator.DataExportRepository.GetAllObjects<Project>());

            if(p == null)
            {
                // user cancelled picking a Project
                return;
            }

            var releaseUI = Activator.Activate<DataReleaseUI, Project>(p);
            
            if(_configuration != null)
                if (_selectedDataSet == null)
                    releaseUI.TickAllFor(_configuration);
                else
                    releaseUI.Tick(_selectedDataSet);            
        }
    }
}
