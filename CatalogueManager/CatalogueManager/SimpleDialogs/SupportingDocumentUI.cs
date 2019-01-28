using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleControls;
using CatalogueManager.SimpleDialogs.Revertable;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableUIComponents;


namespace CatalogueManager.SimpleDialogs
{
    /// <summary>
    /// The RDMP allows you at attach both documents and auxiliary tables (SupportingSQLTable) to your datasets (Catalogue).  These artifacts are then available to data analysts who
    /// want to understand the dataset better.  Also if you tick IsExtractable then whenever the Catalogue is extracted the table/document is automatically copied and extracted into 
    /// project extraction directory for provision to the researcher.
    /// 
    /// <para>Enter the name, description and file path to the file you want attached to your dataset.  Make sure the path is on a network drive or otherwise available to all system users
    /// otherwise other data analysts will not be able to view the file.</para>
    /// 
    /// <para>Tick Extractable if you want a copy of the document to be automatically created whenever the dataset is extracted and supplied to a researcher as part of a project extraction.</para>
    /// 
    /// <para>If you tick IsGlobal then the table will be extracted regardless of what dataset is selected in a researchers data request (useful for global documents e.g. terms of use of 
    /// data).</para>
    /// 
    /// </summary>
    public partial class SupportingDocumentUI : SupportingDocumentUI_Design, ISaveableUI
    {
        private SupportingDocument _supportingDocument;
        
        public SupportingDocumentUI()
        {
            InitializeComponent();

            ticketingControl1.TicketTextChanged += ticketingControl1_TicketTextChanged;
            AssociatedCollection = RDMPCollection.Catalogue;
        }

        public override void SetDatabaseObject(IActivateItems activator, SupportingDocument databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            SupportingDocument = databaseObject;
            objectSaverButton1.SetupFor(this,databaseObject,activator.RefreshBus);
        }
        
        protected SupportingDocument SupportingDocument
        {
            get { return _supportingDocument; }
            set {
                _supportingDocument = value;

                //populate various textboxes
                tbID.Text = "" + value.ID;
                tbDescription.Text = value.Description;
                tbName.Text = value.Name;
                cbExtractable.Checked = value.Extractable;
                cbIsGlobal.Checked = value.IsGlobal;
                ticketingControl1.TicketText = value.Ticket;

                tbUrl.Text = value.URL != null ? value.URL.AbsoluteUri : "";
            }
        }

        private void tbName_TextChanged(object sender, EventArgs e)
        {

            if(string.IsNullOrWhiteSpace(tbName.Text))
            {
                tbName.Text = "No Name";
                tbName.SelectAll();
            }

             SetStringPropertyOn(tbName, "Name", SupportingDocument);
            
        }
        private void tbDescription_TextChanged(object sender, EventArgs e)
        {
            SetStringPropertyOn(tbDescription, "Description", SupportingDocument);
        }

        private void tbUrl_TextChanged(object sender, EventArgs e)
        {
            SetUriPropertyOn(tbUrl, "URL", SupportingDocument);
        }


        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (SupportingDocument != null)
                try
                {
                    Process.Start(SupportingDocument.URL.ToString());
                }
                catch (Exception ex)
                {

                    MessageBox.Show("unable to open file:" + ex.Message);
                }
                
        }
        

        private void SetUriPropertyOn( TextBox tb,string propertyToSet,object toSetOn)
        {
            if (toSetOn != null)
            {
                try
                {
                    Uri u = new Uri(tb.Text);

                    tb.ForeColor = Color.Black;

                    PropertyInfo target = toSetOn.GetType().GetProperty(propertyToSet);
                    FieldInfo targetMaxLength = toSetOn.GetType().GetField(propertyToSet + "_MaxLength");

                    if (target == null || targetMaxLength == null)
                        throw new Exception("Could not find property " + propertyToSet + " or it did not have a specified _MaxLength");

                    if (tb.TextLength > (int)targetMaxLength.GetValue(toSetOn))
                        throw new UriFormatException("Uri is too long to fit in database");

                    target.SetValue(toSetOn, new Uri(tb.Text), null);
                    tb.ForeColor = Color.Black;
                }
                catch (UriFormatException)
                {
                    tb.ForeColor = Color.Red;
                }
            }
        }

        private void SetStringPropertyOn(TextBox tb, string property, object toSetOn)
        {
            if (toSetOn != null)
            {
                PropertyInfo target = toSetOn.GetType().GetProperty(property);
                FieldInfo targetMaxLength = toSetOn.GetType().GetField(property + "_MaxLength");
                
                if (target == null || targetMaxLength == null)
                    throw new Exception("Could not find property " + property + " or it did not have a specified _MaxLength");

                if (tb.TextLength > (int)targetMaxLength.GetValue(toSetOn))
                    tb.ForeColor = Color.Red;
                else
                {
                    target.SetValue(toSetOn, tb.Text, null);
                    tb.ForeColor = Color.Black;
                }
            }
        }
        private void cbExtractable_CheckedChanged(object sender, EventArgs e)
        {
            if (SupportingDocument != null)
                SupportingDocument.Extractable = cbExtractable.Checked;
        }
        private void cbIsGlobal_CheckedChanged(object sender, EventArgs e)
        {
            if (SupportingDocument != null)
                SupportingDocument.IsGlobal = cbIsGlobal.Checked;
        }
        
        void ticketingControl1_TicketTextChanged(object sender, EventArgs e)
        {
            if (_supportingDocument != null)
                _supportingDocument.Ticket = ticketingControl1.TicketText;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            if (ofd.ShowDialog() == DialogResult.OK)
                tbUrl.Text = ofd.FileName;
        }

        public ObjectSaverButton GetObjectSaverButton()
        {
            return objectSaverButton1;
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<SupportingDocumentUI_Design, UserControl>))]
    public abstract class SupportingDocumentUI_Design : RDMPSingleDatabaseObjectControl<SupportingDocument>
    {
        
    }
}
