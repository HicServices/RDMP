using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode;
using ReusableUIComponents.ChecksUI;

namespace CatalogueManager.MainFormUITabs
{
    /// <summary>
    /// RDMP supports extracting all your metadata into DITA format (http://dita.xml.org/ - DITA OASIS Standard).  This is an XML standard with good tool support.  This form lets you
    /// export your entire metadata descriptive database into a collection of DITA files.  This might be useful to you for some reason (e.g. to produce offline PDFs etc) but really 
    /// the recommended route is to use the built in metadata reports (e.g. ConfigureMetadataReport).  Alternatively you can run queries directly on the RDMP Data Catalogue database
    /// which is a super relational database with many tables (Catalogue, CatalogueItem, SupportingDocument etc etc).
    /// 
    /// <para>NOTE: Make sure that you have set a Resource Acronym for each of your datasets (Catalogues) before attempting to extract in DITA format.</para>
    /// </summary>
    public partial class DitaExtractorUI : RDMPUserControl
    {
        public DitaExtractorUI()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog ofd = new FolderBrowserDialog();
            
            
            DialogResult d = ofd.ShowDialog();

            if (d == DialogResult.OK || d == DialogResult.Yes)
                tbExtractionDirectory.Text = ofd.SelectedPath;
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            try
            {
                DirectoryInfo outputPath = new DirectoryInfo(tbExtractionDirectory.Text);

                if (outputPath.GetFiles().Any())
                {
                    DialogResult dr = MessageBox.Show("There are files already in this directory, do you want to delete them?",
                                    "Clear Directory?", MessageBoxButtons.YesNo);

                    if(dr == DialogResult.Yes)
                        foreach (FileInfo file in outputPath.GetFiles())
                            file.Delete();
                }

                DitaCatalogueExtractor extractor = new DitaCatalogueExtractor(RepositoryLocator.CatalogueRepository,outputPath);
                extractor.Extract(progressBarsUI1);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
            
        }

        private void btnShowDirectory_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(tbExtractionDirectory.Text))
                return;

            DirectoryInfo d = new DirectoryInfo(tbExtractionDirectory.Text);

            UsefulStuff.GetInstance().ShowFolderInWindowsExplorer(d);
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            var popup = new PopupChecksUI("Checking Dita extraction",false);

            DitaCatalogueExtractor extractor = new DitaCatalogueExtractor(RepositoryLocator.CatalogueRepository,null);
            popup.StartChecking(extractor);
        }
    }
}
