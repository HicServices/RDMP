using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportManager.DataRelease;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTableUI;
using ReusableLibraryCode.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandRelease: BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private Project _project;
        private ExtractionConfiguration _configuration;

        public ExecuteCommandRelease(IActivateItems activator) : base(activator)
        {
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Release);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            _project =  target as Project;
            _configuration = target as ExtractionConfiguration;

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

            return this;
        }

        public override void Execute()
        {
            base.Execute();

            if (_project == null)
                _project = SelectOne(Activator.RepositoryLocator.DataExportRepository.GetAllObjects<Project>());

            var releaseUI = Activator.Activate<DataReleaseUI, Project>(_project);
            
            if(_configuration != null)
                releaseUI.TickAllFor(_configuration);
       
            _project = null;
            
        }
    }
}
