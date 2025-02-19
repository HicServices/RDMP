// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FAnsi.Discovery;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.DataLoad.Engine.Pipeline;
using Rdmp.Core.DataLoad.Engine.Pipeline.Destinations;
using Rdmp.Core.DataLoad.Modules.DataFlowSources;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.SimpleDialogs.ForwardEngineering;
using Rdmp.UI.SingleControlForms;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using Rdmp.UI.TransparentHelpSystem;
using Rdmp.UI.Tutorials;

namespace Rdmp.UI.SimpleDialogs.SimpleFileImporting;

/// <summary>
/// Allows you to import a flat file into your database with appropriate column data types based on the values read from the file.  This data table will then be referenced by an RDMP
/// Catalogue which can be used to interact with it through RDMP.
/// </summary>
public partial class CreateNewCatalogueByImportingFileUI : RDMPForm
{
    private readonly ExecuteCommandCreateNewCatalogueByImportingFile _command;

    private FileInfo _selectedFile;
    private DataFlowPipelineContext<DataTable> _context;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public HelpWorkflow HelpWorkflow { get; set; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string TargetFolder { get; set; }

    public CreateNewCatalogueByImportingFileUI(IActivateItems activator,
        ExecuteCommandCreateNewCatalogueByImportingFile command) : base(activator)
    {
        _command = command;
        InitializeComponent();

        pbFile.Image = activator.CoreIconProvider.GetImage(RDMPConcept.File).ImageToBitmap();
        serverDatabaseTableSelector1.HideTableComponents();
        serverDatabaseTableSelector1.SelectionChanged += serverDatabaseTableSelector1_SelectionChanged;
        serverDatabaseTableSelector1.SetItemActivator(activator);
        SetupState(State.SelectFile);

        if (command.File != null)
            SelectFile(command.File);

        pbHelp.Image = FamFamFamIcons.help.ImageToBitmap();

        BuildHelpFlow();
    }

    private void BuildHelpFlow()
    {
        var tracker = new TutorialTracker(Activator);

        HelpWorkflow = new HelpWorkflow(this, _command, tracker);

        //////Normal work flow
        var pickFile = new HelpStage(gbPickFile, "1. Choose the file you want to import here.\r\n" +
                                                 "\r\n" +
                                                 "Click on the red icon to disable this help.");
        var pickDb = new HelpStage(gbPickDatabase, "2. Select the database to use for importing data.\r\n" +
                                                   "Username and Password are optional; if not set, the connection will be attempted using your windows user");
        var pickName = new HelpStage(gbTableName, "3. Select the name of the created Catalogue.\r\n" +
                                                  "A Table with the same name will be created in the database selected above.\r\n" +
                                                  "Please note that the chosen pipeline can alter the created Table name. If a table with the same name already exists " +
                                                  "in the selected Database, the execution may fail.");
        var pickPipeline = new HelpStage(gbPickPipeline,
            "4. Select the pipeline to execute in order to transfer the data from the files into the DB.\r\n" +
            "If you are not sure, ask the admin which one to use.");
        var execute = new HelpStage(gbExecute, "5. Click Preview to peek at what data is in the selected file.\r\n" +
                                               "Click Execute to run the process and import your file.");

        pickFile.SetOption(">>", pickDb);
        pickDb.SetOption(">>", pickName);
        pickName.SetOption(">>", pickPipeline);
        pickPipeline.SetOption(">>", execute);
        execute.SetOption("|<<", pickFile);
        //stage4.SetOption("next...", stage2);

        HelpWorkflow.RootStage = pickFile;
    }

    private void btnBrowse_Click(object sender, EventArgs e)
    {
        var ofd = new OpenFileDialog
        {
            Filter = "Comma Separated Values|*.csv|Excel File|*.xls*|Text File|*.txt|All Files|*.*"
        };
        var result = ofd.ShowDialog();

        if (result == DialogResult.OK)
            SelectFile(new FileInfo(ofd.FileName));
    }

    private void SelectFile(FileInfo fileName)
    {
        _selectedFile = fileName;
        SetupState(State.FileSelected);
    }

    private void btnClearFile_Click(object sender, EventArgs e)
    {
        SetupState(State.SelectFile);

        btnConfirmDatabase.Enabled = serverDatabaseTableSelector1.GetDiscoveredDatabase() != null;
    }

    private void SetupState(State state)
    {
        switch (state)
        {
            case State.SelectFile:

                //turn things off
                pbFile.Visible = false;
                lblFile.Visible = false;
                btnClearFile.Visible = false;
                ragSmileyFile.Visible = false;
                ddPipeline.DataSource = null;
                gbPickPipeline.Enabled = false;
                gbExecute.Enabled = false;
                gbPickDatabase.Enabled = false;
                btnConfirmDatabase.Enabled = false;
                gbTableName.Enabled = false;

                _selectedFile = null;

                //turn things on
                btnBrowse.Visible = true;

                break;
            case State.FileSelected:

                //turn things off
                btnBrowse.Visible = false;
                gbExecute.Enabled = false;

                //turn things on
                pbFile.Visible = true;
                gbPickDatabase.Enabled = true;
                gbTableName.Enabled = true;

                //text of the file they selected
                lblFile.Text = _selectedFile.Name;
                lblFile.Left = pbFile.Right + 2;
                lblFile.Visible = true;
                try
                {
                    tbTableName.Text =
                        QuerySyntaxHelper.MakeHeaderNameSensible(Path.GetFileNameWithoutExtension(_selectedFile.Name));
                }
                catch (Exception)
                {
                    tbTableName.Text = string.Empty;
                }

                ragSmileyFile.Visible = true;
                ragSmileyFile.Left = lblFile.Right + 2;

                btnClearFile.Left = ragSmileyFile.Right + 2;
                btnClearFile.Visible = true;

                IdentifyCompatiblePipelines();

                IdentifyCompatibleServers();

                break;
            case State.DatabaseSelected:
                //turn things off

                //turn things on
                gbExecute.Enabled = true;
                gbPickDatabase.Enabled = true; //user still might want to change his mind about targets
                btnConfirmDatabase.Enabled = false;
                gbTableName.Enabled = true;

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state));
        }
    }

    private void IdentifyCompatibleServers()
    {
        var servers = Activator.CoreChildProvider.AllServers;

        if (servers.Length == 1)
        {
            var s = servers.Single();


            var uniqueDatabaseNames =
                Activator.CoreChildProvider.AllTableInfos.Select(t => t.GetDatabaseRuntimeName())
                    .Distinct()
                    .ToArray();

            if (uniqueDatabaseNames.Length == 1)
            {
                serverDatabaseTableSelector1.SetExplicitDatabase(s.ServerName, uniqueDatabaseNames[0]);
                SetupState(State.DatabaseSelected);
            }
            else
            {
                serverDatabaseTableSelector1.SetExplicitServer(s.ServerName);
            }
        }
        else if (servers.Length > 1)
        {
            serverDatabaseTableSelector1.SetDefaultServers(
                servers.Select(s => s.ServerName).ToArray()
            );
        }
    }

    private void serverDatabaseTableSelector1_SelectionChanged()
    {
        btnConfirmDatabase.Enabled = serverDatabaseTableSelector1.GetDiscoveredDatabase() != null;
    }

    private void IdentifyCompatiblePipelines()
    {
        gbPickPipeline.Enabled = true;
        ragSmileyFile.Reset();

        _context = new DataFlowPipelineContextFactory<DataTable>().Create(PipelineUsage.LoadsSingleFlatFile);
        _context.MustHaveDestination = typeof(DataTableUploadDestination);

        if (cbOther.Checked)
            _context.MustHaveSource = typeof(IDataFlowSource<DataTable>);
        else if (_selectedFile.Extension == ".csv" || _selectedFile.Extension == ".txt")
            _context.MustHaveSource = typeof(DelimitedFlatFileDataFlowSource);
        else if (_selectedFile.Extension.StartsWith(".xls")) _context.MustHaveSource = typeof(ExcelDataFlowSource);

        var compatiblePipelines = Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Pipeline>()
            .Where(_context.IsAllowable).ToArray();

        if (compatiblePipelines.Length == 0)
        {
            ragSmileyFile.OnCheckPerformed(new CheckEventArgs("No Pipelines are compatible with the selected file",
                CheckResult.Fail));
            return;
        }

        ddPipeline.DataSource = compatiblePipelines;
        ddPipeline.SelectedItem = compatiblePipelines.FirstOrDefault();
    }

    private enum State
    {
        SelectFile,
        FileSelected,
        DatabaseSelected
    }

    private void ddPipeline_SelectedIndexChanged(object sender, EventArgs e)
    {
        GetFactory();

        if (ddPipeline.SelectedItem is not Pipeline p)
            return;
        try
        {
            var source = DataFlowPipelineEngineFactory.CreateSourceIfExists(p);
            ((IPipelineRequirement<FlatFileToLoad>)source).PreInitialize(new FlatFileToLoad(_selectedFile),
                new FromCheckNotifierToDataLoadEventListener(ragSmileyFile));
            ((ICheckable)source).Check(ragSmileyFile);
        }
        catch (Exception exception)
        {
            ragSmileyFile.Fatal(exception);
        }
    }

    private IProject _projectSpecific;

    private void btnConfirmDatabase_Click(object sender, EventArgs e)
    {
        var db = serverDatabaseTableSelector1.GetDiscoveredDatabase();

        if (db == null)
        {
            MessageBox.Show("You must select a Database");
        }
        else if (db.Exists())
        {
            SetupState(State.DatabaseSelected);
        }
        else
        {
            if (Activator.YesNo($"Create Database '{db.GetRuntimeName()}'", "Create Database"))
            {
                db.Server.CreateDatabase(db.GetRuntimeName());
                SetupState(State.DatabaseSelected);
            }
        }
    }

    private void btnPreview_Click(object sender, EventArgs e)
    {
        if (ddPipeline.SelectedItem is not Pipeline p)
        {
            MessageBox.Show("No Pipeline Selected");
            return;
        }

        var source = (IDataFlowSource<DataTable>)DataFlowPipelineEngineFactory.CreateSourceIfExists(p);

        ((IPipelineRequirement<FlatFileToLoad>)source).PreInitialize(new FlatFileToLoad(_selectedFile),
            new FromCheckNotifierToDataLoadEventListener(ragSmileyFile));

        Cursor.Current = Cursors.WaitCursor;
        var preview = source.TryGetPreview();
        Cursor.Current = Cursors.Default;

        if (preview != null)
        {
            var dtv = new DataTableViewerUI(preview, "Preview");
            SingleControlForm.ShowDialog(dtv);
        }
    }

    private UploadFileUseCase GetUseCase() =>
        new(_selectedFile, serverDatabaseTableSelector1.GetDiscoveredDatabase(), Activator);

    private DataFlowPipelineEngineFactory GetFactory() => new(GetUseCase());

    private void btnExecute_Click(object sender, EventArgs e)
    {
        if (ddPipeline.SelectedItem is not Pipeline p)
        {
            MessageBox.Show("No Pipeline Selected");
            return;
        }

        if (string.IsNullOrWhiteSpace(tbTableName.Text))
        {
            MessageBox.Show("Enter Catalogue name");
            return;
        }

        ragSmileyExecute.Reset();
        try
        {
            var db = serverDatabaseTableSelector1.GetDiscoveredDatabase();
            var engine = GetFactory().Create(p, new FromCheckNotifierToDataLoadEventListener(ragSmileyExecute));
            engine.Initialize(new FlatFileToLoad(_selectedFile), db, Activator);

            var crashed = false;

            var dest = (DataTableUploadDestination)engine.DestinationObject;
            dest.TableNamerDelegate = () => tbTableName.Text;

            using var cts = new CancellationTokenSource();
            var t = Task.Run(() =>
            {
                try
                {
                    engine.ExecutePipeline(new GracefulCancellationToken(cts.Token, cts.Token));
                }
                catch (PipelineCrashedException ex)
                {
                    Activator.ShowException("Error uploading", ex.InnerException ?? ex);
                    if (dest.CreatedTable)
                        ConfirmTableDeletion(db.ExpectTable(dest.TargetTableName));
                    crashed = true;
                }
                catch (Exception ex)
                {
                    Activator.ShowException("Error uploading", ex);
                    if (dest.CreatedTable)
                        ConfirmTableDeletion(db.ExpectTable(dest.TargetTableName));
                    crashed = true;
                }
            }, cts.Token);

            Activator.Wait("Uploading Table...", t, cts);

            if (crashed)
                return;

            if (t.IsFaulted)
                throw t.Exception ?? new Exception("Task Failed");

            if (t.IsCanceled || cts.IsCancellationRequested)
                return;

            ForwardEngineer(db.ExpectTable(dest.TargetTableName));
        }
        catch (Exception exception)
        {
            ragSmileyExecute.Fatal(exception);
        }
    }

    private static void ConfirmTableDeletion(DiscoveredTable expectTable)
    {
        if (expectTable.Exists())
        {
            var confirm = MessageBox.Show(
                $"A table named {expectTable.GetFullyQualifiedName()} has been created as part of this import. Do you want to keep it?",
                "Confirm", MessageBoxButtons.YesNo);
            if (confirm == DialogResult.No)
                expectTable.Drop();
        }
    }

    private void ForwardEngineer(DiscoveredTable targetTableName)
    {
        var extractionPicker = new ConfigureCatalogueExtractabilityUI(Activator,
            new TableInfoImporter(Activator.RepositoryLocator.CatalogueRepository, targetTableName),
            $"File '{_selectedFile.FullName}'", _projectSpecific)
        {
            TargetFolder = TargetFolder,
            TableCreated = targetTableName
        };
        extractionPicker.ShowDialog();

        var catalogue = extractionPicker.CatalogueCreatedIfAny;
        if (catalogue is DatabaseEntity de)
        {
            Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(de));

            MessageBox.Show(
                $"Successfully imported new Dataset '{catalogue}'.\r\nThe edit functionality will now open.");

            Activator.WindowArranger.SetupEditAnything(this, catalogue);
        }

        if (cbAutoClose.Checked)
            Close();
        else
            MessageBox.Show(
                "Creation completed successfully, close the Form when you are finished reviewing the output");
    }

    private void pbHelp_Click(object sender, EventArgs e)
    {
        HelpWorkflow.Start(true);
    }

    public void SetProjectSpecific(IProject project)
    {
        _projectSpecific = project;
    }

    private void tbTableName_TextChanged(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(tbTableName.Text))
            //if the sane name doesn't match the
            tbTableName.ForeColor = !tbTableName.Text.Equals(QuerySyntaxHelper.MakeHeaderNameSensible(tbTableName.Text),
                StringComparison.CurrentCultureIgnoreCase)
                ? Color.Red
                : Color.Black;
    }

    private void cbOther_CheckedChanged(object sender, EventArgs e)
    {
        IdentifyCompatiblePipelines();
    }
}