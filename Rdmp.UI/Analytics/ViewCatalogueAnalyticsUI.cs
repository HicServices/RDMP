using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using Rdmp.Core.Analytics.Data;
using Rdmp.UI.ItemActivation;
using System.Xml.Serialization;
using static ScintillaNET.Style;
using System.IO;
using System.Xml;
using Rdmp.Core.Repositories;
using Org.BouncyCastle.Tls;
using Spectre.Console;
using static MongoDB.Driver.WriteConcern;

namespace Rdmp.UI.Analytics;

public partial class ViewCatalogueAnalyticsUI : ViewCatalogueAnalytics_Design
{
    private Catalogue _catalogue;
    private static XmlSerializer _serializer;
    private CatalogueValidatorConfig _validatorConfig;
    private DQERepository _dqeRepo;
    public ViewCatalogueAnalyticsUI()
    {
        InitializeComponent();
        _serializer ??= new XmlSerializer(typeof(CatalogueValidatorConfig));
    }

    public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        _catalogue = databaseObject;
        _dqeRepo = new DQERepository(_catalogue.CatalogueRepository);
        var foundConfig = _dqeRepo.GetAllObjectsWhere<CatalogueAnalyticConfiguration>("catalogue_ID", _catalogue.ID).First();
        var rdr = XmlReader.Create(new StringReader(foundConfig.ConfigurationXML));
        rdr.Read();
        _validatorConfig = (CatalogueValidatorConfig)_serializer.Deserialize(rdr);
        foreach (var colVal in _validatorConfig.ColumnValidators)
        {
            AddRow(colVal);
        }
    }

#nullable enable

    private void HandleRemove(object? sender, EventArgs e)
    {
        if (sender is null) return;
        int rowID = int.Parse(((Button)sender).Name);
        for (int i = 0; i < 5; i++)
        {
            tableLayoutPanel1.Controls.Remove(tableLayoutPanel1.GetControlFromPosition(i, rowID));
        }
        tableLayoutPanel1.RowStyles.RemoveAt(tableLayoutPanel1.RowCount - 1);
        tableLayoutPanel1.RowCount = tableLayoutPanel1.RowCount - 1;

    }

    private void AddRow(ColumnValidator? colVal)
    {
        tableLayoutPanel1.RowCount = tableLayoutPanel1.RowCount + 1;
        tableLayoutPanel1.RowStyles.Add(new RowStyle());
        var lastRowIndex = tableLayoutPanel1.RowCount - 1;



        var columnCB = new ComboBox();
        columnCB.Items.AddRange(_catalogue.CatalogueItems);

        var regexTb = new TextBox()
        {
            PlaceholderText = "Regex"
        };

        var matchCB = new ComboBox();
        matchCB.Items.AddRange(Enum.GetNames(typeof(AnalyticsValidatorStatuses)));

        var nonMatchCB = new ComboBox();
        nonMatchCB.Items.AddRange(Enum.GetNames(typeof(AnalyticsValidatorStatuses)));

        var removeBtm = new Button() { Text = "Remove", Name = $"{lastRowIndex}" };
        removeBtm.Click += HandleRemove;

        if (colVal is not null)
        {
            columnCB.SelectedItem = _catalogue.CatalogueItems.Where(c => c.Name == colVal.TargetProperty).FirstOrDefault();
            regexTb.Text = colVal.Regex;
            matchCB.SelectedItem = colVal.MatchState.ToString();
            nonMatchCB.SelectedItem = colVal.NotMatchState.ToString();
        }

        tableLayoutPanel1.Controls.Add(columnCB, 0, lastRowIndex);
        tableLayoutPanel1.Controls.Add(regexTb, 1, lastRowIndex);
        tableLayoutPanel1.Controls.Add(matchCB, 2, lastRowIndex);
        tableLayoutPanel1.Controls.Add(nonMatchCB, 3, lastRowIndex);
        tableLayoutPanel1.Controls.Add(removeBtm, 4, lastRowIndex);
    }

    private void btnAddValidationRule_Click(object sender, EventArgs e)
    {
        AddRow(null);
    }

    private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {

    }

    private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
    {

    }

    private void button1_Click(object sender, EventArgs e)
    {
        var newConfig = new CatalogueValidatorConfig();
        for (int i = 1; i < tableLayoutPanel1.RowCount - 1; i++)
        {
            var colVal = new ColumnValidator();
            var control = tableLayoutPanel1.GetControlFromPosition(0, i);
            var cntrl = control;
            colVal.TargetProperty = ((CatalogueItem?)((ComboBox?)control).SelectedItem)?.Name;
        }
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ViewCatalogueAnalytics_Design, UserControl>))]
public abstract class ViewCatalogueAnalytics_Design : RDMPSingleDatabaseObjectControl<Catalogue>
{
}