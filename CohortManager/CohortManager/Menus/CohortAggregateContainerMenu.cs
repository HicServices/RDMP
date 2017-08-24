using System;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Providers;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CommandExecution;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Menus;
using CatalogueManager.Refreshing;
using CohortManager.Collections.Providers;
using CohortManager.ItemActivation;
using MapsDirectlyToDatabaseTableUI;
using RDMPObjectVisualisation.Copying;
using RDMPObjectVisualisation.Copying.Commands;
using RDMPStartup;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;
using ReusableUIComponents.ChecksUI;
using ReusableUIComponents.Icons.IconProvision;

namespace CohortManager.Menus
{
    internal class CohortAggregateContainerMenu : RDMPContextMenuStrip
    {
        private CohortAggregateContainer _container;

        public CohortAggregateContainerMenu(IActivateCohortIdentificationItems activator, CohortIdentificationConfiguration cic, CohortAggregateContainer container)
            : base( activator, container)
        {
            _container = container;
            
            AddToolStripMenuItemSetTo(SetOperation.EXCEPT);
            AddToolStripMenuItemSetTo(SetOperation.UNION);
            AddToolStripMenuItemSetTo(SetOperation.INTERSECT);

            Items.Add("Add Sub Container", CohortIdentificationIcons.addCohortAggregateContainer,(s, e) => AddNewCohortAggregateContainer());
            
            foreach (ToolStripMenuItem item in Items)
                item.Enabled = item.Enabled && (cic != null && !cic.Frozen);

            Items.Add("Add Catalogue(s) into container", _activator.CoreIconProvider.GetImage(RDMPConcept.Catalogue, OverlayKind.Import), (s, e) => AddCatalogues());

            Items.Add("Add Aggregate(s) into container", _activator.CoreIconProvider.GetImage(RDMPConcept.AggregateGraph, OverlayKind.Import), (s, e) => AddAggregates());
            Items.Add("Import (Copy of) Cohort Set into container", _activator.CoreIconProvider.GetImage(RDMPConcept.CohortAggregate, OverlayKind.Import), (s, e) => AddCohortAggregate());

            AddCommonMenuItems();
        }

        private void AddCohortAggregate()
        {
            var cohortAggregates = RepositoryLocator.CatalogueRepository.GetAllObjects<AggregateConfiguration>().Where(c => c.IsCohortIdentificationAggregate).ToArray();

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

            SelectIMapsDirectlyToDatabaseTableDialog dialog = new SelectIMapsDirectlyToDatabaseTableDialog(userCanPickFrom, false, false);
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
                        var cmd = new AggregateConfigurationCommand(aggregateConfiguration);
                        var cmdExecution = new ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer(_activator, cmd, _container);
                        if (cmdExecution.IsImpossible)
                            checks.OnCheckPerformed(
                                new CheckEventArgs(
                                    "Could not add AggregateConfiguration " + aggregateConfiguration + " because of Reason:" +
                                    cmdExecution.ReasonCommandImpossible, CheckResult.Fail));
                        else
                        {
                            cmdExecution.Execute();
                            checks.OnCheckPerformed(new CheckEventArgs("Succesfully added AggregateConfiguration " + aggregateConfiguration, CheckResult.Success));
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
            SelectIMapsDirectlyToDatabaseTableDialog dialog = new SelectIMapsDirectlyToDatabaseTableDialog(RepositoryLocator.CatalogueRepository.GetAllCatalogues(),false,false);
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
                        var cmd = new CatalogueCommand(catalogue);
                        var cmdExecution = new ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(_activator, cmd, _container);
                        if (cmdExecution.IsImpossible)
                            checks.OnCheckPerformed(
                                new CheckEventArgs(
                                    "Could not add Catalogue " + catalogue + " because of Reason:" +
                                    cmdExecution.ReasonCommandImpossible, CheckResult.Fail));
                        else
                        {
                            cmdExecution.Execute();
                            checks.OnCheckPerformed(new CheckEventArgs("Succesfully added Catalogue " + catalogue,CheckResult.Success));
                        }
                    }
                    catch (Exception e)
                    {
                        checks.OnCheckPerformed(new CheckEventArgs("Failed to add Catalogue " + catalogue + "(see Exception for details)",CheckResult.Fail, e));
                    }
                }
            }
        }

        private void AddNewCohortAggregateContainer()
        {
            var newContainer = new CohortAggregateContainer(RepositoryLocator.CatalogueRepository, SetOperation.UNION);
            _container.AddChild(newContainer);
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_container));
        }

        private void AddToolStripMenuItemSetTo(SetOperation operation)
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
            Items.Add(setOperationMenuItem);

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
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_container));

        }
    }
}