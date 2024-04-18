// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FAnsi;
using Rdmp.Core.CommandLine.DatabaseCreation;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.Curation.Checks;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.Core.Startup;
using Rdmp.UI.ChecksUI;
using Rdmp.UI.DataLoadUIs.ModuleUIs;
using Rdmp.UI.SimpleDialogs;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace Rdmp.UI.LocationsMenu;

/// <summary>
/// All metadata in RDMP is stored in one of two main databases.  The Catalogue database records all the technical, descriptive, governance, data load, filtering logic etc about
/// your datasets (including where they are stored etc).  The Data Export Manager database stores all the extraction configurations you have created for releasing to researchers.
/// 
/// <para>This window lets you tell the software where your Catalogue / Data Export Manager databases are or create new ones.  These connection strings are recorded in each users settings file.
/// It is strongly advised that you use Integrated Security (Windows Security) for connecting rather than a username/password as this is the only case where Passwords are not encrypted
/// (Since the encryption certificate location is stored in the Catalogue! - see PasswordEncryptionKeyLocationUI).</para>
/// 
/// <para>Only the Catalogue database is required, if you do not intend to do data extraction at this time then you can skip creating one.  </para>
/// 
/// <para>It is a good idea to run Check after configuring your connection string to ensure that the database is accessible and that the tables/columns in the database match the softwares
/// expectations.  </para>
/// 
/// <para>IMPORTANT: if you configure your connection string wrongly it might take up to 30s for windows to timeout the network connection (e.g. if you specify the wrong server name). This is
/// similar to if you type in a dodgy server name in Microsoft Windows Explorer.</para>
/// </summary>
public partial class ChoosePlatformDatabasesUI : Form
{
    private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;

    public bool ChangesMade;

    private int _seed = 500;
    private int _peopleCount = ExampleDatasetsCreation.NumberOfPeople;
    private int _rowCount = ExampleDatasetsCreation.NumberOfRowsPerDataset;

    public ChoosePlatformDatabasesUI(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
    {
        _repositoryLocator = repositoryLocator;

        InitializeComponent();

        new RecentHistoryOfControls(tbCatalogueConnectionString, new Guid("75e6b0a3-03f2-49fc-9446-ebc1dae9f123"));
        new RecentHistoryOfControls(tbDataExportManagerConnectionString,
            new Guid("9ce952d8-d629-454a-ab9b-a1af97548be6"));

        SetState(State.PickNewOrExisting);

        TableRepository cataDb = null;
        TableRepository dataExportDb = null;

        try
        {
            //are we dealing with a database object repository?
            cataDb = _repositoryLocator.CatalogueRepository as TableRepository;
            dataExportDb = _repositoryLocator.DataExportRepository as TableRepository;
        }
        catch (CorruptRepositoryConnectionDetailsException)
        {
            MessageBox.Show("Current connection strings are invalid and have been cleared");
        }

        //only enable connection string setting if it is a user settings repo
        tbDataExportManagerConnectionString.Enabled =
            tbCatalogueConnectionString.Enabled =
                btnBrowseForCatalogue.Enabled =
                    btnBrowseForDataExport.Enabled =
                        btnSaveAndClose.Enabled =
                            _repositoryLocator is UserSettingsRepositoryFinder;

        //yes
        tbCatalogueConnectionString.Text = cataDb?.ConnectionString;
        tbDataExportManagerConnectionString.Text = dataExportDb?.ConnectionString;

        tbRowCount.Text = ExampleDatasetsCreation.NumberOfRowsPerDataset.ToString();
        tbPeopleCount.Text = ExampleDatasetsCreation.NumberOfPeople.ToString();
    }

    private void SetState(State newState)
    {
        switch (newState)
        {
            case State.PickNewOrExisting:
                pChooseOption.Dock = DockStyle.Top;

                pResults.Visible = false;
                gbCreateNew.Visible = false;
                gbUseExisting.Visible = false;

                pChooseOption.Visible = true;
                pChooseOption.BringToFront();
                break;
            case State.CreateNew:

                pResults.Dock = DockStyle.Fill;
                gbCreateNew.Dock = DockStyle.Top;


                pResults.Visible = true;
                pChooseOption.Visible = false;
                gbUseExisting.Visible = false;

                gbCreateNew.Visible = true;
                pResults.BringToFront();


                break;
            case State.ConnectToExisting:
                pResults.Dock = DockStyle.Fill;
                gbUseExisting.Dock = DockStyle.Top;

                pChooseOption.Visible = false;
                gbCreateNew.Visible = false;

                pResults.Visible = true;
                gbUseExisting.Visible = true;
                pResults.BringToFront();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState));
        }
    }

    private enum State
    {
        PickNewOrExisting,
        CreateNew,
        ConnectToExisting
    }

    private bool SaveConnectionStrings()
    {
        ChangesMade = true;

        try
        {
            // save all the settings
            UserSettings.CatalogueConnectionString = tbCatalogueConnectionString.Text;
            UserSettings.DataExportConnectionString = tbDataExportManagerConnectionString.Text;

            ((UserSettingsRepositoryFinder)_repositoryLocator).RefreshRepositoriesFromUserSettings();
            return true;
        }
        catch (Exception exception)
        {
            checksUI1.OnCheckPerformed(new CheckEventArgs("Failed to save connection settings", CheckResult.Fail,
                exception));
            return false;
        }
    }

    private void ChooseDatabase_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
            btnSaveAndClose_Click(null, null);

        if (e.KeyCode == Keys.Escape)
            Close();
    }

    private void tbCatalogueConnectionString_KeyUp(object sender, KeyEventArgs e)
    {
        //if user is doing a paste
        if (e.KeyCode == Keys.V && e.Control)
        {
            //check to see what he is pasting
            var toPaste = Clipboard.GetText();

            //he is pasting something with newlines
            if (toPaste.Contains(Environment.NewLine))
            {
                //see if he is trying to paste two lines at once, in whichcase surpress Windows and paste it across the two text boxes
                var toPasteArray = toPaste.Split(Environment.NewLine.ToCharArray(),
                    StringSplitOptions.RemoveEmptyEntries);
                if (toPasteArray.Length == 2)
                {
                    tbCatalogueConnectionString.Text = toPasteArray[0];
                    tbDataExportManagerConnectionString.Text = toPasteArray[1];
                    e.SuppressKeyPress = true;
                }
            }
        }
    }

    private void btnSaveAndClose_Click(object sender, EventArgs e)
    {
        //if save is successful
        if (SaveConnectionStrings())
        {
            //integrity checks passed
            UserSettings.UseLocalFileSystem = false;
            RestartApplication();
        }
    }

    private void btnCheckDataExportManager_Click(object sender, EventArgs e)
    {
        CheckRepository(false);
    }

    private void btnCheckCatalogue_Click(object sender, EventArgs e)
    {
        CheckRepository(true);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="catalogue">True for catalogue, false for data export</param>
    private void CheckRepository(bool catalogue)
    {
        try
        {
            //save the settings
            SaveConnectionStrings();

            var repo = catalogue
                ? (TableRepository)_repositoryLocator.CatalogueRepository
                : (TableRepository)_repositoryLocator.DataExportRepository;

            if (repo == null || string.IsNullOrWhiteSpace(repo.ConnectionString))
            {
                checksUI1.OnCheckPerformed(new CheckEventArgs("No connection string has been set", CheckResult.Fail));
                return;
            }

            checksUI1.StartChecking(new MissingFieldsChecker(repo));
            checksUI1.AllChecksComplete += ShowNextStageOnChecksComplete;
        }
        catch (Exception exception)
        {
            checksUI1.OnCheckPerformed(new CheckEventArgs("Checking of Database failed", CheckResult.Fail, exception));
        }
    }

    private void ShowNextStageOnChecksComplete(object sender, AllChecksCompleteHandlerArgs args)
    {
        ((ChecksUI.ChecksUI)sender).AllChecksComplete -= ShowNextStageOnChecksComplete;
    }

    private void btnCreateSuite_Click(object sender, EventArgs e)
    {
        var sb = new StringBuilder();
        try
        {
            Cursor = Cursors.WaitCursor;

            Console.SetOut(new StringWriter(sb));

            var opts = new PlatformDatabaseCreationOptions
            {
                ServerName = tbSuiteServer.Text,
                Prefix = tbDatabasePrefix.Text,
                Username = tbUsername.Text,
                Password = tbPassword.Text,
                ExampleDatasets = cbCreateExampleDatasets.Checked,
                CreateLoggingServer = cbCreateLoggingServer.Checked,
                Seed = _seed,
                NumberOfPeople = _peopleCount,
                NumberOfRowsPerDataset = _rowCount,
                OtherKeywords = tbOtherKeywords.Text,
                CreateDatabaseTimeout = int.TryParse(tbCreateDatabaseTimeout.Text, out var timeout) ? timeout : 30
            };

            var failed = false;

            var task = new Task(() =>
            {
                try
                {
                    PlatformDatabaseCreation.CreatePlatformDatabases(opts);
                    if (!opts.SkipPipelines)
                        PostFixPipelines(opts);
                }
                catch (Exception ex)
                {
                    checksUI1.OnCheckPerformed(
                        new CheckEventArgs("Database creation failed, check exception for details", CheckResult.Fail,
                            ex));
                    failed = true;
                }
            });
            task.Start();

            while (!task.IsCompleted)
            {
                task.Wait(100);
                Application.DoEvents();

                var result = sb.ToString();

                if (string.IsNullOrEmpty(result))
                    continue;

                sb.Clear();

                if (result.Contains("Exception"))
                    throw new Exception(result);

                checksUI1.OnCheckPerformed(new CheckEventArgs(result, CheckResult.Success));
            }

            checksUI1.OnCheckPerformed(new CheckEventArgs("Finished Creating Platform Databases", CheckResult.Success));

            var cata = opts.GetBuilder(PlatformDatabaseCreation.DefaultCatalogueDatabaseName);
            var export = opts.GetBuilder(PlatformDatabaseCreation.DefaultDataExportDatabaseName);

            UserSettings.CatalogueConnectionString = cata.ConnectionString;
            UserSettings.DataExportConnectionString = export.ConnectionString;

            if (!failed)
                RestartApplication();
        }
        catch (Exception exception)
        {
            checksUI1.OnCheckPerformed(new CheckEventArgs("Database creation failed, check exception for details",
                CheckResult.Fail, exception));
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private static void PostFixPipelines(PlatformDatabaseCreationOptions opts)
    {
        var repo = new PlatformDatabaseCreationRepositoryFinder(opts);
        var bulkInsertCsvPipe = repo.CatalogueRepository
            .GetAllObjects<Pipeline>()
            .FirstOrDefault(p => p.Name == "BULK INSERT: CSV Import File (manual column-type editing)");
        if (bulkInsertCsvPipe != null)
        {
            var d = (PipelineComponentArgument)bulkInsertCsvPipe.Destination.GetAllArguments()
                .Single(a => a.Name.Equals("Adjuster"));
            d.SetValue(typeof(AdjustColumnDataTypesUI));
            d.SaveToDatabase();
        }
    }

    private static void RestartApplication()
    {
        MessageBox.Show("Connection Strings Changed, the application will now restart");
        ApplicationRestarter.Restart();
    }

    private void btnCreateNew_Click(object sender, EventArgs e)
    {
        SetState(State.CreateNew);
    }

    private void btnUseExisting_Click(object sender, EventArgs e)
    {
        SetState(State.ConnectToExisting);
    }

    private void btnBack_Click(object sender, EventArgs e)
    {
        SetState(State.PickNewOrExisting);
    }

    private void btnUseYamlFile_Click(object sender, EventArgs e)
    {
        using var fb = new OpenFileDialog();
        var result = fb.ShowDialog();
        if (result == DialogResult.OK)
        {
            try
            {
                var location = new DirectoryInfo(fb.FileName);
                using (var reader = new StreamReader(location.FullName))
                {
                    // Load the stream
                    var yaml = new YamlStream();
                    yaml.Load(reader);
                    var docs = yaml.Documents.First().AllNodes.Select(n => n.ToString());
                    string catalogueConnectionString = null;
                    string dataExportConnectionString = null;
                    foreach (var item in docs.Select((value, i) => new { i, value }))
                    {
                        var value = item.value;
                        var index = item.i;

                        if (value == "CatalogueConnectionString") catalogueConnectionString = docs.ToList()[item.i + 1];
                        if (value == "DataExportConnectionString") dataExportConnectionString = docs.ToList()[item.i + 1];
                    }
                    if (catalogueConnectionString != null) tbCatalogueConnectionString.Text = catalogueConnectionString;
                    if (dataExportConnectionString != null) tbDataExportManagerConnectionString.Text = dataExportConnectionString;

                }
            }
            catch (Exception)
            {
                //Unable to parse yaml file
            }

        };
    }

    private void btnBrowseForCatalogue_Click(object sender, EventArgs e)
    {
        var dialog = new ServerDatabaseTableSelectorDialog("Catalogue Database", false, false, null);
        dialog.LockDatabaseType(DatabaseType.MicrosoftSQLServer);
        if (dialog.ShowDialog() == DialogResult.OK && dialog.SelectedDatabase != null)
            tbCatalogueConnectionString.Text = dialog.SelectedDatabase.Server.Builder.ConnectionString;
    }

    private void btnBrowseForDataExport_Click(object sender, EventArgs e)
    {
        var dialog = new ServerDatabaseTableSelectorDialog("Data Export Database", false, false, null);
        dialog.LockDatabaseType(DatabaseType.MicrosoftSQLServer);
        if (dialog.ShowDialog() == DialogResult.OK)
            tbDataExportManagerConnectionString.Text = dialog.SelectedDatabase.Server.Builder.ConnectionString;
    }


    private void Tb_TextChanged(object sender, EventArgs e)
    {
        var tb = (TextBox)sender;

        try
        {
            var result = int.Parse(tb.Text);

            if (sender == tbSeed)
                _seed = result;
            else if (sender == tbPeopleCount)
                _peopleCount = result;
            else if (sender == tbRowCount)
                _rowCount = result;

            tb.ForeColor = Color.Black;
        }
        catch (Exception)
        {
            tb.ForeColor = Color.Red;
        }
    }

    private void btnCreateYamlFile_Click(object sender, EventArgs e)
    {
        try
        {
            var toSerialize = new ConnectionStringsYamlFile
            {
                CatalogueConnectionString = tbCatalogueConnectionString.Text,
                DataExportConnectionString = tbDataExportManagerConnectionString.Text
            };

            var serializer = new Serializer();
            var yaml = serializer.Serialize(toSerialize);

            var sfd = new SaveFileDialog
            {
                Filter = "Yaml|*.yaml",
                Title = "Save yaml",
                InitialDirectory = UsefulStuff.GetExecutableDirectory().FullName
            };

            if (sfd.ShowDialog() == DialogResult.OK) File.WriteAllText(sfd.FileName, yaml);
        }
        catch (Exception ex)
        {
            ExceptionViewer.Show(ex);
        }
    }

    private void cbCreateExampleDatasets_CheckedChanged(object sender, EventArgs e)
    {
        gbExampleDatasets.Enabled = cbCreateExampleDatasets.Checked;
    }
}