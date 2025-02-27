// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Rdmp.Core.CatalogueAnalysisTools.Data;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.DataQualityEngine.Data;
using Rdmp.UI.CatalogueSummary.DataQualityReporting.SubComponents;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.CatalogueSummary.DataQualityReporting;

/// <summary>
/// Only visible after running the Data Quality Engine at least once on a given dataset (Catalogue).  Shows the number of records each month in the dataset that are passing/failing
/// validation as a stack chart.  The Data tab will show you the raw counts that power the graph.  See SecondaryConstraintUI for validation configuration and ConsequenceKey for the
/// meanings of each consequence classification.
/// </summary>
public partial class TimePeriodicityChartNoControls : RDMPUserControl
{
    private readonly ChartLookAndFeelSetter _chartLookAndFeelSetter = new();

    public TimePeriodicityChartNoControls()
    {
        InitializeComponent();

        chart1.PaletteCustomColors = new Color[]
        {
            ConsequenceBar.CorrectColor,
            ConsequenceBar.WrongColor,
            ConsequenceBar.MissingColor,
            ConsequenceBar.InvalidColor
        };
        chart1.Palette = ChartColorPalette.None;

        dataGridView1.ColumnAdded += (s, e) => e.Column.FillWeight = 1;
    }


    /// <inheritdoc/>
    public void ClearGraph()
    {
        chart1.Series.Clear();
    }

    private string _pivotCategoryValue;


    public void SelectEvaluation(CatalogueValidation evaluation, string pivotCategoryValue)
    {
        _pivotCategoryValue = pivotCategoryValue;
        GenerateChart(evaluation, pivotCategoryValue);
    }

       

    private void GenerateChart(CatalogueValidation evaluation, string pivotCategoryValue)
    {

        chart1.Visible = false;
        var dt = evaluation.GenerateDataTable(pivotCategoryValue);

        if (dt == null)
        {
            ClearGraph();
            chart1.Visible = true;
            return;
        }
        ClearGraph();
        chart1.DataSource = dt;
        dataGridView1.DataSource = dt;



        if (dt.Rows.Count != 0)
            ChartLookAndFeelSetter.PopulateYearMonthChart(chart1, dt, "Data Quality");

        chart1.DataBind();
        chart1.Visible = true;
        chart1.ChartAreas[0].Visible = true;
    }

   
    private void chart1_MouseDown(object sender, MouseEventArgs e)
    {
        if (!chart1.Focused)
            chart1.Focus();

        return;
    }
}