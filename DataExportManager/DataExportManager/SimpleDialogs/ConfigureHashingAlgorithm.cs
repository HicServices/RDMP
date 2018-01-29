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
using MapsDirectlyToDatabaseTable;
using RDMPObjectVisualisation.Copying;
using ReusableLibraryCode;
using ReusableUIComponents;
using ReusableUIComponents.ScintillaHelper;
using ScintillaNET;

namespace DataExportManager.SimpleDialogs
{
    /// <summary>
    /// Any column in a data extraction which is marked with 'Hash On Data Release' (See ExtractionInformationUI) will be wrapped with this SQL string.  Use this to call a scalar valued
    /// function which generates hash strings based on the column value and the project number (salt).
    /// 
    /// For example Work.dbo.HicHash({0},{1}) would wrap column names such that the column name appeared in the {0} and the project number appeared in {1}.  For this to work you must have
    /// a database Work and a scalar function called HicHash (this is just an example, you can call the function whatever you want and adjust it accordingly).  You don't have to use the
    /// salt if you don't want to either, if you don't add a {1} then you won't get a salt argument into your scalar function.
    /// 
    /// This is quite technical if you don't know what a Scalar Function is in SQL then you probably don't want to do hashing and instead you might want to just not extract these columns
    /// or configure them with the RDMP ANO system (See ConfigureANOForTableInfo).
    /// </summary>
    public partial class ConfigureHashingAlgorithm : RDMPForm
    {
        [DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
        public Scintilla QueryPreview { get; set; }

        ConfigurationProperties _configurationProperties;

        public ConfigureHashingAlgorithm()
        {
            InitializeComponent();
            
            if(VisualStudioDesignMode)
                return;

            QueryPreview = new ScintillaTextEditorFactory().Create(new RDMPCommandFactory());
            QueryPreview.ReadOnly = true;

            panel2.Controls.Add(QueryPreview);

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(VisualStudioDesignMode)
                return;

            //get the current hashing algorithm
            _configurationProperties = new ConfigurationProperties(false,RepositoryLocator.DataExportRepository);

            string value = _configurationProperties.TryGetValue(ConfigurationProperties.ExpectedProperties.HashingAlgorithmPattern);
            tbHashingAlgorithm.Text = value;
        }

        private void tbHashingAlgorithm_TextChanged(object sender, EventArgs e)
        {
            string pattern = tbHashingAlgorithm.Text;

            try
            {
                QueryPreview.ReadOnly = false;
                QueryPreview.Text = String.Format(pattern, "[TEST]..[ExampleColumn]", "123");
                _configurationProperties.SetValue(ConfigurationProperties.ExpectedProperties.HashingAlgorithmPattern, pattern);
                
            }
            catch (Exception exception)
            {
                QueryPreview.Text = ExceptionHelper.ExceptionToListOfInnerMessages(exception);

            }
            finally
            {
                QueryPreview.ReadOnly = true;
            }
        }

        private void btnReferenceColumn_Click(object sender, EventArgs e)
        {
            tbHashingAlgorithm.Text = tbHashingAlgorithm.Text + "{0}";
        }

        private void btnReferenceSalt_Click(object sender, EventArgs e)
        {
            tbHashingAlgorithm.Text = tbHashingAlgorithm.Text + "{1}";
        }
    }
}
