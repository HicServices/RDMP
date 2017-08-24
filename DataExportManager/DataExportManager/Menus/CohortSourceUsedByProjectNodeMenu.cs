using System;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections.Providers;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Menus;
using DataExportManager.CohortUI;
using DataExportManager.Collections.Nodes;
using DataExportManager.Collections.Nodes.UsedByProject;
using DataExportManager.Collections.Providers;
using DataExportManager.ItemActivation;
using RDMPStartup;

namespace DataExportManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    public class CohortSourceUsedByProjectNodeMenu : RDMPContextMenuStrip
    {
        private readonly DataExportChildProvider _childProvider;
        private readonly CohortSourceUsedByProjectNode _cohortSourceUsedByProjectNode;

        public CohortSourceUsedByProjectNodeMenu(IActivateDataExportItems activator, DataExportChildProvider childProvider, CohortSourceUsedByProjectNode cohortSourceUsedByProjectNode)
            : base( activator,cohortSourceUsedByProjectNode.Project)
        {
            _childProvider = childProvider;
            _cohortSourceUsedByProjectNode = cohortSourceUsedByProjectNode;
            
            var viewDetail = new ToolStripMenuItem("Show Detailed Summary of Project Cohorts", CatalogueIcons.CohortsSourceDetailedCounts, (s, e) => ShowDetailedSummaryOfCohorts());
            viewDetail.Enabled = !_cohortSourceUsedByProjectNode.IsEmptyNode;
            Items.Add(viewDetail);

            AddCommonMenuItems();
        }

        private void ShowDetailedSummaryOfCohorts()
        {
            var extractableCohortCollection = new ExtractableCohortCollection();
            extractableCohortCollection.RepositoryLocator = RepositoryLocator;
            _activator.ShowWindow(extractableCohortCollection, true);

            extractableCohortCollection.SetupFor(_cohortSourceUsedByProjectNode.CohortsUsedByProject.Select(u=>u.Cohort).ToArray());
        }
    }
}