using System;
using CatalogueLibrary.Ticketing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;
using ReusableUIComponents.Dialogs;
using Color = System.Drawing.Color;

namespace CatalogueManager.LocationsMenu.Ticketing
{
    /// <summary>
    /// This control lets you reference a ticket in your ticketing system (e.g. JIRA, fogbugz etc).  The control has a location for you to record the ticket identifier (e.g. LINK-123).
    /// If you don't yet have a ticketing system configured (and you have a plugin that supports the ticketing system) then you can set up the ticketing system configuration by launching
    /// TicketingSystemConfigurationUI (from Catalogue Manager main menu).
    /// 
    /// <para>Assuming your ticketing system plugin is working correctly and correctly configured in RDMP then clicking 'Show' should take you directly to your ticketing system (e.g. launch a
    /// new browser window at the website page of the ticket).</para>
    /// </summary>
    public partial class TicketingControl : RDMPUserControl
    {
        private ITicketingSystem _ticketingSystemConfiguration;
        public event EventHandler TicketTextChanged;
        public bool IsValidTicketName { get; private set; }

        public string TicketText
        {
            get { return tbTicket.Text; }
            set { tbTicket.Text = value; }
        }

        public string Title
        {
            set
            {
                gbTicketing.Text = value;
            }
        }

        public TicketingControl()
        {
            InitializeComponent();
        }

        protected override void OnRepositoryLocatorAvailable()
        {
            base.OnRepositoryLocatorAvailable(); 
            
            
            ReCheckTicketingSystemInCatalogue();
        }


        public void ReCheckTicketingSystemInCatalogue()
        {
            ragSmiley1.SetVisible(false);
            
            try
            {
                if (VisualStudioDesignMode)
                    return;

                if(RepositoryLocator == null)
                    return;

                TicketingSystemFactory factory = new TicketingSystemFactory(RepositoryLocator.CatalogueRepository);
            
                _ticketingSystemConfiguration = factory.CreateIfExists(RepositoryLocator.CatalogueRepository.GetTicketingSystem());
                gbTicketing.Enabled = _ticketingSystemConfiguration != null;
            }
            catch (Exception exception)
            {
                ragSmiley1.SetVisible(true);
                ragSmiley1.Fatal(exception);
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
            if(h != null)
                h(sender, e);
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
}
