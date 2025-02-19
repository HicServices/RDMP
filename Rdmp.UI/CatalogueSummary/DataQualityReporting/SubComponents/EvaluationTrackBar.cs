// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.DataQualityEngine.Data;

namespace Rdmp.UI.CatalogueSummary.DataQualityReporting.SubComponents;

/// <summary>
/// The Data Quality Engine stores all validation results in a relational database.  This includes the time the DQE was run.  This allows us to 'rewind' and look at previous results
/// e.g. to compare the quality of the dataset before and after a data load.
/// 
/// <para>If this control is not enabled then it means you have only ever done one DQE evaluation or have never evaluated the dataset by using the DQE.</para>
/// 
/// <para>Dragging the slider will adjust a IDataQualityReportingChart to show the results of the DQE on that day.</para>
/// </summary>
public partial class EvaluationTrackBar : UserControl
{
    private Evaluation[] _evaluations;

    public EvaluationTrackBar()
    {
        InitializeComponent();
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

    public Evaluation[] Evaluations
    {
        get => _evaluations;
        set
        {
            _evaluations = value;
            RefreshUI();
        }
    }

    private List<Label> labels = new();
    public event EvaluationSelectedHandler EvaluationSelected;

    private void RefreshUI()
    {
        if (Evaluations == null || Evaluations.Length == 0)
        {
            Enabled = false;
            return;
        }

        if (Evaluations.Length == 1)
        {
            //there is only 1
            Enabled = false;
            EvaluationSelected(this, Evaluations.Single());
        }
        else
        {
            Enabled = true; //let user drag around the trackbar if he wants
        }

        foreach (var label in labels)
        {
            Controls.Remove(label);
            label.Dispose();
        }

        labels.Clear();

        //if there is at least 2 evaluations done then we need to have a track bar of evaluations
        tbEvaluation.Minimum = 0;
        tbEvaluation.Maximum = Evaluations.Length - 1;
        tbEvaluation.TickFrequency = 1;
        tbEvaluation.Value = Evaluations.Length - 1;
        tbEvaluation.LargeChange = 1;

        for (var i = 0; i < Evaluations.Length; i++)
        {
            var ratio = (double)i / (Evaluations.Length - 1);


            var x = tbEvaluation.Left + (int)(ratio * tbEvaluation.Width);
            var y = tbEvaluation.Bottom - 10;

            var l = new Label
            {
                Text = Evaluations[i].DateOfEvaluation.ToString("d")
            };
            l.Location = new Point(x - l.PreferredWidth / 2, y);

            Controls.Add(l);
            l.BringToFront();

            labels.Add(l);
        }

        tbEvaluation.Value = tbEvaluation.Maximum;
        EvaluationSelected(this, Evaluations[tbEvaluation.Value]);
    }

    private void tbEvaluation_ValueChanged(object sender, EventArgs e)
    {
        if (tbEvaluation.Value >= 0)
            EvaluationSelected(this, Evaluations[tbEvaluation.Value]);
    }
}

public delegate void EvaluationSelectedHandler(object sender, Evaluation evaluation);