// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using FAnsi.Discovery;
using Microsoft.Data.SqlClient;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.DataHelper;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.UI.ExtractionUIs.FilterUIs.ParameterUIs;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.SimpleDialogs.ForwardEngineering;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.MainFormUITabs.SubComponents;

/// <summary>
/// This control offers the preferred method of telling RDMP about your existing datasets.  It lets you select a table on your server and then forward engineer an RDMP Catalogue
/// which lets you build a data load for the table, document its columns, configure extraction logic etc.
/// 
/// <para>Start by entering the details of your table (server, database, table etc).  If you specify username/password then SQL Authentication will be used and the credentials will be
/// stored along with the table (See PasswordEncryptionKeyLocationUI for details), if you do not enter username/password then Windows Authentication will be used (preferred).  </para>
/// 
/// <para>Clicking Import will create TableInfo / ColumnInfo objects in your Data Catalogue database and then ConfigureCatalogueExtractabilityUI will be launched which lets you pick which
/// columns are extractable and which contains the Patient Identifier (e.g. CHI number / NHS number etc).  See ConfigureCatalogueExtractabilityUI for full details. </para>
/// </summary>
public partial class ImportSQLTableUI : RDMPForm
{
    private readonly bool _allowImportAsCatalogue;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ITableInfoImporter Importer { get; private set; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ITableInfo TableInfoCreatedIfAny { get; private set; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string TargetFolder { get; set; }

    private Project _projectSpecific;

    public ImportSQLTableUI(IActivateItems activator, bool allowImportAsCatalogue) : base(activator)
    {
        _allowImportAsCatalogue = allowImportAsCatalogue;
        InitializeComponent();

        serverDatabaseTableSelector1.AllowTableValuedFunctionSelection = true;
        serverDatabaseTableSelector1.SelectionChanged += serverDatabaseTableSelector1_SelectionChanged;

        ddContext.DataSource = Enum.GetValues(typeof(DataAccessContext));
        ddContext.SelectedItem = DataAccessContext.Any; //default to any!

        serverDatabaseTableSelector1.SetItemActivator(activator);
    }

    private void serverDatabaseTableSelector1_SelectionChanged()
    {
        AdjustImporter();
    }

    private void AdjustImporter()
    {
        var cataRepo = Activator.RepositoryLocator.CatalogueRepository;
        try
        {
            var tbl = serverDatabaseTableSelector1.GetDiscoveredTable();

            switch (tbl)
            {
                case null:
                    btnImport.Enabled = false;
                    return;
                //if it isn't a table valued function
                case DiscoveredTableValuedFunction discoveredTableValuedFunction:
                    Importer = new TableValuedFunctionImporter(cataRepo, discoveredTableValuedFunction,
                        (DataAccessContext)ddContext.SelectedValue);
                    break;
                default:
                    Importer = new TableInfoImporter(cataRepo, tbl, (DataAccessContext)ddContext.SelectedValue);
                    break;
            }

            btnImport.Enabled = true;
        }
        catch (Exception exception)
        {
            ExceptionViewer.Show(exception);
        }
    }

    private void btnImport_Click(object sender, EventArgs e)
    {
        if (_allowImportAsCatalogue)
        {
            var ui = new ConfigureCatalogueExtractabilityUI(Activator, Importer, "Existing Table", _projectSpecific)
            {
                TargetFolder = TargetFolder
            };
            ui.ShowDialog();
            TableInfoCreatedIfAny = ui.TableInfoCreated;
        }
        else
        {
            // logic to add credentials
            // parent.SetCredentials();
            Importer.DoImport(out var ti, out _);

            if (ti is DatabaseEntity de)
                Activator.Publish(de);

            TableInfoCreatedIfAny = ti;
        }

        try
        {
            DialogResult = DialogResult.OK;

            var ti = TableInfoCreatedIfAny;

            if (ti != null)
            {
                if (ti.IsTableValuedFunction && ti.GetAllParameters().Any())
                {
                    var options = ParameterCollectionUIOptionsFactory.Create(ti);
                    ParameterCollectionUI.ShowAsDialog(Activator, options, true);
                }

                MessageBox.Show($"Successfully imported table '{ti}'");
            }

            Close();
        }
        catch (SqlException exception)
        {
            MessageBox.Show($"Problem importing table :{exception.Message}");
        }
    }

    private void serverDatabaseTableSelector1_IntegratedSecurityUseChanged(bool use)
    {
        lblWarningAboutToSaveUsernameAndPasswordIntoCatalogue.Visible = !use;
        ddContext.Enabled = !use;
    }

    private void ddContext_SelectedIndexChanged(object sender, EventArgs e)
    {
        AdjustImporter();
    }

    public void SetProjectSpecific(Project project)
    {
        _projectSpecific = project;
    }
}