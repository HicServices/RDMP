// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Rdmp.Core.DataQualityEngine.Data;

namespace Rdmp.UI.CatalogueSummary.DataQualityReporting.SubComponents;

/// <summary>
/// Data Quality Engine records all validation results in a relational database, this includes recording with each result the Pivot column value found when evaluating the row.  A Pivot
/// column is a single categorical field in the dataset that is the most useful way of slicing the dataset e.g. Healthboard.  If your dataset has a pivot column
/// then this control will let you change which results are displayed in any IDataQualityReportingCharts from either All rows in the dataset or only
/// those where the pivot column has a specific value.  If your pivot column contains nulls then these records will only be audited under the ALL category.
/// </summary>
public partial class DQEPivotCategorySelector : UserControl
{
    public event Action PivotCategorySelectionChanged;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string SelectedPivotCategory { get; private set; }

    public DQEPivotCategorySelector()
    {
        InitializeComponent();
    }

    public void LoadOptions(Evaluation evaluation)
    {
        flowLayoutPanel1.Controls.Clear();
        SelectedPivotCategory = "ALL";

        if (evaluation == null)
            return;

        foreach (var category in evaluation.GetPivotCategoryValues())
        {
            var rb = new RadioButton
            {
                Text = category,
                Tag = category
            };

            if (rb.Text == "ALL")
                rb.Checked = true; //always select this one by default first

            rb.CheckedChanged += OnCheckedChanged;
            flowLayoutPanel1.Controls.Add(rb);
        }
    }

    private void OnCheckedChanged(object sender, EventArgs eventArgs)
    {
        SelectedPivotCategory = (string)((RadioButton)sender).Tag;
        PivotCategorySelectionChanged?.Invoke();
    }
}