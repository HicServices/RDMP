using BrightIdeasSoftware;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Rdmp.UI.SimpleDialogs
{
    public partial class UpdateCatalogueDataLocationUI : Form
    {

        private Catalogue _catalogue;
        private bool _firstTime = true;
        private IActivateItems _activator;
        public UpdateCatalogueDataLocationUI(IActivateItems activator, Catalogue catalogue)
        {
            InitializeComponent();
            _catalogue = catalogue;
            olvState.AspectGetter = State_AspectGetter;
            GetCurrentDataLocation();
            RefreshData();
            _activator = activator;
        }


        private void RefreshData()
        {
            tlvDatasets.AddObjects(_catalogue.CatalogueItems);
            tlvDatasets.EnableObjects(tlvDatasets.Objects);
            if (_firstTime)
            {
                tlvDatasets.CheckAll();
                _firstTime = false;
            }
        }

        private void GetCurrentDataLocation()
        {
            var location = _catalogue.CatalogueItems.Select(ci => DropColumnIdentifierFromName(ci.ColumnInfo.Name)).ToList();
            if (location.Distinct().Skip(1).Any())
            {
                tbCurrentLocation.Text = "Multiple Locations Found";
            }
            else
            {
                tbCurrentLocation.Text = location.First();
            }

        }

        private string DropColumnIdentifierFromName(string name)
        {
            return string.Join('.', name.Split('.')[..^1]);
        }

        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            tlvDatasets.ModelFilter = new TextMatchFilter(tlvDatasets, tbFilter.Text);
            tlvDatasets.UseFiltering = true;
        }

        private object State_AspectGetter(object rowobject)
        {
            var item = (CatalogueItem)rowobject;
            return item.ColumnInfo.Name;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            //should be disabled until things are set
            var cmd = new ExecuteCommandUpdateCatalogueDataLocation(_activator, tlvDatasets.CheckedObjects.Cast<CatalogueItem>().ToArray(), serverDatabaseTableSelector1.GetDiscoveredTable(), tbMapping.Text);
            if (cmd.Check())
            {
                cmd.Execute();
                _activator.RefreshBus.Publish(_catalogue,new Refreshing.RefreshObjectEventArgs(_catalogue));
                //todo some sort of UI update to let the user know it's worked
            }


        }

        private void serverDatabaseTableSelector1_Load(object sender, EventArgs e)
        {

        }
    }
}
