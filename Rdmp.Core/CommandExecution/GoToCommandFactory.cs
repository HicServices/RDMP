// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Injection;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Governance;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Providers;

namespace Rdmp.Core.CommandExecution
{
    /// <summary>
    /// Builds the "Go To" submenu for travelling between objects in an RDMP database (e.g. Catalogue to extractions using the Catalogue)
    /// </summary>
    public class GoToCommandFactory : CommandFactoryBase
    {
        private readonly IBasicActivateItems _activator;
        public const string GoTo = "Go To";

        public GoToCommandFactory(IBasicActivateItems activator)
        {
            _activator = activator;
        }
        
        public IEnumerable<IAtomicCommand> GetCommands(object forObject)
        {
            //forget old values, get them up to the minute
            if (Is(forObject , out IInjectKnown ii))
                ii.ClearAllInjections();

            if (Is(forObject, out IMapsDirectlyToDatabaseTable mt))
            {
                // Go to import / export definitions
                var export = _activator.RepositoryLocator.CatalogueRepository.GetReferencesTo<ObjectExport>(mt).FirstOrDefault();

                if (export != null)
                    yield return new ExecuteCommandShow(_activator, export, 0, true) { OverrideCommandName = "Show Export Definition" };

                var import = _activator.RepositoryLocator.CatalogueRepository.GetReferencesTo<ObjectImport>(mt).FirstOrDefault();
                if (import != null)
                    yield return new ExecuteCommandShow(_activator, import, 0) { OverrideCommandName = "Show Import Definition" };

                yield return new ExecuteCommandSimilar(_activator, mt, false) { GoTo = true };
            }

            // cic => associated projects
            if (Is(forObject,out CohortIdentificationConfiguration cic))
            {
                yield return new ExecuteCommandShow(_activator,() =>
                {
                    if (_activator.CoreChildProvider is DataExportChildProvider dx)
                        if (dx.AllProjectAssociatedCics != null)
                            return dx.AllProjectAssociatedCics.Where(a => a.CohortIdentificationConfiguration_ID == cic.ID).Select(a => a.Project).Distinct();

                    return new CohortIdentificationConfiguration[0];
                }){OverrideCommandName="Project(s)" };
            }

            if (Is(forObject,out ColumnInfo columnInfo))
            {
                yield return new ExecuteCommandSimilar(_activator, columnInfo, true) { 
                    OverrideCommandName = "Different",
                    GoTo = true,
                };

                yield return new ExecuteCommandShow(_activator, columnInfo.TableInfo_ID, typeof(TableInfo));
                yield return new ExecuteCommandShow(_activator,() => _activator.CoreChildProvider.AllCatalogueItems.Where(catItem => catItem.ColumnInfo_ID == columnInfo.ID)){
                    OverrideCommandName = "Catalogue Item(s)" };
                
                yield return new ExecuteCommandShow(_activator,columnInfo.ANOTable_ID, typeof(ANOTable));
            }

            if (Is(forObject, out ExtractionInformation ei))
            {
                yield return new ExecuteCommandShow(_activator, ei.CatalogueItem?.Catalogue_ID, typeof(Catalogue));
                yield return new ExecuteCommandShow(_activator, ei.CatalogueItem_ID, typeof(CatalogueItem));
                yield return new ExecuteCommandShow(_activator, ei.ColumnInfo,0,true);
            }

            if (Is(forObject, out CatalogueItem ci))
            {
                yield return new ExecuteCommandShow(_activator, ci.Catalogue_ID, typeof(Catalogue));
                yield return new ExecuteCommandShow(_activator, ci.ExtractionInformation,0,true);
                yield return new ExecuteCommandShow(_activator, ci.ColumnInfo, 0,true);
            }

            if (Is(forObject, out ExtractableDataSet eds))
            {
                yield return new ExecuteCommandShow(_activator, eds.Catalogue_ID, typeof(Catalogue));

                yield return new ExecuteCommandShow(_activator, () =>
                 {
                     if (_activator.CoreChildProvider is DataExportChildProvider dx)
                         return dx.SelectedDataSets.Where(s => s.ExtractableDataSet_ID == eds.ID).Select(s => s.ExtractionConfiguration);

                     return new SelectedDataSets[0];
                 }){ OverrideCommandName = "Extraction Configuration(s)"};
            }

            if (Is(forObject, out GovernancePeriod period))
                yield return new ExecuteCommandShow(_activator, () => period.GovernedCatalogues){OverrideCommandName = "Catalogue(s)" };

            if (Is(forObject, out JoinInfo j))
                yield return new ExecuteCommandShow(_activator, j.ForeignKey_ID, typeof(ColumnInfo)){OverrideCommandName="Foreign Key" };

            if (Is(forObject, out Lookup lookup))
            {
                yield return new ExecuteCommandShow(_activator, lookup.Description.TableInfo_ID, typeof(TableInfo));
                yield return new ExecuteCommandShow(_activator, lookup.ForeignKey_ID, typeof(ColumnInfo)){OverrideCommandName = "Foreign Key" };
            }

            if (Is(forObject, out ExtractionFilter masterFilter))
            {
                yield return new ExecuteCommandShow(_activator, () =>
                 _activator.RepositoryLocator.CatalogueRepository
                    .GetAllObjectsWhere<AggregateFilter>("ClonedFromExtractionFilter_ID", masterFilter.ID)
                    .Select(f => f.GetAggregate())
                    .Where(a=>a != null).Distinct()
                ){OverrideCommandName = "Usages (in Cohort Builder)" };

                yield return new ExecuteCommandShow(_activator, () =>
                 _activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<DeployedExtractionFilter>("ClonedFromExtractionFilter_ID", masterFilter.ID).Select(f => f.GetDataset().ExtractionConfiguration).Distinct()
                ){OverrideCommandName = "Usages (in Extractions)" };
            }

            if (Is(forObject, out IFilter filter) && filter.ClonedFromExtractionFilter_ID.HasValue)
            {
                ExtractionFilter parent = null;

                try
                {
                        parent = _activator.RepositoryLocator.CatalogueRepository.GetObjectByID<ExtractionFilter>(filter.ClonedFromExtractionFilter_ID.Value);
                }
                catch (KeyNotFoundException)
                {
                    // new ImpossibleCommand("Parent filter has been deleted") { OverrideCommandName = "Parent Filter" };
                }

                if (parent != null)
                    yield return new ExecuteCommandShow(_activator, parent,0){OverrideCommandName = "Parent Filter" };
            }

            if (Is(forObject, out SelectedDataSets selectedDataSet))
                yield return new ExecuteCommandShow(_activator, selectedDataSet.ExtractableDataSet.Catalogue_ID,typeof(Catalogue));

            if (Is(forObject, out TableInfo tableInfo))
                yield return new ExecuteCommandShow(_activator, () => tableInfo.ColumnInfos.SelectMany(c => _activator.CoreChildProvider.AllCatalogueItems.Where(catItem => catItem.ColumnInfo_ID == c.ID).Select(catItem => catItem.Catalogue)).Distinct()){OverrideCommandName="Catalogue(s)" };

            if (Is(forObject, out AggregateConfiguration aggregate))
            {
                yield return new ExecuteCommandShow(_activator, aggregate.GetCohortIdentificationConfigurationIfAny()?.ID,typeof(CohortIdentificationConfiguration));
                yield return new ExecuteCommandShow(_activator, aggregate.Catalogue_ID,typeof(Catalogue));
            }

            if (Is(forObject, out Catalogue catalogue))
            {
                yield return new ExecuteCommandShow(_activator, catalogue.LoadMetadata_ID,typeof(LoadMetadata)){OverrideCommandName = "Data Load" };

                if (_activator.CoreChildProvider is DataExportChildProvider exp)
                {
                    var cataEds = exp.ExtractableDataSets.SingleOrDefault(d => d.Catalogue_ID == catalogue.ID);

                    if (cataEds != null)
                    {
                        if (cataEds.Project_ID != null)
                        {
                            yield return new ExecuteCommandShow(_activator, () =>
                            {
                                return new[] { _activator.RepositoryLocator.DataExportRepository.GetObjectByID<Project>(cataEds.Project_ID.Value) };
                            }
                            )
                            { OverrideCommandName = "Associated Project" };
                        }

                        yield return new ExecuteCommandShow(_activator, () => cataEds.ExtractionConfigurations.Select(c=>c.Project).Distinct()){OverrideCommandName = "Extracted In (Project)" };
                        yield return new ExecuteCommandShow(_activator, () => cataEds.ExtractionConfigurations){OverrideCommandName = "Extracted In (Extraction Configuration)" };                        
                    }
                        
                }

                yield return new ExecuteCommandShow(_activator, () => catalogue.GetTableInfoList(true)){OverrideCommandName="Table(s)" };

                yield return new ExecuteCommandShow(_activator,
                    () =>
                        _activator
                            .CoreChildProvider
                            .AllAggregateConfigurations.Where(ac => ac.IsCohortIdentificationAggregate && ac.Catalogue_ID == catalogue.ID)
                            .Select(ac => ac.GetCohortIdentificationConfigurationIfAny())
                            .Where(cataCic => cataCic != null)
                            .Distinct())
                    {
                    OverrideCommandName ="Cohort Identification Configuration(s)" 
                    };


                yield return new ExecuteCommandShow(_activator, () => _activator.CoreChildProvider.AllGovernancePeriods.Where(p => p.GovernedCatalogues.Contains(catalogue))){
                    OverrideCommandName = "Governance" };

            }

            if (Is(forObject, out ExtractableCohort cohort))
                yield return new ExecuteCommandShow(_activator, () =>
                     {
                         if (_activator.CoreChildProvider is DataExportChildProvider dx)
                             return dx.ExtractionConfigurations.Where(ec => ec.Cohort_ID == cohort.ID);

                         return new ExtractionConfiguration[0];
                     }){
                    OverrideCommandName = "Extraction Configurations" };

            //if it is a masquerader and masquerading as a DatabaseEntity then add a goto the object
            if (forObject is IMasqueradeAs masqueraderIfAny)
            {
                if (masqueraderIfAny.MasqueradingAs() is DatabaseEntity m)
                    yield return new ExecuteCommandShow(_activator, m,0,true);
            }
        }
    }
}
