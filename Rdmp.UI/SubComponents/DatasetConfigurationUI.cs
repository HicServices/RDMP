using Rdmp.UI.TestsAndSetup.ServicePropogation;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Rdmp.UI.Refreshing;
using Rdmp.Core.Dataset;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;
using Rdmp.Core.Curation.Data.Datasets;

namespace Rdmp.UI.SubComponents;
public partial class DatasetConfigurationUI : DatsetConfigurationUI_Design, IRefreshBusSubscriber
{
    private readonly DatasetConfigurationUICommon _common;

    public DatasetConfigurationUI()
    {
        InitializeComponent();
        _common = new DatasetConfigurationUICommon();
    }

    public override void SetDatabaseObject(IActivateItems activator, Dataset databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        _common.Dataset = databaseObject;

        var catalogues = databaseObject.CatalogueRepository
            .GetAllObjectsWhere<ColumnInfo>("Dataset_ID", databaseObject.ID).SelectMany(static ci => ci.CatalogueItems)
            .Select(static ci => ci.CatalogueName).Distinct().ToList();
        if(catalogues.Count < 1)
        {
            lblDatasetUsage.Text = "This dataset is not linked to data yet.";
        }
        else
        {
            var catalogueString = string.Join(Environment.NewLine, catalogues);
            lblDatasetUsage.Text = $"This dataset is used in the following catalogues:{Environment.NewLine}{catalogueString}";
        }

        Bind(tbName, "Text", "Name", static c => c.Name);
        Bind(tbDOI, "Text", "DigitalObjectIdentifier", static c => c.DigitalObjectIdentifier);
        Bind(tbSource, "Text", "Source", static c => c.Source);
        var s = GetObjectSaverButton();
        s.SetupFor(this, databaseObject, activator);
        GetObjectSaverButton()?.Enable(false);

    }


    public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
    }

    private void label1_Click(object sender, EventArgs e)
    {

    }

    private void label4_Click(object sender, EventArgs e)
    {

    }
}
[TypeDescriptionProvider(
    typeof(AbstractControlDescriptionProvider<DatsetConfigurationUI_Design, UserControl>))]
public abstract class
    DatsetConfigurationUI_Design : RDMPSingleDatabaseObjectControl<Dataset>
{
}