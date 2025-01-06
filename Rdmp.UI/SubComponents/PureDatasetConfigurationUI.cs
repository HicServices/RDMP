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

public partial class PureDatasetConfigurationUI : DatsetConfigurationUI_Design, IRefreshBusSubscriber
{

    private PureDataset _dataset;
    private IActivateItems _activator;
    private PureDatasetProvider _provider;
    private Dataset _dbObject;
    private List<PureDatasetLinkUI> links = new();
    public PureDatasetConfigurationUI()
    {
        InitializeComponent();
    }

    public override void SetDatabaseObject(IActivateItems activator, Dataset databaseObject)
    {
        links = new();
        _activator = activator;
        _dbObject = databaseObject;
        base.SetDatabaseObject(activator, databaseObject);
        _provider = new PureDatasetProvider(activator, activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<DatasetProviderConfiguration>("ID", databaseObject.Provider_ID).First());
        _dataset = _provider.FetchPureDataset(databaseObject);
        if (_dataset.PortalURL is not null)
        {
            btnViewOnPure.Enabled = true;
        }
        tbName.Text = _dataset.Title.En_GB;
        if (_dataset.Descriptions.Any(d => d.Value is not null))
            tbDescription.Text = _dataset.Descriptions.Where(d => d.Value is not null).First().Value.En_GB;
        if (_dataset.TemporalCoveragePeriod is not null && _dataset.TemporalCoveragePeriod.StartDate is not null)
            tbTemporalStart.Text = _dataset.TemporalCoveragePeriod.StartDate.ToDateTime().ToString();
        if (_dataset.TemporalCoveragePeriod is not null && _dataset.TemporalCoveragePeriod.EndDate is not null)
            tbTemporalEnd.Text = _dataset.TemporalCoveragePeriod.EndDate.ToDateTime().ToString();

        if (_dataset.Links is not null)
        {
            var yOffset = 0;
            foreach (var link in _dataset.Links)
            {
                var linkUI = new PureDatasetLinkUI(link);
                links.Add(linkUI);
                this.panelLinks.Controls.Add(linkUI);
                linkUI.Location = new Point(linkUI.Location.X, linkUI.Location.Y + yOffset);
                yOffset += 40;
            }
            this.panelLinks.Height += yOffset;
        }
        else
        {
            lblLinks.Visible = false;
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

    private void PureDatasetConfigurationUI_Load(object sender, EventArgs e)
    {

    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        if (_activator.YesNo("Are you sure?", "Save Changes"))
        {
            var datasetUpdate = new PureDataset();
            _dataset.Title.En_GB = tbName.Text;
            datasetUpdate.Title = _dataset.Title;
            var datasetDescriptionTerm = "/dk/atira/pure/dataset/descriptions/datasetdescription";
            var description = _dataset.Descriptions.Where(d => d.Term.URI == datasetDescriptionTerm).FirstOrDefault();
            if (description is null)
            {
                //error
                Console.WriteLine("No known description!");
            }
            else
            {
                description.Value = new ENGBWrapper(tbDescription.Text);
                datasetUpdate.Descriptions = new List<PureDescription> { description };
            }
            if (!string.IsNullOrWhiteSpace(tbTemporalStart.Text))
            {
                _dataset.TemporalCoveragePeriod.StartDate = new PureDate(DateTime.Parse(tbTemporalStart.Text));
            }
            if (!string.IsNullOrWhiteSpace(tbTemporalEnd.Text))
            {
                _dataset.TemporalCoveragePeriod.EndDate = new PureDate(DateTime.Parse(tbTemporalEnd.Text));
            }
            if (!string.IsNullOrWhiteSpace(tbTemporalStart.Text) || !string.IsNullOrWhiteSpace(tbTemporalEnd.Text))
                datasetUpdate.TemporalCoveragePeriod = _dataset.TemporalCoveragePeriod;

            datasetUpdate.Links = new();
            foreach (var link in links)
            {
                var original = _dataset.Links.Where(l => l.PureID == link.Link.PureID).First();
                if (original.Url == link.LinkUrl && link.LinkDescription == original.Description.En_GB)
                {
                    continue;
                }
                var pl = new PureLink(link.Link.PureID, link.LinkUrl, link.Link.Alias, new ENGBWrapper(link.LinkDescription), link.Link.LinkType);
                datasetUpdate.Links.Add(pl);
            }
            if (!datasetUpdate.Links.Any()) datasetUpdate.Links = null;
            _provider.Update(_dataset.UUID, datasetUpdate);
            SetDatabaseObject(_activator, _dbObject);
            _activator.Show("Successfully updated Pure dataset");
        }
    }

    private void label4_Click(object sender, EventArgs e)
    {

    }

}

[TypeDescriptionProvider(
    typeof(AbstractControlDescriptionProvider<PureDatsetConfigurationUI_Design, UserControl>))]
public abstract class
    PureDatsetConfigurationUI_Design : RDMPSingleDatabaseObjectControl<Dataset>
{
}