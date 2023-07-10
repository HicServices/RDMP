// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Windows.Forms;
using Rdmp.UI.ItemActivation;


namespace Rdmp.UI.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls;

/// <summary>
/// Allows you to specify the value of an IArugment (the database persistence value of a [DemandsInitialization] decorated Property on a MEF class e.g. a Pipeline components public property that the user can set)
/// 
/// <para>This Control is for setting Properties that are of Type bool (true/false)</para>
/// </summary>
[TechnicalUI]
public partial class ArgumentValueBoolUI : UserControl, IArgumentValueUI
{
    private bool _bLoading = true;
    private ArgumentValueUIArgs _args;

    public ArgumentValueBoolUI()
    {
        InitializeComponent();
    }

    public void SetUp(IActivateItems activator, ArgumentValueUIArgs args)
    {
        _args = args;
        _bLoading = true;

        //if no value has been selected set it to false
        if (args.InitialValue == null)
        {
            cbValue.Checked = false;
            args.Setter(false);
        }
        else
            cbValue.Checked = (bool)args.InitialValue;//otherwise use the previous value

        _bLoading = false;
    }

    private void cbValue_CheckedChanged(object sender, System.EventArgs e)
    {
        if(_bLoading)
            return;

        _args.Setter(cbValue.Checked);
    }
}