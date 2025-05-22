using NPOI.OpenXmlFormats.Spreadsheet;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    class ExecuteCommandCopyCatalogueFiltersFromCohortConfigurationToExtraction : BasicCommandExecution, IAtomicCommand
    {

        private readonly IBasicActivateItems _activator;
        private readonly SelectedDataSets _sds;
        private List<IFilter> _rootFilters = [];
        private List<IContainer> _containers = [];
        private List<AggregateConfiguration> _aggregateConfiguration = [];

        private ExecuteCommandCopyCatalogueFiltersFromCohortConfigurationToExtraction(IBasicActivateItems activator) : base(activator)
        {
        }

        public ExecuteCommandCopyCatalogueFiltersFromCohortConfigurationToExtraction(IBasicActivateItems activator, SelectedDataSets sds)
        {
            _activator = activator;
            _sds = sds;
            var cicAssociation = _sds.ExtractionConfiguration.Project.ProjectCohortIdentificationConfigurationAssociations.FirstOrDefault();
            //(new DataExportChildProvider(_activator.CoreChildProvider)).Projects.First(p => p.ID ==_sds.ExtractionConfiguration.pr)
            if (cicAssociation is null)
            {
                SetImpossible("No Cohort Definition available");
                return;
            }
            var cic = cicAssociation.CohortIdentificationConfiguration;
            var acs = cic.RootCohortAggregateContainer.GetAggregateConfigurations();
            foreach (var ac in acs)
            {
                if (ac.Catalogue_ID == _sds.ExtractableDataSet.Catalogue_ID)
                {
                    _aggregateConfiguration.Add(ac);
                }
            }
            var containers = cic.RootCohortAggregateContainer.GetAllSubContainersRecursively();
            foreach (var container in containers)
            {
                var aggregateConfigurations = container.GetAggregateConfigurations();
                foreach (var ac in aggregateConfigurations)
                {
                    if (ac.Catalogue.ID == _sds.ExtractableDataSet.Catalogue_ID)
                    {
                        _rootFilters.AddRange(ac.RootFilterContainer.GetFilters());
                        _containers.AddRange(ac.RootFilterContainer.GetSubContainers());
                    }
                }
            }
        }


        private void AddContainer(IContainer container, IContainer sdsContainer)
        {
            foreach (var filter in container.GetFilters())
            {
                var cmd = new ExecuteCommandCreateNewFilter(_activator, sdsContainer, filter);
                cmd.Execute();
            }
            foreach (var subContainer in container.GetSubContainers())
            {
                var cmd = new ExecuteCommandAddNewFilterContainer(_activator, sdsContainer);
                cmd.Execute();
                var con = sdsContainer.GetSubContainers().Last();
                con.Operation = subContainer.Operation;
                con.SaveToDatabase();
                AddContainer(subContainer, sdsContainer.GetSubContainers().Last());
            }
        }


        public override void Execute()
        {
            base.Execute();
            foreach (var ac in _aggregateConfiguration)
            {
                _sds.CreateRootContainerIfNotExists();
                AddContainer(ac.RootFilterContainer, _sds.RootFilterContainer);
                //foreach(var f in ac.RootFilterContainer.GetFilters())
                //{
                //    var cmd = new ExecuteCommandCreateNewFilter(_activator, _sds.RootFilterContainer, f);
                //    cmd.Execute();
                //    //_sds.RootFilterContainer.AddChild(f);
                //}
                //foreach(var c in ac.RootFilterContainer.GetSubContainers())
                //{
                //    _sds.RootFilterContainer.AddChild(c);
                //}
                //_sds.RootFilterContainer.AddChild(ac.RootFilterContainer);
                _sds.SaveToDatabase();
                //Publish(_sds);
            }
            //foreach (var filter in _rootFilters)
            //{
            //    var cmd = new ExecuteCommandCreateNewFilter(_activator, _sds.RootFilterContainer, filter);
            //    cmd.Execute();
            //}
            //foreach (var container in _containers)
            //{
            //    _sds.CreateRootContainerIfNotExists();
            //    _sds.RootFilterContainer.AddChild(container);
            //    _sds.SaveToDatabase();
            //    Publish(_sds);
            //}

        }
    }
}
