// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.Wizard;

/// <summary>
/// Part of SimpleFilterUI.  Allows you to specify the value of a given parameter of the filter.  There can be multiple parameters on a given filter (or none).  For example a filter
/// 'Drug Prescribed' might have a parameter @drugName and another @amountPrescribed.
/// </summary>
public partial class SimpleParameterUI : UserControl
{
    private readonly IActivateItems _activator;
    private readonly ISqlParameter _parameter;

    public SimpleParameterUI(IActivateItems activator,ISqlParameter parameter)
    {
        _activator = activator;
        _parameter = parameter;
        InitializeComponent();

        lblParameterName.Text = parameter.ParameterName.TrimStart('@');
        pbParameter.Image = _activator.CoreIconProvider.GetImage(RDMPConcept.ParametersNode).ImageToBitmap();

        tbValue.Text = parameter.Value;

        //move the text box to the right of the parameter name but make sure it is minimum visible
        tbValue.Left = Math.Min(Width - tbValue.Width,lblParameterName.Right);
    }

    public void SetValueTo(ExtractionFilterParameterSet set)
    {
        if (set == null)
            return;

        var correctValue = set.GetAllParameters().FirstOrDefault(p=>p.ParameterName.Equals(_parameter.ParameterName));

        if(correctValue == null)
        {
            tbValue.Text = "";
            return;
        }

        tbValue.Text = correctValue.Value;
    }

    public void HandleSettingParameters(IFilter filter)
    {
        //rename operations can have happened
        var parameterToSet = filter.GetAllParameters().Single(p => p.ParameterName.StartsWith(_parameter.ParameterName));
        parameterToSet.Value = tbValue.Text;
        parameterToSet.SaveToDatabase();
    }

    private void tbValue_TextChanged(object sender, EventArgs e)
    {

    }
}