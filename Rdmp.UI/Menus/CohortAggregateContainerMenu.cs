// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.SubComponents.Graphs;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Icons.IconProvision;
using PopupChecksUI = Rdmp.UI.ChecksUI.PopupChecksUI;

namespace Rdmp.UI.Menus
{
    internal class CohortAggregateContainerMenu : RDMPContextMenuStrip
    {
        private CohortAggregateContainer _container;

        public CohortAggregateContainerMenu(RDMPContextMenuStripArgs args, CohortAggregateContainer container): base( args, container)
        {
            _container = container;
            var cic = _container.GetCohortIdentificationConfiguration();

            var setOperation = new ToolStripMenuItem("Set Operation");
            setOperation.DropDownItems.Add(GetChangeOperationMenuItem(SetOperation.EXCEPT));
            setOperation.DropDownItems.Add(GetChangeOperationMenuItem(SetOperation.UNION));
            setOperation.DropDownItems.Add(GetChangeOperationMenuItem(SetOperation.INTERSECT));
            Items.Add(setOperation);


            Add(new ExecuteCommandAddCohortSubContainer(_activator,container));


            Add(new ExecuteCommandDisableOrEnable(_activator, container));
            
            Items.Add("Add Catalogue(s) into container", _activator.CoreIconProvider.GetImage(RDMPConcept.Catalogue, OverlayKind.Import), (s, e) => AddCatalogues());

            Items.Add("Add Aggregate(s) into container", _activator.CoreIconProvider.GetImage(RDMPConcept.AggregateGraph, OverlayKind.Import), (s, e) => AddAggregates());
            Items.Add("Import (Copy of) Cohort Set into container", _activator.CoreIconProvider.GetImage(RDMPConcept.CohortAggregate, OverlayKind.Import), (s, e) => AddCohortAggregate());
            Add(new ExecuteCommandImportCohortIdentificationConfiguration(_activator,null,container));

            foreach (ToolStripMenuItem item in Items)
                item.Enabled = item.Enabled && (cic != null && !cic.Frozen);

            Add(new ExecuteCommandUnMergeCohortIdentificationConfiguration(_activator,container));

            //Add Graph results of container commands

            //this requires cache to exist (and be populated for the container)
            if (cic != null && cic.QueryCachingServer_ID != null)
            {
                var matchIdentifiers = new ToolStripMenuItem("Graph All Records For Matching Patients", _activator.CoreIconProvider.GetImage(RDMPConcept.AggregateGraph));

                var availableGraphs = _activator.CoreChildProvider.AllAggregateConfigurations.Where(g => !g.IsCohortIdentificationAggregate).ToArray();
                var allCatalogues = _activator.CoreChildProvider.AllCatalogues;

                if (availableGraphs.Any())
                {

                    foreach (var cata in allCatalogues.OrderBy(c => c.Name))
                    {
                        var cataGraphs = availableGraphs.Where(g => g.Catalogue_ID == cata.ID).ToArray();
                        
                        //if there are no graphs belonging to the Catalogue skip it
                        if(!cataGraphs.Any())
                            continue;

                        //otherwise create a subheading for it
                        var catalogueSubheading = new ToolStripMenuItem(cata.Name, CatalogueIcons.Catalogue);

                        //add graph for each in the Catalogue
                        foreach (var graph in cataGraphs)
                            Add(
                                new ExecuteCommandViewCohortAggregateGraph(_activator,
                                    new CohortSummaryAggregateGraphObjectCollection(container, graph)), Keys.None,
                                catalogueSubheading);

                        matchIdentifiers.DropDownItems.Add(catalogueSubheading);
                    }

                    Items.Add(matchIdentifiers);
                }
            }
        }

        private void AddCohortAggregate()
        {
            var cohortAggregates = _activator.CoreChildProvider.AllAggregateConfigurations.Where(c=>
                c.IsCohortIdentificationAggregate && !c.IsJoinablePatientIndexTable()).ToArray();

            if (!cohortAggregates.Any())
            {
                MessageBox.Show("You do not currently have any cohort sets");
                return;
            }

            AddAggregates(cohortAggregates);
        }

        private void AddAggregates()
        {

            var nonCohortAggregates = RepositoryLocator.CatalogueRepository.GetAllObjects<AggregateConfiguration>().Where(c => !c.IsCohortIdentificationAggregate).ToArray();

            if (!nonCohortAggregates.Any())
            {
                MessageBox.Show("You do not currently have any non-cohort AggregateConfigurations");
                return;
            }

            AddAggregates(nonCohortAggregates);
        }

        private void AddAggregates(AggregateConfiguration[] userCanPickFrom)
        {

            SelectIMapsDirectlyToDatabaseTableDialog dialog = new SelectIMapsDirectlyToDatabaseTableDialog(_activator, userCanPickFrom, false, false);
            dialog.AllowMultiSelect = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (!dialog.MultiSelected.Any())
                    return;

                PopupChecksUI checks = new PopupChecksUI("Adding Aggregate(s)", true);


                foreach (AggregateConfiguration aggregateConfiguration in dialog.MultiSelected)
                {
                    try
                    {
                        var cmd = new AggregateConfigurationCombineable(aggregateConfiguration);
                        var cmdExecution = new ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer(_activator, cmd, _container);
                        if (cmdExecution.IsImpossible)
                            checks.OnCheckPerformed(
                                new CheckEventArgs(
                                    "Could not add AggregateConfiguration " + aggregateConfiguration + " because of Reason:" +
                                    cmdExecution.ReasonCommandImpossible, CheckResult.Fail));
                        else
                        {
                            cmdExecution.Execute();
                            checks.OnCheckPerformed(new CheckEventArgs("Successfully added AggregateConfiguration " + aggregateConfiguration, CheckResult.Success));
                        }
                    }
                    catch (Exception e)
                    {
                        checks.OnCheckPerformed(new CheckEventArgs("Failed to add AggregateConfiguration " + aggregateConfiguration + "(see Exception for details)", CheckResult.Fail, e));
                    }
                }
            }
        }

        private void AddCatalogues()
        {
            SelectIMapsDirectlyToDatabaseTableDialog dialog = new SelectIMapsDirectlyToDatabaseTableDialog(_activator, RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>(),false,false);
            dialog.AllowMultiSelect = true;

            if(dialog.ShowDialog() == DialogResult.OK)
            {
                if(!dialog.MultiSelected.Any())
                    return;

                PopupChecksUI checks = new PopupChecksUI("Adding Catalogues",true);


                foreach (Catalogue catalogue in dialog.MultiSelected)
                {
                    try
                    {
                        var cmd = new CatalogueCombineable(catalogue);
                        var cmdExecution = new ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(_activator, cmd, _container);
                        if (cmdExecution.IsImpossible)
                            checks.OnCheckPerformed(
                                new CheckEventArgs(
                                    "Could not add Catalogue " + catalogue + " because of Reason:" +
                                    cmdExecution.ReasonCommandImpossible, CheckResult.Fail));
                        else
                        {
                            cmdExecution.Execute();
                            checks.OnCheckPerformed(new CheckEventArgs("Successfully added Catalogue " + catalogue,CheckResult.Success));
                        }
                    }
                    catch (Exception e)
                    {
                        checks.OnCheckPerformed(new CheckEventArgs("Failed to add Catalogue " + catalogue + "(see Exception for details)",CheckResult.Fail, e));
                    }
                }
            }
        }

        private ToolStripMenuItem GetChangeOperationMenuItem(SetOperation operation)
        {
            var setOperationMenuItem = new ToolStripMenuItem("Set " + operation, null, (o, args) => SetOperationTo(operation));

            switch (operation)
            {
                case SetOperation.UNION:
                    setOperationMenuItem.Image = CatalogueIcons.UNION;
                    break;
                case SetOperation.INTERSECT:
                    setOperationMenuItem.Image = CatalogueIcons.INTERSECT;
                    break;
                case SetOperation.EXCEPT:
                    setOperationMenuItem.Image = CatalogueIcons.EXCEPT;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("operation");
            }

            setOperationMenuItem.Enabled = _container.Operation != operation;
            
            return setOperationMenuItem;
        }

        public void SetOperationTo(SetOperation newOperation)
        {
            var oldOperation = _container.Operation;
            _container.Operation = newOperation;

            //if the old name was UNION and we are changing to INTERSECT Operation then we should probably change the Name too! even if they have something like 'INTERSECT the people who are big and small' and they change to UNION we want it to be changed to 'UNION the people who are big and small'
            if (_container.Name.StartsWith(oldOperation.ToString()))
                _container.Name = newOperation + _container.Name.Substring(oldOperation.ToString().Length);
            else
            {
                var dialog = new TypeTextOrCancelDialog("New name for container?","You have changed the operation, do you want to give it a new description?", 1000, _container.Name);

                if (dialog.ShowDialog() == DialogResult.OK)
                    _container.Name = dialog.ResultText;
            }
            _container.SaveToDatabase();
            Publish(_container);

        }
    }
}