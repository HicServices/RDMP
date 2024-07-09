// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using FluentFTP.Helpers;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataLoad.Engine.LoadProcess.Scheduling.Strategy;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.Ticketing;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.LocationsMenu.Ticketing;

/// <summary>
/// The RDMP recognises that there are a wide array of software systems for tracking time worked, issues,project requests, bug reports etc.  The RDMP is designed to support gated
/// interactions with ticketing systems (which can be skipped entirely if you do not want the functionality).  This window lets you configure which ticketing system you have, the
/// credentials needed to access it and where it is located.  You will need to make sure you select the appropriate Type of ticketing system you have.
/// 
/// <para>Because there are many different ticketing systems and they can often be configured in diverse ways, the RDMP uses a 'plugin' approach to interacting with ticketing systems.
/// The scope of functionality includes: </para>
/// 
/// <para>1. Validating whether a ticket is valid
/// 2. Navigating to the ticket when the user clicks 'Show' in a TicketingControlUI (See TicketingControlUI)
/// 3. Determining whether a given project extraction can go ahead (This lets you drive ethics/approvals process through your normal ticketing system but have RDMP prevent
/// releases of data until the ticketing system says its ok). </para>
/// 
/// <para>Ticketing systems are entirely optional and you can ignore them if you don't have one or don't want to configure it.  If you do not see a Type that corresponds with your
/// ticketing system you might need to write your own Ticketing dll (See ITicketingSystem interface) and upload it as a plugin to the Data Catalogue.</para>
/// </summary>
public partial class TicketingSystemConfigurationUI : RDMPUserControl
{
    private TicketingSystemConfiguration _ticketingSystemConfiguration;
    private const string NoneText = "<<NONE>>";

    public TicketingSystemConfigurationUI()
    {
        InitializeComponent();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (VisualStudioDesignMode)
            return;

        RefreshUIFromDatabase();
    }

    private bool _bLoading = true;

    private void RefreshUIFromDatabase()
    {
        _bLoading = true;

        var ticketing = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<TicketingSystemConfiguration>()
            .ToArray();

        if (ticketing.Length > 1)
            throw new Exception(
                "You have multiple TicketingSystemConfiguration configured, open the table TicketingSystemConfiguration and delete one of them");

        _ticketingSystemConfiguration = ticketing.SingleOrDefault();

        cbxType.Items.Clear();
        cbxType.Items.AddRange(MEF.GetTypes<ITicketingSystem>().Select(t => t.FullName).ToArray());

        ddCredentials.Items.Clear();
        ddCredentials.Items.Add(NoneText);
        ddCredentials.Items.AddRange(_activator.RepositoryLocator.CatalogueRepository
            .GetAllObjects<DataAccessCredentials>().ToArray());


        if (_ticketingSystemConfiguration == null)
        {
            gbTicketingSystem.Enabled = false;
            tbID.Text = "";
            tbName.Text = "";
            tbUrl.Text = "";
            cbxType.Text = "";
            cbDisabled.Checked = false;
            btnCreate.Enabled = true;
            btnDelete.Enabled = false;
        }
        else
        {
            gbTicketingSystem.Enabled = true;

            tbID.Text = _ticketingSystemConfiguration.ID.ToString();
            tbName.Text = _ticketingSystemConfiguration.Name;
            tbUrl.Text = _ticketingSystemConfiguration.Url;
            cbxType.Text = _ticketingSystemConfiguration.Type;
            cbDisabled.Checked = !_ticketingSystemConfiguration.IsActive;

            ddCredentials.Text = _ticketingSystemConfiguration.DataAccessCredentials_ID != null
                ? _ticketingSystemConfiguration.DataAccessCredentials.ToString()
                : NoneText;

            btnCreate.Enabled = false;
            btnDelete.Enabled = true;
            btnSave.Enabled = false;
            tbReleases.Text = string.Join(',', _ticketingSystemConfiguration.GetReleaseStatuses().Select(s => s.Status).ToList());
        }

        _bLoading = false;
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        _ticketingSystemConfiguration.SaveToDatabase();

        var releases = tbReleases.Text.Split(',');
        var existingReleases = _activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<TicketingSystemReleaseStatus>("TicketingSystemConfigurationID", _ticketingSystemConfiguration.ID);
        var toDelete = existingReleases.Where(s => !releases.Contains(s.Status)).ToList();
        foreach (var release in releases.Where(rs => rs != "" && !existingReleases.Select(er => er.Status).Contains(rs)))
        {
            var rs = new TicketingSystemReleaseStatus(_activator.RepositoryLocator.CatalogueRepository, release.Trim(), null, _ticketingSystemConfiguration);
            rs.SaveToDatabase();
        }
        toDelete.ForEach(rs => rs.DeleteInDatabase());
        btnSave.Enabled = false;
        RefreshUIFromDatabase();
    }

    private void btnCreate_Click(object sender, EventArgs e)
    {
        new TicketingSystemConfiguration(_activator.RepositoryLocator.CatalogueRepository, "New Ticketing System");
        RefreshUIFromDatabase();
    }

    private void btnDelete_Click(object sender, EventArgs e)
    {
        if (Activator.YesNo(
                "Are you sure you want to delete the Ticketing system from this Catalogue database? there can be only one so be sure before you delete it.",
                "Confirm deleting Ticketing system"))
        {
            _ticketingSystemConfiguration.DeleteInDatabase();
            RefreshUIFromDatabase();
        }
    }

    private void btnCheck_Click(object sender, EventArgs e)
    {
        if (btnSave.Enabled)
            btnSave_Click(null, null);

        ITicketingSystem instance;
        try
        {
            var factory = new TicketingSystemFactory(_activator.RepositoryLocator.CatalogueRepository);
            instance = factory.CreateIfExists(_ticketingSystemConfiguration);

            if (instance != null)
            {
                var knownStatuses = instance.GetAvailableStatuses();
                var requestedStatuses = tbReleases.Text.Split(',').Where(s => s.Trim() != "");
                if (!requestedStatuses.Any()) checksUI1.OnCheckPerformed(new CheckEventArgs($"No Release status set", CheckResult.Fail));

                foreach (var status in requestedStatuses.Where(s => !knownStatuses.Contains(s.Trim())))
                {
                    checksUI1.OnCheckPerformed(new CheckEventArgs($"{status} is not a known status within the ticketing system", CheckResult.Fail));
                }
            }

            checksUI1.OnCheckPerformed(
                new CheckEventArgs($"successfully created a instance of {instance.GetType().FullName}",
                    CheckResult.Success));
        }
        catch (Exception exception)
        {
            checksUI1.OnCheckPerformed(
                new CheckEventArgs("Could not create ticketing system from your current configuration",
                    CheckResult.Fail, exception));
            return;
        }

        checksUI1.StartChecking(instance);
    }

    private void btnEditCredentials_Click(object sender, EventArgs e)
    {
        if (ddCredentials.SelectedItem is DataAccessCredentials creds)
            _activator.CommandExecutionFactory.Activate(creds);
    }

    private void btnAddCredentials_Click(object sender, EventArgs e)
    {
        new DataAccessCredentials(_activator.RepositoryLocator.CatalogueRepository, "New Data Access Credentials");
        RefreshUIFromDatabase();
    }

    private void btnDeleteCredentials_Click(object sender, EventArgs e)
    {
        try
        {
            if (_ticketingSystemConfiguration.DataAccessCredentials_ID != null)
            {
                var toDelete = _ticketingSystemConfiguration.DataAccessCredentials;

                if (Activator.YesNo($"Confirm deleting Encrypted Credentials {toDelete.Name}?", "Confirm delete?"))
                    toDelete.DeleteInDatabase();
            }
        }
        catch (Exception ex)
        {
            ExceptionViewer.Show(ex);
        }

        RefreshUIFromDatabase();
    }

    private void tReleases_TextChanged(object sender, EventArgs e)
    {
        btnSave.Enabled = true;
    }

    private void tb_TextChanged(object sender, EventArgs e)
    {
        if (_bLoading)
            return;

        _ticketingSystemConfiguration.Name = tbName.Text;
        _ticketingSystemConfiguration.Url = tbUrl.Text;
        _ticketingSystemConfiguration.Type = cbxType.Text;
        btnSave.Enabled = true;
    }

    private void ddCredentials_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_bLoading)
            return;


        if (ddCredentials.SelectedItem is not DataAccessCredentials creds)
        {
            _ticketingSystemConfiguration.DataAccessCredentials_ID = null;
            _ticketingSystemConfiguration.SaveToDatabase();
        }
        else
        {
            _ticketingSystemConfiguration.DataAccessCredentials_ID = creds.ID;
            _ticketingSystemConfiguration.SaveToDatabase();
        }
    }

    private IActivateItems _activator;

    public override void SetItemActivator(IActivateItems activator)
    {
        base.SetItemActivator(activator);
        _activator = activator;
    }

    private void cbDisabled_CheckedChanged(object sender, EventArgs e)
    {
        if (_ticketingSystemConfiguration != null)
        {
            _ticketingSystemConfiguration.IsActive = !cbDisabled.Checked;
            _ticketingSystemConfiguration.SaveToDatabase();
        }
    }

    private void label5_Click(object sender, EventArgs e)
    {

    }
}