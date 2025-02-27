// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.CatalogueAnalysisTools.Data;
using Rdmp.Core.DataQualityEngine.Data;
using Rdmp.UI.CatalogueSummary.DataQualityReporting.SubComponents;

namespace Rdmp.UI.CatalogueSummary.DataQualityReporting;

/// <summary>
/// Only visible after running the data quality engine on a dataset (Catalogue).   Shows each extractable column in the dataset with a horizontal bar indicating what proportion
/// of the values in the dataset that are in that column are passing validation.  By comparing this chart with the TimePeriodicityChart you can if validation problems in the
/// dataset are attributable to specific columns or whether the quality of the entire dataset is bad.  For example if the entire TimePeriodicityChart is red (failing validation)
/// but only one column in the ColumnStatesChart is showing red then you know that the scope of the problem is limited only to that column.
/// 
/// <para>Also included in each column bar (underneath the main colour) is a black/grey bar which shows what proportion of the values in the column were null.</para>
/// </summary>
public partial class ColumnStatesChart : UserControl, IDataQualityReportingChart
{
    public ColumnStatesChart()
    {
        InitializeComponent();
    }

    /// <inheritdoc/>
    public void ClearGraph()
    {
        panel1.Controls.Clear();
    }

    /// <inheritdoc/>
    public void SelectEvaluation(Evaluation evaluation, string pivotCategoryValue)
    {
        GenerateChart(evaluation, pivotCategoryValue);
    }

    private void GenerateChart(Evaluation evaluation, string pivotCategoryValue)
    {
        panel1.Controls.Clear();

        var row = 0;

        foreach (var property in evaluation.ColumnStates.Select(static c => c.TargetProperty).Distinct())
        {
            var bar = new ConsequenceBar
            {
                Label = property,
                Correct = evaluation.ColumnStates.Where(c => c.TargetProperty.Equals(property))
                    .Sum(s => s.PivotCategory.Equals(pivotCategoryValue) ? s.CountCorrect : 0),
                Invalid = evaluation.ColumnStates.Where(c => c.TargetProperty.Equals(property)).Sum(s =>
                    s.PivotCategory.Equals(pivotCategoryValue) ? s.CountInvalidatesRow : 0),
                Missing = evaluation.ColumnStates.Where(c => c.TargetProperty.Equals(property))
                    .Sum(s => s.PivotCategory.Equals(pivotCategoryValue) ? s.CountMissing : 0),
                Wrong = evaluation.ColumnStates.Where(c => c.TargetProperty.Equals(property))
                    .Sum(s => s.PivotCategory.Equals(pivotCategoryValue) ? s.CountWrong : 0),
                DBNull = evaluation.ColumnStates.Where(c => c.TargetProperty.Equals(property))
                    .Sum(s => s.PivotCategory.Equals(pivotCategoryValue) ? s.CountDBNull : 0),

                Width = panel1.Width,
                Location = new Point(0, 23 * row++),

                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            bar.GenerateToolTip();

            panel1.Controls.Add(bar);
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        foreach (var bar in panel1.Controls.OfType<ConsequenceBar>())
            bar.Invalidate();
    }
}