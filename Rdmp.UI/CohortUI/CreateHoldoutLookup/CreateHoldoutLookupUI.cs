// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.


using System;
using System.ComponentModel;
using System.Windows.Forms;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.CohortUI.CreateHoldoutLookup;

/// <summary>
/// Once you have created a cohort for your holdout, this dialog allows you to configure how many and from when to store
/// a catalogue for use as exclusion fields in other cohorts
/// </summary>
public partial class CreateHoldoutLookupUI : RDMPForm
{
    private readonly IExternalCohortTable _target;
    private readonly CohortIdentificationConfiguration _cic;


    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string CohortDescription
    {
        get => tbDescription.Text;
        set => tbDescription.Text = value;
    }


    public CreateHoldoutLookupUI(IActivateItems activator, IExternalCohortTable target,  CohortIdentificationConfiguration cic = null) :
        base(activator)
    {
        _target = target;

        InitializeComponent();

        if (_target == null)
            return;

        _cic = cic;
        tbName.Text = $"holdout_{cic?.Name ?? ""}";

        taskDescriptionLabel1.SetupFor(new DialogArgs
        {
            TaskDescription =
                "For Details, see: https://github.com/HicServices/RDMP/blob/0b4eea78a99e58d75ae5189ec491f5ef54b52e1e/Rdmp.UI/CohortUI/CreateHoldoutLookup/Holdouts.md"
        });
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public CohortHoldoutLookupRequest Result { get; set; }

    private void btnOk_Click(object sender, EventArgs e)
    {
        var name = tbName.Text;
        var minDate = textBox2.Text;
        var maxDate = textBox3.Text;
        var dateColumnName = textBox4.Text;
        var description = tbDescription.Text;
        Result = new CohortHoldoutLookupRequest(_cic, name, decimal.ToInt32(numericUpDown1.Value),
            comboBox1.Text == "%", description, minDate, maxDate, dateColumnName);
        DialogResult = DialogResult.OK;
        Close();
    }
    private void btnCancel_Click(object sender, EventArgs e)
    {
        Result = null;
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void CohortHoldoutCreationRequestUI_Load(object sender, EventArgs e)
    {
        _target.Check(ragSmiley1);
    }

    private void tbName_TextChanged(object sender, EventArgs e)
    {
    }

    private void btnClear_Click(object sender, EventArgs e)
    {
    }


    private void gbChooseCohortType_Enter(object sender, EventArgs e)
    {


    }


    private void label1_Click(object sender, EventArgs e)
    {


    }

    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {

    }

    private void label2_Click(object sender, EventArgs e)
    {

    }

    private void textBox2_TextChanged(object sender, EventArgs e)
    {

    }

    private void label3_Click(object sender, EventArgs e)
    {

    }

    private void label4_Click(object sender, EventArgs e)
    {

    }

    private void tbDescription_TextChanged(object sender, EventArgs e)
    {

    }
}
