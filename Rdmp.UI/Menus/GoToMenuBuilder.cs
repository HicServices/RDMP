// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Injection;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Governance;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Providers;
using Rdmp.UI.Collections;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.CommandExecution.AtomicCommands.UIFactory;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.Menus
{
    /// <summary>
    /// Builds the "Go To" submenu for travelling between objects in an RDMP database (e.g. Catalogue to extractions using the Catalogue)
    /// </summary>
    public class GoToMenuBuilder
    {
        private readonly IActivateItems _activator;
        private AtomicCommandUIFactory _commandFactory;
        public const string GoTo = "Go To";

        public GoToMenuBuilder(IActivateItems activator)
        {
            _activator = activator;
            _commandFactory = new AtomicCommandUIFactory(activator);
        }

        public ToolStripMenuItem GetMenu(IMapsDirectlyToDatabaseTable forObject)
        {
            //forget old values, get them up to the minute
            if(forObject is IInjectKnown ii)
                ii.ClearAllInjections();

            var menu = new ToolStripMenuItem(GoTo);
            
            // Go to import / export definitions
            var export = _activator.RepositoryLocator.CatalogueRepository.GetReferencesTo<ObjectExport>(forObject).FirstOrDefault();

            if(export != null)
                Add(menu,new ExecuteCommandShow(_activator,export,0,true){OverrideCommandName = "Show Export Definition"});

            var import = _activator.RepositoryLocator.CatalogueRepository.GetReferencesTo<ObjectImport>(forObject).FirstOrDefault();
            if(import != null)
                Add(menu,new ExecuteCommandShow(_activator,import,0){OverrideCommandName = "Show Import Definition"});

            // cic => associated projects
            if (forObject is CohortIdentificationConfiguration cic)
            {
                AddGoTo(menu,()=>{
                    if(_activator.CoreChildProvider is DataExportChildProvider dx)
                        if(dx.AllProjectAssociatedCics != null)
                            return dx.AllProjectAssociatedCics.Where(a=>a.CohortIdentificationConfiguration_ID == cic.ID).Select(a=>a.Project).Distinct();
                    
                    return new CohortIdentificationConfiguration[0];
                },"Project(s)");
            }
           
            if (forObject is ColumnInfo columnInfo)
            {
                AddGoTo<TableInfo>(menu,columnInfo.TableInfo_ID, "Table");
                AddGoTo(menu,()=>_activator.CoreChildProvider.AllCatalogueItems.Where(catItem=>catItem.ColumnInfo_ID == columnInfo.ID),"Catalogue Item(s)");
                AddGoTo<ANOTable>(menu,columnInfo.ANOTable_ID);
            }
            
            if (forObject is ExtractionInformation ei)
            {
                AddGoTo<Catalogue>(menu,ei.CatalogueItem?.Catalogue_ID, "Catalogue");
                AddGoTo<CatalogueItem>(menu,ei.CatalogueItem_ID, "CatalogueItem");
                AddGoTo(menu,ei.ColumnInfo,"Column");
            }
            
            if (forObject is CatalogueItem ci)
            {
                AddGoTo<Catalogue>(menu,ci.Catalogue_ID, "Catalogue");
                AddGoTo(menu,ci.ExtractionInformation, "ExtractionInformation");
                AddGoTo(menu,ci.ColumnInfo, "Column");
            }

            if (forObject is ExtractableDataSet eds)
            {
                AddGoTo<Catalogue>(menu,eds.Catalogue_ID);
            
                AddGoTo(menu,()=>
                {
                    if(_activator.CoreChildProvider is DataExportChildProvider dx)
                        return dx.SelectedDataSets.Where(s=>s.ExtractableDataSet_ID == eds.ID).Select(s=>s.ExtractionConfiguration);
                    
                    return new SelectedDataSets[0];               
                }                
                ,"Extraction Configurations");
            }

            if(forObject is GovernancePeriod period)
                AddGoTo(menu,()=>period.GovernedCatalogues,"Catalogue(s)");

            if(forObject is JoinInfo j)
                AddGoTo<ColumnInfo>(menu,j.ForeignKey_ID,"Foreign Key");


            if (forObject is Lookup lookup)
            {
                AddGoTo<TableInfo>(menu,lookup.Description.TableInfo_ID,"Table");
                AddGoTo<ColumnInfo>(menu,lookup.ForeignKey_ID,"Foreign Key");
            }

            if(forObject is ExtractionFilter masterFilter)
            {
                AddGoTo(menu,()=>
                _activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<AggregateFilter>("ClonedFromExtractionFilter_ID", masterFilter.ID).Select(f=>f.GetAggregate()).Distinct()
                ,"Usages (in Cohort Builder)");

                AddGoTo(menu,()=>
                _activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<DeployedExtractionFilter>("ClonedFromExtractionFilter_ID" , masterFilter.ID).Select(f=>f.GetDataset().ExtractionConfiguration).Distinct()
                ,"Usages (in Extractions)");
            }

            if(forObject is IFilter filter && filter.ClonedFromExtractionFilter_ID.HasValue)
            {
                try
                {
                    var parent = _activator.RepositoryLocator.CatalogueRepository.GetObjectByID<ExtractionFilter>(filter.ClonedFromExtractionFilter_ID.Value);
    
                    if(parent != null)
                        AddGoTo(menu,parent,"Parent Filter");
                }
                catch (KeyNotFoundException)
                {
                }
            }

            if (forObject is SelectedDataSets selectedDataSet)
                AddGoTo<Catalogue>(menu,selectedDataSet.ExtractableDataSet.Catalogue_ID);

            if(forObject is TableInfo tableInfo)
                AddGoTo(menu,()=>tableInfo.ColumnInfos.SelectMany(c=>_activator.CoreChildProvider.AllCatalogueItems.Where(catItem=>catItem.ColumnInfo_ID == c.ID).Select(catItem=>catItem.Catalogue)).Distinct(),"Catalogue(s)");

            if (forObject is AggregateConfiguration aggregate)
            {
                AddGoTo<CohortIdentificationConfiguration>(menu,aggregate.GetCohortIdentificationConfigurationIfAny()?.ID);
                AddGoTo<Catalogue>(menu,aggregate.Catalogue_ID);
            }

            if (forObject is Catalogue catalogue)
            {
                AddGoTo<LoadMetadata>(menu,catalogue.LoadMetadata_ID,"Data Load");

                if(_activator.CoreChildProvider is DataExportChildProvider exp)
                {
                    var cataEds = exp.ExtractableDataSets.SingleOrDefault(d=>d.Catalogue_ID == catalogue.ID);
                    if(cataEds != null)
                        AddGoTo(menu,()=>cataEds.ExtractionConfigurations,"Extraction Configuration(s)");
                }

                AddGoTo(menu,()=>catalogue.GetTableInfoList(true),"Table(s)");

                AddGoTo(menu,
                    ()=>
                        _activator
                            .CoreChildProvider
                            .AllAggregateConfigurations.Where(ac=>ac.IsCohortIdentificationAggregate && ac.Catalogue_ID == catalogue.ID)
                            .Select(ac=>ac.GetCohortIdentificationConfigurationIfAny())
                            .Where(cataCic=>cataCic != null)
                            .Distinct(),
                    "Cohort Identification Configuration(s)");

                AddGoTo(menu,()=>_activator.CoreChildProvider.AllGovernancePeriods.Where(p=>p.GovernedCatalogues.Contains(catalogue)),"Governance");

            }
            
            if(forObject is ExtractableCohort cohort)
                AddGoTo(menu,()=>
                    {
                        if(_activator.CoreChildProvider is DataExportChildProvider dx)
                            return dx.ExtractionConfigurations.Where(ec => ec.Cohort_ID == cohort.ID);
                    
                        return new ExtractionConfiguration[0];
                    }               
                    ,"Extraction Configurations");
            
            //if it is a masquerader and masquerading as a DatabaseEntity then add a goto the object
            if (forObject is IMasqueradeAs masquerader)
            {
                if(masquerader.MasqueradingAs() is DatabaseEntity m)
                    AddGoTo(menu,m,m.GetType().Name);
            }

            if (menu.DropDownItems.Count == 0)
                menu.Enabled = false;

            return menu;
        }

        /// <summary>
        /// Creates a new command under GoTo that navigates the user to the results of <paramref name="func"/>.  This function
        /// is only evaluated when the GoTo menu is expanded (not when the main context menu is popped).
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="func"></param>
        /// <param name="title"></param>
        protected void AddGoTo(ToolStripMenuItem menu,Func<IEnumerable<IMapsDirectlyToDatabaseTable>> func, string title)
        {
            bool bFirstTime = true;

            var proxy = new ToolStripMenuItem("proxy") {Enabled = false};
            menu.DropDownItems.Add(proxy);

            menu.DropDownOpening += (s,e) => 
            {
                if(bFirstTime)
                {
                    menu.DropDownItems.Remove(proxy);
                    AddGoTo(menu,func(),title);
                    bFirstTime = false;
                }
            };
        }

        protected void AddGoTo(ToolStripMenuItem menu,IEnumerable<IMapsDirectlyToDatabaseTable> objects, string title)
        {           
            Add(menu,new ExecuteCommandShow(_activator,objects,1){OverrideCommandName = title });
        }

        private void Add(ToolStripMenuItem menu, IAtomicCommand cmd)
        {
            menu.DropDownItems.Add(_commandFactory.CreateMenuItem(cmd));
        }

        protected void AddGoTo(ToolStripMenuItem menu,IMapsDirectlyToDatabaseTable o, string title)
        {
            Add(menu,new ExecuteCommandShow(_activator,o,1){OverrideCommandName = title });
        }
        protected void AddGoTo<T>(ToolStripMenuItem menu,int? foreignKey, string title = null) where T:IMapsDirectlyToDatabaseTable
        {
            if(foreignKey.HasValue)
                Add(menu,new ExecuteCommandShow(_activator,_activator.RepositoryLocator.GetObjectByID<T>(foreignKey.Value),1){OverrideCommandName = title ?? typeof(T).Name });
            else
                Add(menu,new ImpossibleCommand("No object exists"){ OverrideCommandName = title ?? typeof(T).Name});
        }
    }
}
