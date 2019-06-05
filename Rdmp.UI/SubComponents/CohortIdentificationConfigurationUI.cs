// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CohortCreation.Execution;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.UI.Collections;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.CommandExecution.AtomicCommands.CohortCreationCommands;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;

namespace Rdmp.UI.SubComponents
{
    /// <summary>
    /// Allows you to view/edit a CohortIdentificationConfiguration.  You should start by giving it a meaningful name e.g. 'Project 132 Cases - Deaths caused by diabetic medication' 
    /// and a comprehensive description e.g. 'All patients in Tayside and Fife who are over 16 at the time of their first prescription of a diabetic medication (BNF chapter 6.1) 
    /// and died within 6 months'.  An accurate up-to-date description will help future data analysts to understand the configuration.
    /// 
    /// <para>If you have a large data repository or plan to use lots of different datasets or complex filters in your CohortIdentificationCriteria you should configure a caching database
    /// from the dropdown menu.</para>
    /// 
    /// <para>Next you should add datasets and set operations (<see cref="CohortAggregateContainer"/>) either by right clicking or dragging and dropping into the tree view</para>
    /// 
    /// <para>In the above example you might have </para>
    /// 
    /// <para>Set 1 - Prescribing</para>
    /// 
    /// <para>    Filter 1 - Prescription is for a diabetic medication</para>
    /// 
    /// <para>    Filter 2 - Prescription is the first prescription of it's type for the patient</para>
    /// 
    /// <para>    Filter 3 - Patient died within 6 months of prescription</para>
    /// 
    /// <para>INTERSECT</para>
    /// 
    /// <para>Set 2 - Demography</para>
    ///     
    /// <para>    Filter 1 - Latest known healthboard is Tayside or Fife</para>
    /// 
    /// <para>    Filter 2 - Date of Death - Date of Birth > 16 years</para>
    ///  
    /// </summary>
    public partial class CohortIdentificationConfigurationUI : CohortIdentificationConfigurationUI_Design, ISaveableUI
    {
        private CohortIdentificationConfiguration _configuration;

        ToolStripMenuItem _miClearCache = new ToolStripMenuItem("Clear Cached Records");

        ToolStripMenuItem cbIncludeCumulative = new ToolStripMenuItem("Calculate Cumulative Totals") { CheckOnClick = true };


        private RDMPCollectionCommonFunctionality _commonFunctionality;

        public CohortIdentificationConfigurationUI()
        {
            InitializeComponent();

            olvExecute.IsButton = true;
            olvExecute.ButtonSizing = OLVColumn.ButtonSizingMode.CellBounds;
            tlvCic.RowHeight = 19;
            olvExecute.AspectGetter += ExecuteAspectGetter;
            tlvCic.ButtonClick += tlvCic_ButtonClick;
            AssociatedCollection = RDMPCollection.Cohort;

            CohortCompilerUI1.SelectionChanged += CohortCompilerUI1_SelectionChanged;

            _miClearCache.Click += MiClearCacheClick;
            _miClearCache.Image = CatalogueIcons.ExternalDatabaseServer_Cache;

            cbIncludeCumulative.CheckedChanged += (s, e) => CohortCompilerUI1.Compiler.IncludeCumulativeTotals = cbIncludeCumulative.Checked;
        }

        void CohortCompilerUI1_SelectionChanged(IMapsDirectlyToDatabaseTable obj)
        {
            var joinable = obj as JoinableCohortAggregateConfiguration;
            
            tlvCic.SelectedObject = joinable != null ? joinable.AggregateConfiguration : obj;
        }

        public override void SetDatabaseObject(IActivateItems activator, CohortIdentificationConfiguration databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            _configuration = databaseObject;

            lblFrozen.Visible = _configuration.Frozen;

            tbID.Text = _configuration.ID.ToString();
            tbName.Text = _configuration.Name;
            tbDescription.Text = _configuration.Description;
            ticket.TicketText = _configuration.Ticket;
            tlvCic.Enabled = !databaseObject.Frozen;
            
            if (_commonFunctionality == null)
            {
                _commonFunctionality = new RDMPCollectionCommonFunctionality();

                _commonFunctionality.SetUp(RDMPCollection.Cohort, tlvCic, activator, olvNameCol, olvNameCol, new RDMPCollectionCommonFunctionalitySettings
                {
                    AddFavouriteColumn = false,
                    AddCheckColumn = false,
                    AllowPinning = false,
                    AllowSorting =  false,
                });

                tlvCic.AddObject(_configuration);
                tlvCic.ExpandAll();
            }

            CommonFunctionality.AddToMenu(cbIncludeCumulative);
            CommonFunctionality.AddToMenu(new ToolStripSeparator());
            CommonFunctionality.AddToMenu(_miClearCache);
            CommonFunctionality.AddToMenu(new ExecuteCommandSetQueryCachingDatabase(Activator, _configuration));
            CommonFunctionality.AddToMenu(new ExecuteCommandCreateNewQueryCacheDatabase(activator, _configuration));

            CommonFunctionality.AddToMenu(new ToolStripSeparator());
            CommonFunctionality.AddToMenu(new ExecuteCommandShowXmlDoc(activator, "CohortIdentificationConfiguration.QueryCachingServer_ID", "Query Caching"), "Help (What is Query Caching)");

            CommonFunctionality.Add(new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(activator, null).SetTarget(_configuration),
                "Commit Cohort",
                activator.CoreIconProvider.GetImage(RDMPConcept.ExtractableCohort,OverlayKind.Add));

            CohortCompilerUI1.SetDatabaseObject(activator, databaseObject);
        }

        public override void SetItemActivator(IActivateItems activator)
        {
            base.SetItemActivator(activator);
            ticket.SetItemActivator(activator);
        }

        public override string GetTabName()
        {
            return "Execute:" + base.GetTabName();
        }

        private void tbName_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbName.Text))
            {
                tbName.Text = "No Name";
                tbName.SelectAll();
            }

            _configuration.Name = tbName.Text;
        }

        private void tbDescription_TextChanged(object sender, EventArgs e)
        {
            _configuration.Description = tbDescription.Text;
        }

        private void ticket_TicketTextChanged(object sender, EventArgs e)
        {
            _configuration.Ticket = ticket.TicketText;
        }


        private object ExecuteAspectGetter(object rowObject)
        {
            //don't expose any buttons if global execution is in progress
            if (CohortCompilerUI1.IsExecutingGlobalOperations())
                return null;

            if (rowObject is AggregateConfiguration || rowObject is CohortAggregateContainer)
            {
                var plannedOp = GetNextOperation(GetState((IMapsDirectlyToDatabaseTable)rowObject));

                if (plannedOp == Operation.None)
                    return null;

                return plannedOp;
            }

            return null;
        }

        private Operation GetNextOperation(CompilationState currentState)
        {
            switch (currentState)
            {
                case CompilationState.NotScheduled:
                    return Operation.Execute;
                case CompilationState.Scheduled:
                    return Operation.None;
                case CompilationState.Executing:
                    return Operation.Cancel;
                case CompilationState.Finished:
                    return Operation.Execute;
                case CompilationState.Crashed:
                    return Operation.Execute;
                default:
                    throw new ArgumentOutOfRangeException("currentState");
            }
        }

        #region Job control
        private enum Operation
        {
            Execute,
            Cancel,
            Clear,
            None
        }

        public override void ConsultAboutClosing(object sender, FormClosingEventArgs e)
        {
            base.ConsultAboutClosing(sender, e);

            CohortCompilerUI1.ConsultAboutClosing(sender,e);
        }

        private CompilationState GetState(IMapsDirectlyToDatabaseTable rowObject)
        {
            return CohortCompilerUI1.GetState(rowObject);
        }

        void tlvCic_ButtonClick(object sender, CellClickEventArgs e)
        {
            var o = e.Model;
            var aggregate = o as AggregateConfiguration;
            var container = o as CohortAggregateContainer;

            if (aggregate != null)
            {
                var joinable = aggregate.JoinableCohortAggregateConfiguration;
                                    
                if(joinable != null)
                    OrderActivity(GetNextOperation(GetState(joinable)), joinable);
                else
                    OrderActivity(GetNextOperation(GetState(aggregate)), aggregate);
            }
            if (container != null)
            {
                OrderActivity(GetNextOperation(GetState(container)),container);
            }
        }

        private void OrderActivity(Operation operation, IMapsDirectlyToDatabaseTable o)
        {
            switch (operation)
            {
                case Operation.Execute:
                    CohortCompilerUI1.StartThisTaskOnly(o);
                    break;
                case Operation.Cancel:
                    CohortCompilerUI1.Cancel(o);
                    break;
                case Operation.Clear:
                    CohortCompilerUI1.Clear(o);
                    break;
                case Operation.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("operation");
            }
        }


        private void btnExecute_Click(object sender, EventArgs e)
        {
            CohortCompilerUI1.StartAll();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            var plan = PlanGlobalOperation();

            btnExecute.Enabled = plan == Operation.Execute;
            btnAbortLoad.Enabled = plan == Operation.Cancel;
            
            _miClearCache.Enabled = CohortCompilerUI1.AnyCachedTasks();
        }

        private Operation PlanGlobalOperation()
        {
            var allTasks = CohortCompilerUI1.GetAllTasks();

            //if any are still executing or scheduled for execution
            if (allTasks.Any(t => t.State == CompilationState.Executing || t.State == CompilationState.Scheduled))
                return Operation.Cancel;

            //if all are complete
            return Operation.Execute;
        }
        #endregion

        private void MiClearCacheClick(object sender, EventArgs e)
        {
            CohortCompilerUI1.ClearAllCaches();
        }

        private void btnAbortLoad_Click(object sender, EventArgs e)
        {
            CohortCompilerUI1.CancelAll();
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<CohortIdentificationConfigurationUI_Design, UserControl>))]
    public abstract class CohortIdentificationConfigurationUI_Design : RDMPSingleDatabaseObjectControl<CohortIdentificationConfiguration>
    {
    }
}
