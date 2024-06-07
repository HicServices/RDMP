// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.ReusableLibraryCode.Settings;

namespace Rdmp.UI;

/// <summary>
/// Maintains lists of recently typed things into text boxes etc, use HostControl to have this class setup all the autocomplete and monitor .Leave events for self population
/// Once you call HostControl then that is you done, this class does the rest.
/// </summary>
public class RecentHistoryOfControls
{
    private readonly Guid _controlGuid;
    private readonly HashSet<string> _rvContents;
    private readonly List<string> _recentValues;

    public RecentHistoryOfControls(TextBox c, Guid controlGuid) : this(controlGuid)
    {
        var values = new AutoCompleteStringCollection();
        values.AddRange(_recentValues.ToArray());

        c.AutoCompleteCustomSource = values;
        c.AutoCompleteSource = AutoCompleteSource.CustomSource;
        c.AutoCompleteMode = AutoCompleteMode.Suggest;
        c.Leave += (sender, args) => AddResult(c.Text);
    }

    public RecentHistoryOfControls(ComboBox c, Guid controlGuid) : this(controlGuid)
    {
        var values = new AutoCompleteStringCollection();
        values.AddRange(_recentValues.ToArray());

        c.AutoCompleteCustomSource = values;

        if (c.DropDownStyle == ComboBoxStyle.DropDownList)
        {
            c.SelectedIndexChanged += (sender, args) => AddResult(c.Text);
        }
        else
        {
            c.AutoCompleteMode = AutoCompleteMode.Suggest;
            c.AutoCompleteSource = AutoCompleteSource.CustomSource;
            c.Leave += (sender, args) => AddResult(c.Text);
        }
    }

    private RecentHistoryOfControls(Guid controlGuid)
    {
        _controlGuid = controlGuid;
        _recentValues = new List<string>(UserSettings.GetHistoryForControl(controlGuid));
        _rvContents = new HashSet<string>(_recentValues);
    }

    public void AddResult(string value, bool save = true)
    {
        // bump it to the top
        if (!_rvContents.Add(value))
            _recentValues.Remove(value);
        _recentValues.Add(value);
        if (save)
            Save();
    }

    public void Clear()
    {
        //clear the selected key only
        _recentValues.Clear();
        Save();
    }

    private void Save()
    {
        UserSettings.SetHistoryForControl(_controlGuid, _recentValues);
    }

    public static void SetValueToMostRecentlySavedValue(TextBox c)
    {
        if (c.AutoCompleteCustomSource.Count > 0)
            c.Text = c.AutoCompleteCustomSource[^1]; //set the current text to the last used text
    }

    public static void SetValueToMostRecentlySavedValue(ComboBox c)
    {
        if (c.AutoCompleteCustomSource.Count > 0)
            c.Text = c.AutoCompleteCustomSource[^1]; //set the current text to the last used text
    }

    public static void AddHistoryAsItemsToComboBox(ComboBox c)
    {
        if (c.AutoCompleteCustomSource.Count <= 0) return;
        var items = new string[c.AutoCompleteCustomSource.Count];
        c.AutoCompleteCustomSource.CopyTo(items, 0);
        c.Items.AddRange(items.Cast<object>().ToArray());
    }
}