using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Datasets;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rdmp.UI.SubComponents;

public partial class DatasetProviderConfigurationUI : DatasetProviderConfigurationUI_Design, IRefreshBusSubscriber
{
    private DatasetProviderConfiguration _configuration;
    private IActivateItems _activator;
    public DatasetProviderConfigurationUI()
    {
        InitializeComponent();
    }

    public override void SetDatabaseObject(IActivateItems activator, DatasetProviderConfiguration databaseObject)
    {
        _activator = activator;
        base.SetDatabaseObject(activator, databaseObject);
        _configuration = databaseObject;
        tbName.Text = _configuration.Name;
        tbOrgId.Text = _configuration.Organisation_ID;
        tbType.Text = _configuration.Type;
        tbUrl.Text = _configuration.Url;
        cbAccessCredentials.Items.Clear();
        var credentials = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<DataAccessCredentials>().ToArray();
        cbAccessCredentials.Items.AddRange(credentials);
        cbAccessCredentials.SelectedIndex = Array.FindIndex(credentials, c => c.ID == _configuration.DataAccessCredentials_ID);
    }

    private void label1_Click(object sender, EventArgs e)
    {

    }

    private void label3_Click(object sender, EventArgs e)
    {

    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        _configuration.Name = tbName.Text;
        _configuration.Url = tbUrl.Text;
        _configuration.DataAccessCredentials_ID = ((DataAccessCredentials)cbAccessCredentials.SelectedItem).ID;
        _configuration.Organisation_ID = tbOrgId.Text;
        _configuration.SaveToDatabase();
        Publish(_configuration);
        _activator.Show("Updated Dataset Provider Configuration");
    }

    public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
    }
}

[TypeDescriptionProvider(
    typeof(AbstractControlDescriptionProvider<DatasetProviderConfigurationUI_Design, UserControl>))]
public abstract class
    DatasetProviderConfigurationUI_Design : RDMPSingleDatabaseObjectControl<DatasetProviderConfiguration>
{
}