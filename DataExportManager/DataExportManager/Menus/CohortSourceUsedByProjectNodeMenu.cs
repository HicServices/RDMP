using System;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections;
using CatalogueManager.Collections.Providers;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using DataExportLibrary.Providers.Nodes.UsedByProject;
using DataExportManager.CohortUI;
using DataExportManager.Collections.Providers;
using RDMPStartup;

namespace DataExportManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class CohortSourceUsedByProjectNodeMenu : RDMPContextMenuStrip
    {
        private readonly CohortSourceUsedByProjectNode _cohortSourceUsedByProjectNode;

        public CohortSourceUsedByProjectNodeMenu(RDMPContextMenuStripArgs args, CohortSourceUsedByProjectNode cohortSourceUsedByProjectNode)
            : base( args,cohortSourceUsedByProjectNode.Source)
        {
            _cohortSourceUsedByProjectNode = cohortSourceUsedByProjectNode;
            
            var viewDetail = new ToolStripMenuItem("Show Detailed Summary of Project Cohorts", CatalogueIcons.AllCohortsNode, (s, e) => ShowDetailedSummaryOfCohorts());
            viewDetail.Enabled = !_cohortSourceUsedByProjectNode.IsEmptyNode;
            Items.Add(viewDetail);
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