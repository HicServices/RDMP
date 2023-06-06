// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Performance;
using Rdmp.UI.Performance;


namespace Rdmp.UI.SimpleDialogs;

/// <summary>
/// This form is mainly used for diagnostic purposes and lets you track every SQL query sent to the RDMP Data Catalogue and Data Export Manager databases.  This is useful for diagnosing
/// the problem with sluggish user interfaces.  Once you select 'Start Command Auditing' it will record each unique SQL query sent to either database and the number of times it is sent
/// including a StackTrace for the location in the RMDP software which the query was issued from.
/// </summary>
public partial class PerformanceCounterUI : Form
{
    public PerformanceCounterUI()
    {
        InitializeComponent();
        timer1.Start();
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
        if (DatabaseCommandHelper.PerformanceCounter == null)
        {
            lblCommandsAudited.Text = "Commands Audited:0";
        }
        else
        {
            var timesSeen = DatabaseCommandHelper.PerformanceCounter.DictionaryOfQueries.Values.Sum(s=>s.TimesSeen);

            lblCommandsAudited.Text =
                $"Commands Audited:{timesSeen} ({DatabaseCommandHelper.PerformanceCounter.DictionaryOfQueries.Keys.Count} distinct)";
        }
    }

    private void btnToggleCommandAuditing_Click(object sender, EventArgs e)
    {

        if(DatabaseCommandHelper.PerformanceCounter == null)
        {
            DatabaseCommandHelper.PerformanceCounter = new ComprehensiveQueryPerformanceCounter();
            btnToggleCommandAuditing.Text = "Stop Command Auditing";
            btnViewPerformanceResults.Enabled = true;
        }
        else
        {
            DatabaseCommandHelper.PerformanceCounter = null;
            btnToggleCommandAuditing.Text = "Start Command Auditing";
            btnViewPerformanceResults.Enabled = false;
        }
    }
        
    private void CatalogueLibraryPerformanceCounterUI_FormClosing(object sender, FormClosingEventArgs e)
    {
        //clear it before closing always
        DatabaseCommandHelper.PerformanceCounter = null;
    }

    private void btnViewPerformanceResults_Click(object sender, EventArgs e)
    {

        //if there aren't any results don't show
        if (DatabaseCommandHelper.PerformanceCounter == null || !DatabaseCommandHelper.PerformanceCounter.DictionaryOfQueries.Keys.Any())
            return;                

        var f = new Form();
        var ui = new PerformanceCounterResultsUI();
        ui.Dock = DockStyle.Fill;

        //remove the current counter while this UI is running (the UI is designed to be a snapshot not a realtime view
        var performanceCounter = DatabaseCommandHelper.PerformanceCounter;
        DatabaseCommandHelper.PerformanceCounter = null;

        ui.LoadState(performanceCounter);
        f.WindowState = FormWindowState.Maximized;
        f.Controls.Add(ui);

        TopMost = false;
        f.ShowDialog();
        TopMost = true;

        //now the viewer has been closed we can reinstantiate the performance counter
        DatabaseCommandHelper.PerformanceCounter = performanceCounter;
    }
}