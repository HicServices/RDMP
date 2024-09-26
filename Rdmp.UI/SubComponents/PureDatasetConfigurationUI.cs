using Rdmp.Core.Curation.Data.Datasets;
using Rdmp.Core.Datasets;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rdmp.UI.SubComponents;

public partial class PureDatasetConfigurationUI : DatsetConfigurationUI_Design, IRefreshBusSubscriber
{

    private PureDataset _dataset;
    public PureDatasetConfigurationUI()
    {
        InitializeComponent();
    }

    public override void SetDatabaseObject(IActivateItems activator, Dataset databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        var provider = new PureDatasetProvider(activator, activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<DatasetProviderConfiguration>("ID", databaseObject.Provider_ID).First());
        _dataset = provider.FetchPureDataset(databaseObject);
        lblUrl.Text = _dataset.PortalURL;
        if (_dataset.PortalURL is not null)
        {
            btnViewOnPure.Enabled = true;
        }

    }

    public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void label1_Click(object sender, EventArgs e)
    {

    }

    private void btnViewOnPure_Click(object sender, EventArgs e)
    {
        UsefulStuff.OpenUrl(_dataset.PortalURL);
    }
}

[TypeDescriptionProvider(
    typeof(AbstractControlDescriptionProvider<PureDatsetConfigurationUI_Design, UserControl>))]
public abstract class
    PureDatsetConfigurationUI_Design : RDMPSingleDatabaseObjectControl<Dataset>
{
}