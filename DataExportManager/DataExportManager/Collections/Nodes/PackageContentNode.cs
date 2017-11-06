using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.DataTables.DataSetPackages;
using DataExportManager.Collections.Providers;

namespace DataExportManager.Collections.Nodes
{
    public class PackageContentNode
    {
        public ExtractableDataSetPackage Package { get; set; }
        public ExtractableDataSet DataSet { get; set; }

        public PackageContentNode(ExtractableDataSetPackage package, ExtractableDataSet dataSet)
        {
            Package = package;
            DataSet = dataSet;
        }

        public override string ToString()
        {
            return DataSet.ToString();
        }

        public void DeleteWithConfirmation(IActivateItems activator, DataExportChildProvider childProvider)
        {
            if (MessageBox.Show(
                            "Remove ExtractableDataSet '" + DataSet +
                            "' from Package '"+Package+ "'? (Does not delete DataSet or affect any current ExtractionConfigurations)", "Confirm removing DataSet from Package", MessageBoxButtons.YesNo) ==
                        DialogResult.Yes)
            {
                childProvider.PackageContents.RemoveDataSetFromPackage(Package,DataSet);
                activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(Package));
            }
        }
    }
}
