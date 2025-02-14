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
using Rdmp.Core;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Rules;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs;

/// <summary>
/// Restricts the times of day in which caching can take place (e.g. from midnight-4am only).  For a description of what caching is see CacheProgressUI or the RDMP user manual.  Format is
/// in standard TimeSpan.TryParse format (see https://msdn.microsoft.com/en-us/library/3z48198e(v=vs.110).aspx or search online for 'TimeSpan.TryParse c#') each TimeSpan can be followed by
/// a comma and then another TimeSpan format e.g.  '10:20:00-10:40:00,11:20:00-11:40:00' would create a permission window that could download from the cache between 10:20 AM and 10:40 AM then
/// caching wouldn't be allowed again until 11:20am to 11:40am.
/// </summary>
public partial class PermissionWindowUI : PermissionWindowUI_Design, ISaveableUI
{
    private IPermissionWindow _permissionWindow;

    public PermissionWindowUI()
    {
        InitializeComponent();
        AssociatedCollection = RDMPCollection.DataLoad;
    }

    public override void SetDatabaseObject(IActivateItems activator, PermissionWindow databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        _permissionWindow = databaseObject;

        var periods = _permissionWindow.PermissionWindowPeriods;
        var periodsByDay = new Dictionary<int, List<PermissionWindowPeriod>>();
        foreach (var period in periods)
        {
            if (!periodsByDay.ContainsKey(period.DayOfWeek))
                periodsByDay.Add(period.DayOfWeek, new List<PermissionWindowPeriod>());

            periodsByDay[period.DayOfWeek].Add(period);
        }

        var textBoxes = new[] { tbSunday, tbMonday, tbTuesday, tbWednesday, tbThursday, tbFriday, tbSaturday };
        for (var i = 0; i < 7; ++i)
            PopulatePeriodTextBoxForDay(textBoxes[i], i, periodsByDay);

        CommonFunctionality.AddHelp(tbMonday, "IPermissionWindow.PermissionWindowPeriods");
    }

    protected override void SetBindings(BinderWithErrorProviderFactory rules, PermissionWindow databaseObject)
    {
        base.SetBindings(rules, databaseObject);

        Bind(tbName, "Text", "Name", w => w.Name);
        Bind(tbDescription, "Text", "Description", w => w.Description);
        Bind(tbID, "Text", "ID", w => w.ID);
    }

    private static void PopulatePeriodTextBoxForDay(TextBox textBox, int dayNum,
        Dictionary<int, List<PermissionWindowPeriod>> periodsByDay)
    {
        if (periodsByDay.TryGetValue(dayNum, out var value)) PopulateTextBox(textBox, value);
    }

    private static void PopulateTextBox(TextBox textBox, IEnumerable<PermissionWindowPeriod> periods)
    {
        textBox.Text = string.Join(",", periods.Select(period => period.ToString()));
    }

    private static List<PermissionWindowPeriod> CreatePeriodListFromTextBox(int dayOfWeek, TextBox textBox)
    {
        var listString = textBox.Text;
        var periodList = new List<PermissionWindowPeriod>();
        foreach (var periodString in listString.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = periodString.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
            if (!TimeSpan.TryParse(parts[0], out var start))
                throw new Exception($"Could not parse {parts[0]} as a TimeSpan");

            if (!TimeSpan.TryParse(parts[1], out var end))
                throw new Exception($"Could not parse {parts[1]} as a TimeSpan");

            periodList.Add(new PermissionWindowPeriod(dayOfWeek, start, end));
        }

        return periodList;
    }

    public void RebuildPermissionWindowPeriods()
    {
        var periodList = new List<PermissionWindowPeriod>();
        periodList.AddRange(CreatePeriodListFromTextBox(0, tbSunday));
        periodList.AddRange(CreatePeriodListFromTextBox(1, tbMonday));
        periodList.AddRange(CreatePeriodListFromTextBox(2, tbTuesday));
        periodList.AddRange(CreatePeriodListFromTextBox(3, tbWednesday));
        periodList.AddRange(CreatePeriodListFromTextBox(4, tbThursday));
        periodList.AddRange(CreatePeriodListFromTextBox(5, tbFriday));
        periodList.AddRange(CreatePeriodListFromTextBox(6, tbSaturday));

        _permissionWindow.SetPermissionWindowPeriods(periodList);
    }

    private void tbDay_TextChanged(object sender, EventArgs e)
    {
        ragSmiley1.Reset();
        try
        {
            RebuildPermissionWindowPeriods();
        }
        catch (Exception exception)
        {
            ragSmiley1.Fatal(exception);
        }
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<PermissionWindowUI_Design, UserControl>))]
public abstract class PermissionWindowUI_Design : RDMPSingleDatabaseObjectControl<PermissionWindow>;