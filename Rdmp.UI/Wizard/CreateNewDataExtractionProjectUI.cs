// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.CohortCommitting.Pipeline.Sources;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Sources;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.DataLoad.Modules.DataFlowSources;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Rdmp.Core.Setting;
using Rdmp.UI.CohortUI.CohortSourceManagement;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SingleControlForms;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.Wizard;

/// <summary>
/// Provides a single screen allowing you to execute a CohortIdentificationConfiguration or load an IdentifierList into the snapshot store, allocate release identifiers and build an
/// extraction project with specific datasets.  Each time you use this user interface you will get a new Project so do not use the wizard if you already have an existing Project e.g.
/// if you want to do a project refresh or adjust a cohort etc (In such a case you should use CohortIdentificationCollectionUI to add a new ExtractionConfiguration/Cohort to your existing
/// Project).
/// </summary>
public partial class CreateNewDataExtractionProjectUI : RDMPForm
{
    private Project[] _existingProjects;
    private int _projectNumber;
    private FileInfo _cohortFile;
    private ExtractionConfiguration _configuration;
    private ExtractableCohort _cohortCreated;

    /// <summary>
    /// Datasets that should be added to the <see cref="Project"/> when executed
    /// </summary>
    private IExtractableDataSet[] _selectedDatasets = Array.Empty<IExtractableDataSet>();

    private bool _bLoading;

    public ExtractionConfiguration ExtractionConfigurationCreatedIfAny { get; private set; }
    public Project ProjectCreatedIfAny { get; private set; }

    private void GetNextProjectNumber(IActivateItems activator)
    {

        var AutoSuggestProjectNumbers = false;
        var AutoSuggestProjectNumbersSetting = activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Setting>().Where(s => s.Key == "AutoSuggestProjectNumbers").FirstOrDefault();
        if (AutoSuggestProjectNumbersSetting is not null) AutoSuggestProjectNumbers = Convert.ToBoolean(AutoSuggestProjectNumbersSetting.Value);
        _existingProjects = activator.RepositoryLocator.DataExportRepository.GetAllObjects<Project>();
        if (AutoSuggestProjectNumbers)
        {
            var highestNumber = _existingProjects.Max(p => p.ProjectNumber);
            tbProjectNumber.Text = highestNumber == null ? "1" : (highestNumber.Value + 1).ToString();
        }

    }

    public CreateNewDataExtractionProjectUI(IActivateItems activator) : base(activator)
    {
        InitializeComponent();

        GetNextProjectNumber(activator);
        pbCohort.Image = activator.CoreIconProvider.GetImage(RDMPConcept.CohortIdentificationConfiguration)
            .ImageToBitmap();
        pbCohortFile.Image = activator.CoreIconProvider.GetImage(RDMPConcept.File).ImageToBitmap();
        pbCohortSources.Image = activator.CoreIconProvider.GetImage(RDMPConcept.ExternalCohortTable).ImageToBitmap();

        IdentifyCompatiblePipelines();

        IdentifyCompatibleCohortSources();

        cbxDatasets.Items.AddRange(activator.RepositoryLocator.DataExportRepository
            .GetAllObjects<ExtractableDataSet>());
        btnPackage.Image = activator.CoreIconProvider.GetImage(RDMPConcept.ExtractableDataSetPackage).ImageToBitmap();
        btnPackage.Enabled =
            activator.RepositoryLocator.DataExportRepository.GetAllObjects<ExtractableDataSetPackage>().Any();

        cbxCohort.DataSource = activator.RepositoryLocator.CatalogueRepository
            .GetAllObjects<CohortIdentificationConfiguration>();
        cbxCohort.PropertySelector = collection =>
            collection.Cast<CohortIdentificationConfiguration>().Select(c => c.ToString());
        ClearCic();

        hlpDatasets.SetHelpText("Datasets",
            "Pick which datasets should be extracted when this Project ExtractionConfiguration is run.  You can always change this later on.");
        hlpDefineCohortAndDatasets.SetHelpText("Define Cohort and Datasets",
            "If you have a cohort (list of identifiers to extract) in a file or defined in an RDMP CohortIdentificationConfiguration you can commit this to the Project here.  You can always commit the cohort later on and/or update the cohort etc.");
        hlpExtractionPipeline.SetHelpText("Extraction Pipeline",
            "Choose the default pipeline that should be used to extract the data.  This determines what the output format is e.g. CSV / to database.  If unsure you can leave this blank and choose it later on");
        hlpIdentifierAllocation.SetHelpText("Identifier Allocation",
            "Choose where to store the cohort (if you have multiple cohort databases) and the name.");

        hlpCicPipe.SetHelpText("Pipeline",
            "Choose which Pipeline to use to read the RDMP CohortIdentificationConfiguration and commit it to your cohort database.  Pipeline selection affects which operations are run including which identifier allocation method is used to allocate release identifiers");
        hlpFlatFilePipe.SetHelpText("Pipeline",
            "Choose which Pipeline to use to read the cohort flat file and commit it to your cohort database.  Pipeline selection must be for a source compatible with the file type e.g. CSV / fixed width.  Selection also affects which operations are run including which identifier allocation method is used to allocate release identifiers");
    }

    private void IdentifyCompatibleCohortSources()
    {
        var sources = Activator.RepositoryLocator.DataExportRepository.GetAllObjects<ExternalCohortTable>();

        ddCohortSources.Items.AddRange(sources);

        if (sources.Length == 1)
        {
            ddCohortSources.SelectedItem = sources[0];
            ddCohortSources.Enabled = false;
        }

        btnCreateNewCohortSource.Enabled = sources.Length == 0;
    }

    private void IdentifyCompatiblePipelines()
    {
        var p = Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Pipeline>();

        foreach (var pipeline in p)
        {
            var source = pipeline.Source;
            var destination = pipeline.Destination;


            //pipeline doesn't have a source / destination
            if (source == null || destination == null)
                continue;

            //source defines use case
            var sourceType = source.GetClassAsSystemType();
            var destinationType = destination.GetClassAsSystemType();

            if (typeof(ExecuteDatasetExtractionSource).IsAssignableFrom(sourceType))
                ddExtractionPipeline.Items.Add(pipeline);

            //destination is not a cohort destination
            if (!typeof(ICohortPipelineDestination).IsAssignableFrom(destinationType))
                continue;

            //cic
            if (typeof(CohortIdentificationConfigurationSource).IsAssignableFrom(sourceType))
                ddCicPipeline.Items.Add(pipeline);

            //flat file
            if (typeof(DelimitedFlatFileDataFlowSource).IsAssignableFrom(sourceType))
                ddFilePipeline.Items.Add(pipeline);
        }

        //for each dropdown if there's only one option
        foreach (var dd in new ComboBox[] { ddCicPipeline, ddExtractionPipeline, ddFilePipeline })
            if (dd.Items.Count == 1)
                dd.SelectedItem = dd.Items[0]; //select it
    }

    private void btnBrowse_Click(object sender, EventArgs e)
    {
        var browser = new FolderBrowserDialog();
        if (browser.ShowDialog() == DialogResult.OK)
            tbExtractionDirectory.Text = browser.SelectedPath;
    }

    private void CreateNewDataExtractionProjectUI_Load(object sender, EventArgs e)
    {
    }

    private void tbProjectNumber_TextChanged(object sender, EventArgs e)
    {
        ragProjectNumber.Reset();

        //if there is no project number
        if (string.IsNullOrWhiteSpace(tbProjectNumber.Text))
        {
            ragProjectNumber.Warning(new Exception("Project Number is required"));
            _projectNumber = -1;
            return;
        }

        try
        {
            _projectNumber = int.Parse(tbProjectNumber.Text);

            var collisionProject = _existingProjects.FirstOrDefault(p => p.ProjectNumber == _projectNumber);
            if (collisionProject != null)
                ragProjectNumber.Warning(new Exception(
                    $"There is already an existing Project ('{collisionProject}') with ProjectNumber {_projectNumber}"));
        }
        catch (Exception ex)
        {
            ragProjectNumber.Fatal(ex);
        }
    }

    private void btnSelectClearCohortFile_Click(object sender, EventArgs e)
    {
        if (_cohortFile != null)
        {
            ClearFile();
            return;
        }

        var ofd = new OpenFileDialog
        {
            Filter = "Comma Separated Values|*.csv"
        };
        var result = ofd.ShowDialog();

        if (result == DialogResult.OK)
            SelectFile(new FileInfo(ofd.FileName));
    }

    private void ClearFile()
    {
        _cohortFile = null;
        gbCic.Enabled = true;

        lblCohortFile.Text = "Cohort File...";
        btnSelectClearCohortFile.Text = "Browse...";
        btnSelectClearCohortFile.Left =
            Math.Min(gbFile.Width - btnSelectClearCohortFile.Width, lblCohortFile.Right + 5);
    }

    private void SelectFile(FileInfo fileInfo)
    {
        _cohortFile = fileInfo;
        gbCic.Enabled = false;

        tbCohortName.Text = _cohortFile.Name;

        lblCohortFile.Text = _cohortFile.Name;
        btnSelectClearCohortFile.Text = "Clear";
        btnSelectClearCohortFile.Left =
            Math.Min(gbFile.Width - btnSelectClearCohortFile.Width, lblCohortFile.Right + 5);
    }

    private void cbxCohort_SelectionChangeCommitted(object sender, EventArgs e)
    {
        var cic = cbxCohort.SelectedItem as CohortIdentificationConfiguration;

        if (cic != null)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                ragCic.Reset();

                tbCohortName.Text = cic.ToString();

                var source = new CohortIdentificationConfigurationSource
                {
                    Timeout = 5
                };
                source.PreInitialize(cic, ThrowImmediatelyDataLoadEventListener.Quiet);
                source.Check(ragCic);

                ClearFile();
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        gbFile.Enabled = cic == null;
    }

    private void btnClearCohort_Click(object sender, EventArgs e)
    {
        ClearCic();
    }

    private void ClearCic()
    {
        cbxCohort.SelectedItem = null;
        tbCohortName.Text = null;
    }

    private void btnExecute_Click(object sender, EventArgs e)
    {
        Cursor = Cursors.WaitCursor;

        var problem = AllRequiredDataPresent();

        try
        {
            if (problem != null)
            {
                MessageBox.Show(problem);
                return;
            }

            ragExecute.Reset();

            //create the project
            ProjectCreatedIfAny ??= new Project(Activator.RepositoryLocator.DataExportRepository, tbProjectName.Text);

            ProjectCreatedIfAny.ProjectNumber = int.Parse(tbProjectNumber.Text);
            ProjectCreatedIfAny.ExtractionDirectory = tbExtractionDirectory.Text;

            if (!Directory.Exists(ProjectCreatedIfAny.ExtractionDirectory))
                Directory.CreateDirectory(ProjectCreatedIfAny.ExtractionDirectory);

            ProjectCreatedIfAny.SaveToDatabase();

            if (cbDefineCohort.Checked)
            {
                if (_configuration == null)
                {
                    _configuration = new ExtractionConfiguration(Activator.RepositoryLocator.DataExportRepository,
                        ProjectCreatedIfAny)
                    {
                        Name = "Cases"
                    };
                    _configuration.SaveToDatabase();
                }
                foreach (var ds in _selectedDatasets)
                    _configuration.AddDatasetToConfiguration(ds);


                ICommandExecution cmdAssociateCicWithProject = null;

                if (_cohortCreated == null)
                {
                    var cohortDefinition = new CohortDefinition(null, tbCohortName.Text, 1,
                        ProjectCreatedIfAny.ProjectNumber.Value,
                        (ExternalCohortTable)ddCohortSources.SelectedItem);

                    //execute the cohort creation bit
                    var cohortRequest = new CohortCreationRequest(ProjectCreatedIfAny, cohortDefinition,
                        Activator.RepositoryLocator.DataExportRepository, tbCohortName.Text);

                    ComboBox dd;
                    if (_cohortFile != null)
                    {
                        //execute cohort creation from file.
                        cohortRequest.FileToLoad = new FlatFileToLoad(_cohortFile);
                        dd = ddFilePipeline;
                    }
                    else
                    {
                        //execute cohort creation from cic
                        cohortRequest.CohortIdentificationConfiguration =
                            (CohortIdentificationConfiguration)cbxCohort.SelectedItem;
                        dd = ddCicPipeline;


                        //since we are about to execute a cic and store the results we should associate it with the Project (if successful)
                        cmdAssociateCicWithProject =
                            new ExecuteCommandAssociateCohortIdentificationConfigurationWithProject(Activator).SetTarget(
                                ProjectCreatedIfAny).SetTarget(cohortRequest.CohortIdentificationConfiguration);
                    }

                    var engine = cohortRequest.GetEngine((Pipeline)dd.SelectedItem,
                        ThrowImmediatelyDataLoadEventListener.Quiet);
                    engine.ExecutePipeline(new GracefulCancellationToken());
                    _cohortCreated = cohortRequest.CohortCreatedIfAny;
                }

                //associate the configuration with the cohort
                _configuration.Cohort_ID = _cohortCreated.ID;

                //set the pipeline to use
                var pipeline = (Pipeline)ddExtractionPipeline.SelectedItem;
                if (pipeline != null)
                    _configuration.DefaultPipeline_ID = pipeline.ID;

                _configuration.SaveToDatabase();

                //User defined cohort if it came from cic then associate the cic with the project
                if (cmdAssociateCicWithProject is { IsImpossible: false })
                    cmdAssociateCicWithProject.Execute();
            }

            Cursor = Cursors.Default;

            ExtractionConfigurationCreatedIfAny = _configuration;

            DialogResult = DialogResult.OK;
            MessageBox.Show("Project Created Successfully");
            Close();
        }
        catch (Exception exception)
        {
            ragExecute.Fatal(exception);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private string AllRequiredDataPresent()
    {
        if (string.IsNullOrWhiteSpace(tbProjectName.Text))
            return "You must name your project";

        if (string.IsNullOrWhiteSpace(tbProjectNumber.Text))
            return "You must supply a Project Number";

        if (ragProjectNumber.IsFatal())
            return "There is a problem with the Project Number";

        if (string.IsNullOrWhiteSpace(tbExtractionDirectory.Text))
            return "You must specify a project extraction directory where the flat files will go";

        if (!cbDefineCohort.Checked)
            return null;

        if (string.IsNullOrWhiteSpace(tbCohortName.Text))
            return "You must provide a name for your cohort";

        if (!_selectedDatasets.Any())
            return "You must check at least one dataset";

        if (ddCicPipeline.SelectedItem == null && _cohortFile == null)
            return "You must select a cohort execution pipeline";

        if (ddFilePipeline.SelectedItem == null && _cohortFile != null)
            return "You must select a cohort file import pipeline";

        if (ddExtractionPipeline.SelectedItem == null)
            return "You must select an extraction pipeline";

        if (ddCohortSources.SelectedItem == null)
            return "You must choose an Identifier Allocation database (to put your cohort / anonymous mappings)";

        if (cbxCohort.SelectedItem == null && _cohortFile == null)
            return "You must choose either a file or a cohort identification query to build the cohort from";

        //no problems
        return null;
    }

    private void btnCreateNewCohortSource_Click(object sender, EventArgs e)
    {
        var wizard = new CreateNewCohortDatabaseWizardUI(Activator);
        wizard.SetItemActivator(Activator);
        SingleControlForm.ShowDialog(wizard);
        IdentifyCompatibleCohortSources();
    }

    private void cbDefineCohort_CheckedChanged(object sender, EventArgs e)
    {
        gbCohortAndDatasets.Visible = cbDefineCohort.Checked;
        OnSizeChanged(e);
    }

    private void cbxDatasets_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_bLoading)
            return;

        _selectedDatasets = cbxDatasets.SelectedItem != null
            ? new[] { (IExtractableDataSet)cbxDatasets.SelectedItem }
            : Array.Empty<IExtractableDataSet>();
    }

    private void btnPick_Click(object sender, EventArgs e)
    {
        if (Activator.SelectObjects(new DialogArgs
        {
            InitialObjectSelection = _selectedDatasets,
            TaskDescription = "Which datasets should be extracted in this Project?"
        }, cbxDatasets.Items.Cast<ExtractableDataSet>().ToArray(), out var selected))
        {
            _selectedDatasets = selected;
            UpdateDatasetControlVisibility();
        }
    }


    private void btnPackage_Click(object sender, EventArgs e)
    {
        if (Activator.SelectObjects(new DialogArgs
        {
            TaskDescription =
                        "Which Package(s) should be added to the Project.  Datasets in all packages chosen will be added to the Project"
        }, Activator.RepositoryLocator.DataExportRepository.GetAllObjects<ExtractableDataSetPackage>(),
                out var selected))
        {
            _selectedDatasets = selected
                .SelectMany(p =>
                    Activator.RepositoryLocator.DataExportRepository.GetAllDataSets(p,
                        cbxDatasets.Items.Cast<ExtractableDataSet>().ToArray()))
                .Distinct()
                .ToArray();

            UpdateDatasetControlVisibility();
        }
    }

    /// <summary>
    /// Updates the enabledness and selected item of (<see cref="cbxDatasets"/> to match the current <see cref="_selectedDatasets"/>)
    /// </summary>
    private void UpdateDatasetControlVisibility()
    {
        _bLoading = true;
        cbxDatasets.Visible = _selectedDatasets.Length <= 1;
        lblDatasets.Visible = _selectedDatasets.Length > 1;
        lblDatasets.Text = $"{_selectedDatasets.Length} Datasets";
        cbxDatasets.SelectedItem = _selectedDatasets.Length <= 1 ? _selectedDatasets.SingleOrDefault() : null;
        _bLoading = false;
    }

    private void btnClearDatasets_Click(object sender, EventArgs e)
    {
        _selectedDatasets = Array.Empty<IExtractableDataSet>();
        UpdateDatasetControlVisibility();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        Close();
    }
}