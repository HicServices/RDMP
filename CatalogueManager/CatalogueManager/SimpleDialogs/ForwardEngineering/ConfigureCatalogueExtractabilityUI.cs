using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;

namespace CatalogueManager.SimpleDialogs.ForwardEngineering
{
    /// <summary>
    /// Allows you to choose whether to mark all columns in a newly created Catalogue as Extractable.  Also lets you specify which Column contains the patient identifier (used to link
    /// the records with those in the other tables).
    /// </summary>
    public partial class ConfigureCatalogueExtractabilityUI : UserControl
    {
        public ConfigureCatalogueExtractabilityUI()
        {
            InitializeComponent();
        }

        public bool MakeAllColumnsExtractable { get { return cbMakeAllColumnsExtractable.Checked; }}
        public ColumnInfo ExtractionIdentifier { get { return ddExtractionIdentifier.SelectedItem as ColumnInfo; }}

        public void SetUp(ColumnInfo[] columnInfos)
        {
            ddExtractionIdentifier.Items.AddRange(columnInfos);
            gbMarkAllExtractable.Enabled = true;
        }

        private void cbMakeAllColumnsExtractable_CheckedChanged(object sender, System.EventArgs e)
        {
            ddExtractionIdentifier.Enabled = cbMakeAllColumnsExtractable.Checked;
        }

        public void MarkExtractionIdentifier(IActivateItems activator ,ExtractionInformation[] eis)
        {
            if (ExtractionIdentifier != null)
            {
                //make the ExtractionInformation associated with the ColumnInfo an IsExtractionIdentifier
                var identifier = eis.Single(e => e.ColumnInfo.Equals(ExtractionIdentifier));
                identifier.IsExtractionIdentifier = true;
                identifier.SaveToDatabase();

                //and make the Catalogue an ExtractableDataSet
                var cata = identifier.CatalogueItem.Catalogue;

                if (activator.RepositoryLocator.DataExportRepository != null)
                {
                    //make sure catalogue is not already extractable
                    if (!activator.RepositoryLocator.DataExportRepository.GetAllObjectsWithParent<ExtractableDataSet>(cata).Any())
                    {
                        var ds = new ExtractableDataSet(activator.RepositoryLocator.DataExportRepository, cata);
                        activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(ds));
                    }
                }
            }
        }

        private void btnClear_Click(object sender, System.EventArgs e)
        {
            ddExtractionIdentifier.SelectedItem = null;
        }
    }
}
