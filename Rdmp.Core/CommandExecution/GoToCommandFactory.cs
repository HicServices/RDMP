﻿// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using System.Linq;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Governance;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers;
using SixLabors.ImageSharp.PixelFormats;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Injection;

namespace Rdmp.Core.CommandExecution;

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

            if(SupportsReplacement(forObject))
            {
                yield return new ExecuteCommandShow(_activator, () => GetReplacementIfAny(mt)) { OverrideCommandName = "Replacement" };
            }
                    

            yield return new ExecuteCommandSimilar(_activator, mt, false) { GoTo = true };
        }

        // cic => associated projects
        if (Is(forObject,out CohortIdentificationConfiguration cic))
        {
            yield return new ExecuteCommandShow(_activator, () =>
            {
                if (_activator.CoreChildProvider is DataExportChildProvider { AllProjectAssociatedCics: not null } dx) return dx.AllProjectAssociatedCics.Where(a => a.CohortIdentificationConfiguration_ID == cic.ID).Select(a => a.Project).Distinct();

                return Array.Empty<CohortIdentificationConfiguration>();
            })
            {
                OverrideCommandName = "Project(s)",
                OverrideIcon = GetImage(RDMPConcept.Project)
            };
        }

        if (Is(forObject,out ColumnInfo columnInfo))
        {
            yield return new ExecuteCommandSimilar(_activator, columnInfo, true) { 
                OverrideCommandName = "Different",
                GoTo = true,
                OverrideIcon = GetImage(RDMPConcept.ColumnInfo)
            };

            yield return new ExecuteCommandShow(_activator, columnInfo.TableInfo_ID, typeof(TableInfo)) {
                OverrideCommandName = "Table Info",
                OverrideIcon = GetImage(RDMPConcept.TableInfo)};
            yield return new ExecuteCommandShow(_activator,() => _activator.CoreChildProvider.AllCatalogueItems.Where(catItem => catItem.ColumnInfo_ID == columnInfo.ID)){
                OverrideCommandName = "Catalogue Item(s)",
                OverrideIcon = GetImage(RDMPConcept.CatalogueItem)
            };
                
            yield return new ExecuteCommandShow(_activator, columnInfo.ANOTable_ID, typeof(ANOTable)) { 
                OverrideCommandName = "ANO Table",
                OverrideIcon = GetImage(RDMPConcept.ANOTable) };
        }

        if (Is(forObject, out ExtractionInformation ei))
        {
            yield return new ExecuteCommandShow(_activator, ei.CatalogueItem?.Catalogue_ID, typeof(Catalogue)) {
                OverrideCommandName = "Catalogue",
                OverrideIcon = GetImage(RDMPConcept.Catalogue) 
            };
            yield return new ExecuteCommandShow(_activator, ei.CatalogueItem_ID, typeof(CatalogueItem)) {
                OverrideCommandName = "Catalogue Item",
                OverrideIcon = GetImage(RDMPConcept.CatalogueItem) 
            };
            yield return new ExecuteCommandShow(_activator, ei.ColumnInfo, 0, true)
            {
                OverrideCommandName = "Column Info",
                OverrideIcon = GetImage(RDMPConcept.ColumnInfo) 
            };
        }

        if (Is(forObject, out CatalogueItem ci))
        {
            yield return new ExecuteCommandShow(_activator, ci.Catalogue_ID, typeof(Catalogue)) {
                OverrideCommandName = "Catalogue",
                OverrideIcon = GetImage(RDMPConcept.Catalogue) 
            };
            yield return new ExecuteCommandShow(_activator, ci.ExtractionInformation,0,true) {
                OverrideCommandName = "Extraction Information",
                OverrideIcon = GetImage(RDMPConcept.ExtractionInformation) 
            };
            yield return new ExecuteCommandShow(_activator, ci.ColumnInfo, 0,true) {
                OverrideCommandName = "Column Info",
                OverrideIcon = GetImage(RDMPConcept.ColumnInfo) };
        }

        if (Is(forObject, out ExtractableDataSet eds))
        {
            yield return new ExecuteCommandShow(_activator, eds.Catalogue_ID, typeof(Catalogue)) {
                OverrideCommandName = "Catalogue",
                OverrideIcon = GetImage(RDMPConcept.Catalogue) 
            };

            yield return new ExecuteCommandShow(_activator, () =>
            {
                if (_activator.CoreChildProvider is DataExportChildProvider dx)
                    return dx.SelectedDataSets.Where(s => s.ExtractableDataSet_ID == eds.ID).Select(s => s.ExtractionConfiguration);

                return Array.Empty<SelectedDataSets>();
            }){ OverrideCommandName = "Extraction Configuration(s)", OverrideIcon = GetImage(RDMPConcept.ExtractionConfiguration) };
        }

        if (Is(forObject, out GovernancePeriod period))
            yield return new ExecuteCommandShow(_activator, () => period.GovernedCatalogues){OverrideCommandName = "Catalogue(s)", OverrideIcon = GetImage(RDMPConcept.Catalogue) };

        if (Is(forObject, out JoinInfo j))
            yield return new ExecuteCommandShow(_activator, j.ForeignKey_ID, typeof(ColumnInfo)){OverrideCommandName="Foreign Key", OverrideIcon = GetImage(RDMPConcept.ColumnInfo) };

        if (Is(forObject, out Lookup lookup))
        {
            yield return new ExecuteCommandShow(_activator, lookup.Description.TableInfo_ID, typeof(TableInfo));
            yield return new ExecuteCommandShow(_activator, lookup.ForeignKey_ID, typeof(ColumnInfo)){OverrideCommandName = "Foreign Key", OverrideIcon = GetImage(RDMPConcept.ColumnInfo) };
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
                _activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<DeployedExtractionFilter>("ClonedFromExtractionFilter_ID", masterFilter.ID)
                    .Select(f => f.GetDataset()?.ExtractionConfiguration)
                    .Where(c => c != null).Distinct()
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
                yield return new ExecuteCommandShow(_activator, parent,0){OverrideCommandName = "Parent Filter", OverrideIcon = GetImage(RDMPConcept.Filter) };
        }

        if (Is(forObject, out SelectedDataSets selectedDataSet))
        {
            yield return new ExecuteCommandShow(_activator, selectedDataSet.ExtractableDataSet.Catalogue_ID, typeof(Catalogue))
            {
                OverrideCommandName = "Catalogue",
                OverrideIcon = GetImage(RDMPConcept.Catalogue)
            };

            var ep = selectedDataSet.ExtractionProgressIfAny;
            if(ep != null)
            {
                yield return new ExecuteCommandShow(_activator, ep, 0, true);
            }
        }

        if (Is(forObject, out TableInfo tableInfo))
            yield return new ExecuteCommandShow(_activator, () => tableInfo.ColumnInfos.SelectMany(c => _activator.CoreChildProvider.AllCatalogueItems.Where(catItem => catItem.ColumnInfo_ID == c.ID).Select(catItem => catItem.Catalogue)).Distinct()){OverrideCommandName="Catalogue(s)", OverrideIcon = GetImage(RDMPConcept.Catalogue) };

        if (Is(forObject, out AggregateConfiguration aggregate))
        {
            yield return new ExecuteCommandShow(_activator, aggregate.GetCohortIdentificationConfigurationIfAny()?.ID, typeof(CohortIdentificationConfiguration)) {
                OverrideCommandName = "Cohort Identification Configuration",
                OverrideIcon = GetImage(RDMPConcept.CohortIdentificationConfiguration) };
            yield return new ExecuteCommandShow(_activator, aggregate.Catalogue_ID, typeof(Catalogue)) { 
                OverrideCommandName = "Catalogue",
                OverrideIcon = GetImage(RDMPConcept.Catalogue) };
        }

        if (Is(forObject, out Catalogue catalogue))
        {
            yield return new ExecuteCommandShow(_activator, catalogue.LoadMetadata_ID,typeof(LoadMetadata)){OverrideCommandName = "Data Load", OverrideIcon = GetImage(RDMPConcept.LoadMetadata) };

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
                            { OverrideCommandName = "Associated Project", OverrideIcon = GetImage(RDMPConcept.Project) };
                    }

                    yield return new ExecuteCommandShow(_activator, () => cataEds.ExtractionConfigurations.Select(c=>c.Project).Distinct()){OverrideCommandName = "Extracted In (Project)", OverrideIcon = GetImage(RDMPConcept.Project) };
                    yield return new ExecuteCommandShow(_activator, () => cataEds.ExtractionConfigurations){OverrideCommandName = "Extracted In (Extraction Configuration)", OverrideIcon = GetImage(RDMPConcept.ExtractionConfiguration) };                        
                }
                        
            }

            yield return new ExecuteCommandShow(_activator, () => catalogue.GetTableInfoList(true)){OverrideCommandName="Table(s)", OverrideIcon = GetImage(RDMPConcept.TableInfo) };

            yield return new ExecuteCommandShow(_activator,
                    () =>
                        _activator
                            .CoreChildProvider
                            .AllAggregateConfigurations.Where(ac => ac.IsCohortIdentificationAggregate && ac.Catalogue_ID == catalogue.ID)
                            .Select(ac => ac.GetCohortIdentificationConfigurationIfAny())
                            .Where(cataCic => cataCic != null)
                            .Distinct())
                { OverrideCommandName ="Cohort Identification Configuration(s)", OverrideIcon = GetImage(RDMPConcept.CohortIdentificationConfiguration) };


            yield return new ExecuteCommandShow(_activator, () => _activator.CoreChildProvider.AllGovernancePeriods.Where(p => p.GovernedCatalogues.Contains(catalogue))){
                OverrideCommandName = "Governance",
                OverrideIcon = GetImage(RDMPConcept.GovernancePeriod)
            };

        }

        if (Is(forObject, out ExtractableCohort cohort))
        {
            yield return new ExecuteCommandShow(_activator, () =>
            {
                if (_activator.CoreChildProvider is DataExportChildProvider dx)
                    return dx.ExtractionConfigurations.Where(ec => ec.Cohort_ID == cohort.ID);

                return Array.Empty<ExtractionConfiguration>();
            })
            {
                OverrideCommandName = "Extraction Configuration(s)",
                OverrideIcon = GetImage(RDMPConcept.ExtractionConfiguration)
            };

            yield return new ExecuteCommandShow(_activator, () =>
            {
                if (_activator.CoreChildProvider is DataExportChildProvider dx)
                    return dx.Projects.Where(p => p.ProjectNumber == cohort.ExternalProjectNumber);

                return Array.Empty<Project>();
            })
            {
                OverrideCommandName = "Project(s)",
                OverrideIcon = GetImage(RDMPConcept.Project)
            };
        }

        //if it is a masquerader and masquerading as a DatabaseEntity then add a goto the object
        if (forObject is IMasqueradeAs masqueraderIfAny)
        {
            if (masqueraderIfAny.MasqueradingAs() is DatabaseEntity m)
                yield return new ExecuteCommandShow(_activator, m, 0, true) { OverrideIcon = _activator.CoreIconProvider.GetImage(m) };
        }
    }

    private bool SupportsReplacement(object o)
    {
        return o switch
        {
            DashboardLayout _=> false,
            _ => true
        };
    }

    private IEnumerable<IMapsDirectlyToDatabaseTable> GetReplacementIfAny(IMapsDirectlyToDatabaseTable mt)
    {
        var replacement = _activator.RepositoryLocator.CatalogueRepository
            .GetAllObjectsWhere<ExtendedProperty>("Name",ExtendedProperty.ReplacedBy)
            .FirstOrDefault(r=>r.IsReferenceTo(mt));

        if(replacement == null)
            return Enumerable.Empty<IMapsDirectlyToDatabaseTable>();

        return new []{mt.Repository.GetObjectByID(mt.GetType(),int.Parse(replacement.Value))};
    }

    private Image<Rgba32> GetImage(RDMPConcept concept)
    {
        return _activator.CoreIconProvider.GetImage(concept);
    }
}