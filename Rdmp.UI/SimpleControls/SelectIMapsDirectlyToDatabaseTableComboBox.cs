// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.SimpleControls;

public partial class SelectIMapsDirectlyToDatabaseTableComboBox : UserControl
{
    private List<IMapsDirectlyToDatabaseTable> _available;
    private bool _settingUp;
    public event EventHandler<EventArgs> SelectedItemChanged;
    private IActivateItems _activator;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IMapsDirectlyToDatabaseTable SelectedItem
    {
        get => suggestComboBox1.SelectedItem as IMapsDirectlyToDatabaseTable;
        set
        {
            if (value != null && !_available.Contains(value))
            {
                _available.Add(value);
                SetUp(_available);
            }

            //avoids circular event calls
            if (!Equals(suggestComboBox1.SelectedItem, value))
            {
                if (value != null)
                    suggestComboBox1.SelectedItem = value;
                else
                    suggestComboBox1.SelectedIndex = -1;

                suggestComboBox1_SelectedIndexChanged(this, EventArgs.Empty);
            }
        }
    }

    public void SetItemActivator(IActivateItems activator)
    {
        _activator = activator;
    }

    public SelectIMapsDirectlyToDatabaseTableComboBox()
    {
        InitializeComponent();

        suggestComboBox1.PropertySelector = s => s.Cast<object>().Select(o => o == null ? "<None>>" : o.ToString());
        suggestComboBox1.SelectedIndexChanged += suggestComboBox1_SelectedIndexChanged;
    }

    private void suggestComboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (!_settingUp) SelectedItemChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetUp(IEnumerable<IMapsDirectlyToDatabaseTable> available)
    {
        _settingUp = true;
        _available = available.ToList();

        try
        {
            var before = suggestComboBox1.SelectedIndex;
            suggestComboBox1.DataSource = available;

            //if it was clear before don't take item 0
            if (before == -1)
                suggestComboBox1.SelectedIndex = -1;
        }
        finally
        {
            _settingUp = false;
        }
    }

    private void lPick_Click(object sender, EventArgs e)
    {
        if (_activator.SelectObject(new DialogArgs
        {
            WindowTitle = "Select New Value"
        }, _available.ToArray(), out var selected))
            suggestComboBox1.SelectedItem = selected;
    }

    private void suggestComboBox1_TextUpdate(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(suggestComboBox1.Text))
            suggestComboBox1.SelectedIndex = -1;
    }
}