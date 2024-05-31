// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Ticketing;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using Color = System.Drawing.Color;

namespace Rdmp.UI.LocationsMenu.Versioning;

/// <summary>
/// This control lets you reference a ticket in your ticketing system (e.g. JIRA, fogbugz etc).  The control has a location for you to record the ticket identifier (e.g. LINK-123).
/// If you don't yet have a ticketing system configured (and you have a plugin that supports the ticketing system) then you can set up the ticketing system configuration by launching
/// TicketingSystemConfigurationUI (from Catalogue Manager main menu).
/// 
/// <para>Assuming your ticketing system plugin is working correctly and correctly configured in RDMP then clicking 'Show' should take you directly to your ticketing system (e.g. launch a
/// new browser window at the website page of the ticket).</para>
/// </summary>
public partial class VersioningControlUI : RDMPUserControl
{
    private ITicketingSystem _ticketingSystemConfiguration;
    public event EventHandler TicketTextChanged;
    public bool IsValidTicketName { get; private set; }

    public string TicketText
    {
        get => tbTicket.Text;
        set => tbTicket.Text = value;
    }

    public string Title
    {
        set => gbTicketing.Text = value;
    }

    public VersioningControlUI()
    {
        InitializeComponent();
    }

    public override void SetItemActivator(IActivateItems activator)
    {
        base.SetItemActivator(activator);

        ReCheckTicketingSystemInCatalogue();
    }

    public void ReCheckTicketingSystemInCatalogue()
    {

        try
        {
            if (VisualStudioDesignMode)
                return;

            if (Activator == null)
                throw new Exception("Activator has not been set, call SetItemActivator");

            var factory = new TicketingSystemFactory(Activator.RepositoryLocator.CatalogueRepository);

            var configuration = Activator.RepositoryLocator.CatalogueRepository.GetTicketingSystem();
            _ticketingSystemConfiguration = factory.CreateIfExists(configuration);

            gbTicketing.Enabled = _ticketingSystemConfiguration != null;
        }
        catch (Exception)
        {

        }
    }


    private void tbTicket_TextChanged(object sender, EventArgs e)
    {
        if (_ticketingSystemConfiguration != null)
        {
            IsValidTicketName = _ticketingSystemConfiguration.IsValidTicketName(tbTicket.Text);
            tbTicket.ForeColor = IsValidTicketName ? Color.Black : Color.Red;
        }

        var h = TicketTextChanged;
        h?.Invoke(sender, e);
    }

    private void btnShowTicket_Click(object sender, EventArgs e)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(tbTicket.Text) && IsValidTicketName)
                _ticketingSystemConfiguration.NavigateToTicket(tbTicket.Text);
        }
        catch (Exception exception)
        {
            ExceptionViewer.Show(exception);
        }
    }
}