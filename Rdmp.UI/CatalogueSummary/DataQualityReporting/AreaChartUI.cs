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
using Rdmp.Core.CommandExecution;
using Rdmp.Core.DataQualityEngine.Data;
using Rdmp.UI.CatalogueSummary.DataQualityReporting.SubComponents;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.CatalogueSummary.DataQualityReporting;

/// <summary>
/// Only visible after running the Data Quality Engine at least once on a given dataset (Catalogue).  Shows the number of records each month in the dataset that are passing/failing
/// validation as a stack chart.  The Data tab will show you the raw counts that power the graph.  See SecondaryConstraintUI for validation configuration and ConsequenceKey for the
/// meanings of each consequence classification.
/// </summary>
public partial class AreaChartUI : RDMPUserControl
{
    private readonly ChartLookAndFeelSetter _chartLookAndFeelSetter = new();

    private Func<int,int> _OnTabChange;

    public AreaChartUI(Func<int,int> onTabChange=null)
    {
        InitializeComponent();
        _OnTabChange = onTabChange;
    }


    /// <inheritdoc/>
    public void ClearGraph()
    {
        chart1.Series.Clear();
    }

    private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {
        if(_OnTabChange is not null)
        {
            _OnTabChange(tabControl1.SelectedIndex);
        }
    }


    public void GenerateChart(DataTable dt, string seriesName)
    {

        chart1.Visible = false;

        if (dt == null)
        {
            ClearGraph();
            chart1.Visible = true;
            return;
        }

        chart1.DataSource = dt;
        dataGridView1.DataSource = dt;

        chart1.Series.Clear();


        if (dt.Rows.Count != 0)
            ChartLookAndFeelSetter.PopulateYearMonthChart(chart1, dt, seriesName,1);

        chart1.DataBind();
        chart1.Visible = true;

    }

}