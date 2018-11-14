using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.LogViewer.Tabs;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataQualityEngine.Data;
using ReusableUIComponents;

namespace Dashboard.CatalogueSummary
{
    /// <summary>
    /// Summarises the state of a single dataset (Catalogue).  This includes:
    /// 
    /// <para>Loads - A history of all the loads you have ever made to the dataset (highlighted according to whether they were successful or failed).  Expanding nodes will let you see the progress
    /// messages, error messages, tables loaded and data sources etc (See <see cref="LoggingTab"/> for more information about the RDMP logging structure).</para>
    /// 
    /// <para>Descriptions / Issues - Pie charts showing how many of the extractable columns are lacking descriptions and how many outstanding issues there are on the dataset (See IssueUI)</para>
    /// 
    /// <para>Data Quality Tab - Shows a longitudinal breakdown of all Data Quality Engine runs on the dataset including the ability to 'rewind' to look at the dataset quality graphs of previous
    /// runs of the DQE over time (e.g. before and after a data load).</para>
    /// </summary>
    public partial class CatalogueSummaryScreen : CatalogueSummaryScreen_Design
    {
        private Catalogue _catalogue;

        public CatalogueSummaryScreen()
        {
            InitializeComponent();

            dqePivotCategorySelector1.PivotCategorySelectionChanged += dqePivotCategorySelector1_PivotCategorySelectionChanged;

            AssociatedCollection = RDMPCollection.Catalogue;
            DoubleBuffered = true;
        }

        public Catalogue Catalogue
        {
            get { return _catalogue; }
            private set
            {
                //novel value
                _catalogue = value;
                
                //clear old DQE graphs
                ClearDQEGraphs();

                //if there is a catalogue
                if(Catalogue != null)
                {

                    DQERepository dqeRepository = null;
                    try
                    {
                        //try to get the dqe server
                        dqeRepository = new DQERepository((CatalogueRepository)Catalogue.Repository);
                    }
                    catch (Exception)
                    {
                        //there is no dqe server, ah well nevermind
                    }

                    //dqe server did exist!
                    if(dqeRepository != null)
                    {
                        //get evaluations for the catalogue
                        Evaluation[] evaluations = dqeRepository.GetAllEvaluationsFor(Catalogue).ToArray();
                        
                        //there have been some evaluations
                        evaluationTrackBar1.Evaluations = evaluations;
                    }
                }

            }
        }

        private void ClearDQEGraphs()
        {
            timePeriodicityChart1.ClearGraph();
            columnStatesChart1.ClearGraph();
        }

        private Evaluation _lastSelected;
        private void evaluationTrackBar1_EvaluationSelected(object sender, Evaluation evaluation)
        {
            dqePivotCategorySelector1.LoadOptions(evaluation);

            string category = dqePivotCategorySelector1.SelectedPivotCategory;
            
            timePeriodicityChart1.SelectEvaluation(evaluation,category??"ALL");
            columnStatesChart1.SelectEvaluation(evaluation, category ?? "ALL");
            _lastSelected = evaluation;
        }

        void dqePivotCategorySelector1_PivotCategorySelectionChanged()
        {
            if(_lastSelected ==  null)
                return;
            
            string category = dqePivotCategorySelector1.SelectedPivotCategory;

            timePeriodicityChart1.SelectEvaluation(_lastSelected, category ?? "ALL");
            columnStatesChart1.SelectEvaluation(_lastSelected, category ?? "ALL");

        }

        public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            Catalogue = databaseObject;
        }

        public override string GetTabName()
        {
            return "DQE:"+ base.GetTabName();
            
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<CatalogueSummaryScreen_Design, UserControl>))]
    public abstract class CatalogueSummaryScreen_Design:RDMPSingleDatabaseObjectControl<Catalogue>
    {
    }
}
