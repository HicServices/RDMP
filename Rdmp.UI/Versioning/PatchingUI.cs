// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FAnsi.Discovery;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.ReusableLibraryCode.Checks;


namespace Rdmp.UI.Versioning;

/// <summary>
/// This window appears whenever the RDMP has detected that there is a version mismatch between your database and the RDMP software (or a plugin you have written - See <see cref="PluginPatcher"/>).
/// The RDMP enforces a strict version policy in which the version of the codebase (the software running) must always match that of the databases it is running on.  Each new version of
/// the RDMP software will include SQL patches designed to bring your databases up-to-date with the new feature set.
/// 
/// <para>This dialog shows you the version number of the database and the version number of the patching assembly (separate version numbers are maintained for the Catalogue Manager Database,
/// Data Export Database, Logging databases etc).</para>
/// </summary>
public partial class PatchingUI : Form
{
    private readonly DiscoveredDatabase _database;
    private readonly ITableRepository _repository;

    private IPatcher _patcher;

    private PatchingUI(DiscoveredDatabase database, ITableRepository repository, IPatcher patcher)
    {
        _database = database;
        _repository = repository;
        _patcher = patcher;
        InitializeComponent();
        this.btnAttemptPatching.Enabled = false;
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            return;

        var name = $"{patcher.Name} v{patcher.GetDbAssembly().GetName().Version.ToString(3)}";

        tbPatch.Text = $"{name}";

        if (_database == null)
        {
            tbDatabase.Text = "Form loaded without a specific database to target!";
            tbDatabase.ForeColor = Color.Red;
        }
        else
        {
            tbDatabase.Text = $"{_database.GetRuntimeName()}, Version:{repository.GetVersion()}";
        }
        btnAttemptPatching_Click(null, null);

    }

    private void btnAttemptPatching_Click(object sender, EventArgs e)
    {
        try
        {
            var toMem = new ToMemoryCheckNotifier(checksUI1);

            var mds = new MasterDatabaseScriptExecutor(_database);


            mds.PatchDatabase(_patcher, toMem, static _ => true, static () => false);

            //if it crashed during patching
            if (toMem.GetWorst() == CheckResult.Fail)
            {
                btnAttemptPatching.Enabled = true;
                return;
            }

            toMem.OnCheckPerformed(new CheckEventArgs(
                "Patching completed without exception, disabling the patching button", CheckResult.Success, null));
            //patching worked so prevent them doing it again!
            btnAttemptPatching.Enabled = false;

            if (_repository != null)
            {
                _repository.ClearUpdateCommandCache();
                checksUI1.OnCheckPerformed(new CheckEventArgs("Cleared UPDATE commands cache", CheckResult.Success,
                    null));
            }

            checksUI1.OnCheckPerformed(new CheckEventArgs("Patching Successful", CheckResult.Success, null));

            if (MessageBox.Show("Application will now restart", "Restart Application", MessageBoxButtons.OK) == DialogResult.OK)
                ApplicationRestarter.Restart();
        }
        catch (Exception exception)
        {
            checksUI1.OnCheckPerformed(new CheckEventArgs("Patching failed", CheckResult.Fail, exception));
        }
    }

    public static void ShowIfRequired(DiscoveredDatabase database, ITableRepository repository, IPatcher patcher)
    {
        if (Patch.IsPatchingRequired(database, patcher, out _, out _, out _) == Patch.PatchingState.Required)
            new PatchingUI(database, repository, patcher).ShowDialog();
    }
}