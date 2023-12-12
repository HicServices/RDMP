using Rdmp.UI.TestsAndSetup.ServicePropogation;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Rdmp.UI.Refreshing;
using Rdmp.Core.Dataset;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;


namespace Rdmp.UI.SubComponents;
public partial class DatsetConfigurationUI : DatsetConfigurationUI_DesignProviderUI, IRefreshBusSubscriber
{
    DatasetConfigurationUICommon Common;

    public DatsetConfigurationUI()
    {
        InitializeComponent();
        Common = new DatasetConfigurationUICommon();
    }

    public override void SetDatabaseObject(IActivateItems activator, Dataset databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        Common.Dataset = databaseObject;

        var catalogues = databaseObject.CatalogueRepository.GetAllObjectsWhere<ColumnInfo>("Dataset_ID", databaseObject.ID).SelectMany(ci => ci.CatalogueItems).ToList().Select(ci => ci.CatalogueName).Distinct();
        if(catalogues.Count() < 1)
        {
            lblDatasetUsage.Text = "This dataset is not linked to data yet.";
        }
        else
        {
            var catalogueString = string.Join(Environment.NewLine, catalogues);
            lblDatasetUsage.Text = $"This dataset is used in the following catalogues:{Environment.NewLine}{catalogueString}";
        }

        Bind(tbName, "Text", "Name", c => c.Name);
        Bind(tbDOI, "Text", "DigitalObjectIdentifier", c => c.DigitalObjectIdentifier);
        Bind(tbSource, "Text", "Source", c => c.Source);
        Bind(tbFolder, "Text", "Folder", c => c.Folder);
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
    DatsetConfigurationUI_DesignProviderUI : RDMPSingleDatabaseObjectControl<Dataset>
{
}