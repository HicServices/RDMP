using System.Drawing;
using System.Linq;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportManager.DataRelease;
using ReusableLibraryCode.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandRelease: BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private Project _project;
        private ExtractionConfiguration _configuration;
        private ISelectedDataSets _selectedDataSet;

        public ExecuteCommandRelease(IActivateItems activator) : base(activator)
        {
        }

        public override string GetCommandHelp()
        {
            return "Gather all the extracted files into one releasable package and freeze the extraction configuration";
        }

        public Image GetImage(IIconProvider iconProvider)
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

            if (_project == null)
                _project = SelectOne(Activator.RepositoryLocator.DataExportRepository.GetAllObjects<Project>());

            var releaseUI = Activator.Activate<DataReleaseUI, Project>(_project);
            
            if(_configuration != null)
                if (_selectedDataSet == null)
                    releaseUI.TickAllFor(_configuration);
                else
                    releaseUI.Tick(_selectedDataSet);


            _project = null;
            
        }
    }
}
