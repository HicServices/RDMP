// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls;

/// <summary>
/// Allows you to specify the value of an IArugment (the database persistence value of a [DemandsInitialization] decorated Property on a MEF class e.g. a Pipeline components public property that the user can set)
/// 
/// <para>This Control is for setting Properties that are of of a known collection type e.g. TableInfo (from all TableInfos in a dle configuration).</para>
/// </summary>
[TechnicalUI]
public partial class ArgumentValueComboBoxUI : UserControl, IArgumentValueUI
{
    private readonly IActivateItems _activator;
    private readonly object[] _objectsForComboBox;
    private bool _bLoading = true;

    private const string ClearSelection = "<<Clear Selection>>";

    private HashSet<Type> types;
    private ArgumentValueUIArgs _args;
    private bool _isEnum;

    public ArgumentValueComboBoxUI(IActivateItems activator, object[] objectsForComboBox)
    {
        _activator = activator;
        _objectsForComboBox = objectsForComboBox;
        InitializeComponent();

        //Stop mouse wheel scroll from scrolling the combobox when it's closed to avoid the value being changed without user noticing.
        RDMPControlCommonFunctionality.DisableMouseWheel(cbxValue);

        if (objectsForComboBox == null || objectsForComboBox.Length == 0)
            return;

        btnPick.Enabled = objectsForComboBox.All(o => o is IMapsDirectlyToDatabaseTable);

        //If it is a dropdown of Types
        if (objectsForComboBox.All(o => o is Type))
        {
            //add only the names (not the full namespace)
            types = new HashSet<Type>(objectsForComboBox.Cast<Type>());

            cbxValue.DropDownStyle = ComboBoxStyle.DropDownList;
            cbxValue.Items.AddRange(types.Select(t => t.Name).ToArray());
            cbxValue.Items.Add(ClearSelection);
        }
        else if (objectsForComboBox.All(t => t is Enum)) //don't offer "ClearSelection" if it is an Enum list
        {
            _isEnum = true;
            cbxValue.DataSource = objectsForComboBox;
            cbxValue.DropDownStyle = ComboBoxStyle.DropDownList;
        }
        else
        {
            cbxValue.DropDownStyle = ComboBoxStyle.DropDownList;

            cbxValue.DropDown += (s, e) => { LateLoad(objectsForComboBox); };
        }
    }

    private bool haveLateLoaded = false;

    private void LateLoad(object[] objectsForComboBox)
    {
        if (haveLateLoaded) return;

        cbxValue.BeginUpdate();
        cbxValue.Items.AddRange(objectsForComboBox.Except(cbxValue.Items.Cast<object>()).ToArray());
        cbxValue.Items.Add(ClearSelection);
        cbxValue.EndUpdate();

        haveLateLoaded = true;
    }

    public void SetUp(IActivateItems activator, ArgumentValueUIArgs args)
    {
        _bLoading = true;
        _args = args;

        object currentValue = null;

        try
        {
            if (_isEnum && _args.InitialValue == null)
                args.Setter(cbxValue.SelectedItem);
            else
                currentValue = _args.InitialValue;
        }
        catch (Exception e)
        {
            _args.Fatal(e);
        }

        if (cbxValue.Items.Count == 0 && _args.InitialValue != null) cbxValue.Items.Add(_args.InitialValue);

        if (currentValue != null) cbxValue.Text = types != null ? ((Type)currentValue).Name : currentValue.ToString();

        _bLoading = false;
    }

    private void cbxValue_TextChanged(object sender, EventArgs e)
    {
        if (_bLoading)
            return;

        //user chose to clear selection from a combo box
        if (cbxValue.Text == ClearSelection)
            _args.Setter(null);
        else if (cbxValue.SelectedItem != null)
            _args.Setter(
                types != null ? types.Single(t => t.Name.Equals(cbxValue.SelectedItem)) : cbxValue.SelectedItem);
    }

    private void btnPick_Click(object sender, EventArgs e)
    {
        if (_activator.SelectObject(new DialogArgs
        {
            TaskDescription = $"Choose a new value for '{_args.Required.Name}'",
            AllowSelectingNull = true
        }, _objectsForComboBox, out var selected))
        {
            if (selected == null)
            {
                cbxValue.Text = ClearSelection;
            }
            else
            {
                if (!cbxValue.Items.Contains(selected)) cbxValue.Items.Add(selected);

                cbxValue.SelectedItem = selected;
            }
        }
    }
}