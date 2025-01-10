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

public partial class HDRDatasetConfigurationUI : DatsetConfigurationUI_Design, IRefreshBusSubscriber
{

    private HDRDataset _dataset;
    private IActivateItems _activator;
    private HDRDatasetProvider _provider;
    private Dataset _dbObject;
    public HDRDatasetConfigurationUI()
    {
        InitializeComponent();
    }

    public override void SetDatabaseObject(IActivateItems activator, Dataset databaseObject)
    {
        _activator = activator;
        _dbObject = databaseObject;
        base.SetDatabaseObject(activator, databaseObject);
        _provider = new HDRDatasetProvider(activator, activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<DatasetProviderConfiguration>("ID", databaseObject.Provider_ID).First());
        _dataset = _provider.FetchHDRDataset(databaseObject);
        var summary= _dataset.data.versions.First().metadata.metadata.summary;
        //if (_dataset.PortalURL is not null)
        //{
        //    btnViewOnHDR.Enabled = true;
        //}
        tbName.Text = summary.title;
        tbAbstract.Text = summary.@abstract;
        tbContactPoint.Text = summary.contactPoint.ToString();
        tbDOI.Text = summary.doiName?.ToString();
        tbDescription.Text = summary.description;
        tbStartDate.Text = _dataset.data.versions.First().metadata.metadata.provenance.temporal.startDate;
        tbEndDate.Text = _dataset.data.versions.First().metadata.metadata.provenance.temporal.endDate;
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
        //UsefulStuff.OpenUrl(_dataset.PortalURL);
    }

    private void PureDatasetConfigurationUI_Load(object sender, EventArgs e)
    {

    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        if (_activator.YesNo("Are you sure?", "Save Changes"))
        {
            var datasetUpdate = new HDRDataset();
            var summary = _dataset.data.versions.First().metadata.metadata.summary;
            _dataset.data.versions.First().metadata.metadata.summary.title = tbName.Text;
            _dataset.data.versions.First().metadata.metadata.summary.@abstract = tbAbstract.Text;
            //    _dataset.Title.En_GB = tbName.Text;
            //    datasetUpdate.Title = _dataset.Title;
            //    var datasetDescriptionTerm = "/dk/atira/pure/dataset/descriptions/datasetdescription";
            //    var description = _dataset.Descriptions.Where(d => d.Term.URI == datasetDescriptionTerm).FirstOrDefault();
            //    if (description is null)
            //    {
            //        //error
            //        Console.WriteLine("No known description!");
            //    }
            //    else
            //    {
            //        description.Value = new ENGBWrapper(tbDescription.Text);
            //        datasetUpdate.Descriptions = new List<PureDescription> { description };
            //    }
            //    if (!string.IsNullOrWhiteSpace(tbStartDate.Text))
            //    {
            //        _dataset.TemporalCoveragePeriod.StartDate = new PureDate(DateTime.Parse(tbStartDate.Text));
            //    }
            //    if (!string.IsNullOrWhiteSpace(tbTemporalEnd.Text))
            //    {
            //        _dataset.TemporalCoveragePeriod.EndDate = new PureDate(DateTime.Parse(tbTemporalEnd.Text));
            //    }
            //    if (!string.IsNullOrWhiteSpace(tbStartDate.Text) || !string.IsNullOrWhiteSpace(tbTemporalEnd.Text))
            //        datasetUpdate.TemporalCoveragePeriod = _dataset.TemporalCoveragePeriod;

            //    datasetUpdate.Links = new();
            //    foreach (var link in links)
            //    {
            //        var original = _dataset.Links.Where(l => l.PureID == link.Link.PureID).First();
            //        if (original.Url == link.LinkUrl && link.LinkDescription == original.Description.En_GB)
            //        {
            //            continue;
            //        }
            //        var pl = new PureLink(link.Link.PureID, link.LinkUrl, link.Link.Alias, new ENGBWrapper(link.LinkDescription), link.Link.LinkType);
            //        datasetUpdate.Links.Add(pl);
            //    }
            //    if (!datasetUpdate.Links.Any()) datasetUpdate.Links = null;
                _provider.Update(_dataset.data.id.ToString(), datasetUpdate);
                SetDatabaseObject(_activator, _dbObject);
                _activator.Show("Successfully updated Pure dataset");
        }
    }

    private void label4_Click(object sender, EventArgs e)
    {

    }

}

[TypeDescriptionProvider(
    typeof(AbstractControlDescriptionProvider<HDRDatsetConfigurationUI_Design, UserControl>))]
public abstract class
    HDRDatsetConfigurationUI_Design : RDMPSingleDatabaseObjectControl<Dataset>
{
}