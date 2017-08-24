using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Data;

namespace DataExportManager.SimpleDialogs
{
    /// <summary>
    /// As part of a data extraction, a ReleaseDocument is generated.  This is a Microsoft Word document which lists in tabular format the datasets released, the filters applied, the number
    /// of rows extracted, distinct patient identifiers etc.  This document can optionally include a statement about use of the data / accreditation or a disclaimer or whatever else message
    /// you want researchers to read.  
    /// 
    /// You can only have one message at a time and it is constant, we suggest something like "this data was supplied by blah, please accredit us and the NHS as the data provider... etc"
    /// </summary> 
    public partial class ConfigureDisclaimer : RDMPForm
    {
        private bool _allowClose = false;
        ConfigurationProperties config;

        public ConfigureDisclaimer()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            if(VisualStudioDesignMode)
                return;

             config = new ConfigurationProperties(false,RepositoryLocator.DataExportRepository);

            string value = config.TryGetValue(ConfigurationProperties.ExpectedProperties.ReleaseDocumentDisclaimer);

            tb.Text = value ?? GetDefaultText();

            _allowClose = true;

            base.OnLoad(e);
        }

        private string GetDefaultText()
        {
            return
           @"*****************************************************************************************************************************
Please acknowledge HIC as a data source in any publications/reports which contain results generated from our data.  We suggest adding the following:
We acknowledge the support of the Health Informatics Centre, University of Dundee for managing and supplying the anonymised data.
Once you have finished your project, please inform HIC who will make arrangements to recover and archive your project.
*****************************************************************************************************************************";
        }

        private void btnCancelChanges_Click(object sender, EventArgs e)
        {
            _allowClose = true;
            DialogResult = DialogResult.Cancel; 
            Close();
        }

        private void btnSaveAndClose_Click(object sender, EventArgs e)
        {
            config.SetValue(ConfigurationProperties.ExpectedProperties.ReleaseDocumentDisclaimer,tb.Text);
            _allowClose = true;
            DialogResult = DialogResult.OK;
            Close();
        }
        private void ConfigureDisclaimer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (VisualStudioDesignMode)
                return;

            if (!_allowClose)
                switch (MessageBox.Show("Save changes? Yes - save and close, No - discard changes and close, Cancel - Do not close", "Save Changes?", MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        break;
                    case DialogResult.Yes:
                        btnSaveAndClose_Click(null,null);
                        break;
                    case DialogResult.No:
                        btnCancelChanges_Click(null,null);
                        break;
                    default:
                        e.Cancel = true;
                        break;
                }

        }


        private void tb_TextChanged(object sender, EventArgs e)
        {
            _allowClose = false;
        }
    }
}
