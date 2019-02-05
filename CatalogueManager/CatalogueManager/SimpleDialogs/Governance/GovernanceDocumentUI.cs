using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using CatalogueLibrary.Data.Governance;
using CatalogueManager.Collections;
using CatalogueManager.Rules;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode;
using ReusableUIComponents;
using ReusableUIComponents.Dialogs;

namespace CatalogueManager.SimpleDialogs.Governance
{
    /// <summary>
    /// The RDMP is designed to store sensitive clinical datasets and make them available in research ready (anonymous) form.  This usually requires governance approval from the data
    /// provider.  It is important to store the document trail and schedule (e.g. do you require yearly re-approval) for audit purposes.  The RDMP does this through Governance Periods
    /// (See GovernancePeriodUI).  
    /// 
    /// <para>This control allows you to configure/view attachments of a GovernancePeriod (e.g. an email, a scan of a signed approval letter etc). For ease of reference you should describe
    /// what is in the document (e.g. 'letter to Fife healthboard (Mary Sue) listing the datasets we host and requesting re-approval for 2016.  Letter is signed by Dr Governancer.)'</para>
    /// </summary>
    public partial class GovernanceDocumentUI : GovernanceDocumentUI_Design, ISaveableUI
    {
        public GovernanceDocumentUI()
        {
            InitializeComponent();
            AssociatedCollection = RDMPCollection.Catalogue;
        }

        protected override void SetBindings(BinderWithErrorProviderFactory rules, GovernanceDocument databaseObject)
        {
            base.SetBindings(rules, databaseObject);

            Bind(tbID, "Text", "ID", g=>g.ID);
            Bind(tbName, "Text", "Name", g => g.Name);
            Bind(tbDescription, "Text", "Description", g => g.Description);
            Bind(tbPath, "Text", "URL", g => g.URL);
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                tbPath.Text = ofd.FileName;
            }
        }
        
        private void btnOpenContainingFolder_Click(object sender, EventArgs e)
        {
            try
            {
                UsefulStuff.GetInstance().ShowFileInWindowsExplorer(new FileInfo(tbPath.Text));
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<GovernanceDocumentUI_Design, UserControl>))]
    public abstract class GovernanceDocumentUI_Design : RDMPSingleDatabaseObjectControl<GovernanceDocument>
    {
    }
}
