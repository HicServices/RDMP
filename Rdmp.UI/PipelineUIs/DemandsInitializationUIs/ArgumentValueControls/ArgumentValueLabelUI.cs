// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Windows.Forms;
using Rdmp.UI.ItemActivation;


namespace Rdmp.UI.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls;

/// <summary>
/// Normally IArgumentValueUIs allows you to specify the value of an IArugment (the database persistence value of a [DemandsInitialization] decorated Property on a MEF class e.g. a Pipeline components public property that the user can set)
/// 
/// <para>But in the case of this control the Type is not user editable but will be populated (hopefully) by the RDMP automatically e.g. CatalogueRepository.  In this case this control
/// will display to the user some information about why he cannot specify a value for the IArgument.</para>
/// </summary>
[TechnicalUI]
public sealed partial class ArgumentValueLabelUI : UserControl
{
    public ArgumentValueLabelUI(string readonlyText)
    {
        InitializeComponent();

        lbl.Text = readonlyText;
    }

    public void SetUp(IActivateItems activator, ArgumentValueUIArgs args)
    {
    }
}