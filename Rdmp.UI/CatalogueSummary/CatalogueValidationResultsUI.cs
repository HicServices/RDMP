// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using NPOI.SS.Formula.Functions;
using Rdmp.Core;
using Rdmp.Core.CatalogueAnalysisTools.Data;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataQualityEngine.Data;
using Rdmp.Core.Repositories;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.CatalogueSummary;

/// <summary>
/// Shows a longitudinal breakdown of all Data Quality Engine runs on the dataset including the ability to 'rewind' to look at the dataset quality graphs of previous
/// runs of the DQE over time (e.g. before and after a data load).
/// </summary>
public partial class CatalogueValidationResultsUI : CatalogueValidationSummaryScreen_Design
{
    public CatalogueValidationResultsUI()
    {
        InitializeComponent();

        dqePivotCategorySelector1.PivotCategorySelectionChanged +=
            dqePivotCategorySelector1_PivotCategorySelectionChanged;

        AssociatedCollection = RDMPCollection.Catalogue;
        DoubleBuffered = true;
    }

    private void ClearDQEGraphs()
    {
        timePeriodicityChart1.ClearGraph();
        //columnStatesChart1.ClearGraph();
    }

    private CatalogueValidation _lastSelected;

    private void evaluationTrackBar1_EvaluationSelected(object sender, CatalogueValidation evaluation)
    {
        dqePivotCategorySelector1.LoadOptions(evaluation);

        var category = dqePivotCategorySelector1.SelectedPivotCategory;

        timePeriodicityChart1.SelectEvaluation(evaluation, category ?? "ALL");
        //columnStatesChart1.SelectEvaluation(evaluation, category ?? "ALL");
        _lastSelected = evaluation;
        
        var counts = evaluation.GetCounts();

        lblRecordCount.Text = counts.RecordCount.ToString();
        lblExtractionIdentifiersCount.Text = counts.ExtractionIdentifierCount.ToString();
    }

    private void dqePivotCategorySelector1_PivotCategorySelectionChanged()
    {
        if (_lastSelected == null)
            return;

        var category = dqePivotCategorySelector1.SelectedPivotCategory;

        timePeriodicityChart1.SelectEvaluation(_lastSelected, category ?? "ALL");
        //columnStatesChart1.SelectEvaluation(_lastSelected, category ?? "ALL");
    }

    public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        timePeriodicityChart1.SetItemActivator(activator);

        //clear old DQE graphs
        ClearDQEGraphs();

        DQERepository dqeRepository = null;
        try
        {
            //try to get the dqe server
            dqeRepository = new DQERepository(databaseObject.CatalogueRepository);
        }
        catch (Exception)
        {
            //there is no dqe server, ah well nevermind
        }

        //dqe server did exist!
        if (dqeRepository != null)
        {
            //get evaluations for the catalogue
            var evaluations = dqeRepository.GetAllObjectsWhere<CatalogueValidation>("Catalogue_ID", databaseObject.ID).ToArray();
            if (evaluations.Any())
            {
                _lastSelected = evaluations.Last();
                dqePivotCategorySelector1.LoadOptions(_lastSelected);
                dqePivotCategorySelector1_PivotCategorySelectionChanged();
            }
            //there have been some evaluations
            evaluationTrackBar1.Validations = evaluations;
        }

        //CommonFunctionality.Add(
        //    new ExecuteCommandConfigureCatalogueValidationRules(activator).SetTarget(databaseObject));
        //CommonFunctionality.Add(new ExecuteCommandRunDQEOnCatalogue(activator, databaseObject),
        //    "Run Data Quality Engine...");
    }

    public override string GetTabName() => $"DQE:{base.GetTabName()}";

    private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
    {

    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<CatalogueValidationSummaryScreen_Design, UserControl>))]
public abstract class CatalogueValidationSummaryScreen_Design : RDMPSingleDatabaseObjectControl<Catalogue>
{
}