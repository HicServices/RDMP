using NPOI.HSSF.Record.Aggregates.Chart;
using Rdmp.Core.Curation.Data.Datasets;
using Rdmp.Core.Datasets;
using Rdmp.Core.Datasets.PureItems;
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

public partial class JiraDatasetConfigurationUI : DatsetConfigurationUI_Design, IRefreshBusSubscriber
{

    private JiraDataset _dataset;
    private IActivateItems _activator;
    private JiraDatasetProvider _provider;
    private Dataset _dbObject;
    public JiraDatasetConfigurationUI()
    {
        InitializeComponent();
    }

    public override void SetDatabaseObject(IActivateItems activator, Dataset databaseObject)
    {
        _activator = activator;
        _dbObject = databaseObject;
        base.SetDatabaseObject(activator, databaseObject);
        _provider = new JiraDatasetProvider(activator, activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<DatasetProviderConfiguration>("ID", databaseObject.Provider_ID).First());
        _dataset = (JiraDataset)_provider.FetchDatasetByID(int.Parse(databaseObject.Url.Split("/").Last()));
        if (_dataset._links.self is not null)
        {
            btnViewOnJira.Enabled = true;
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
        UsefulStuff.OpenUrl(_dataset._links.self);
    }

    private void PureDatasetConfigurationUI_Load(object sender, EventArgs e)
    {

    }

    private void label4_Click(object sender, EventArgs e)
    {

    }

}

[TypeDescriptionProvider(
    typeof(AbstractControlDescriptionProvider<JiraDatsetConfigurationUI_Design, UserControl>))]
public abstract class
    JiraDatsetConfigurationUI_Design : RDMPSingleDatabaseObjectControl<Dataset>
{
}