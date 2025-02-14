// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction;
using Rdmp.Core.DataExport.DataRelease.Pipeline;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers;
using Rdmp.Core.Providers.Nodes;
using Rdmp.UI.Collections;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.PipelineUIs.Pipelines;
using Rdmp.UI.PipelineUIs.Pipelines.PluginPipelineUsers;
using Rdmp.UI.Refreshing;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.DataRelease;

/// <summary>
/// The ultimate end point of the Data Export Manager is the provision of a packaged up Release of all the anonymised datasets for all the cohort(s) (e.g. 'Cases' and 'Controls') in
/// a research project.  There is no going back once you have sent the package to the researcher, if you have accidentally included the wrong datasets or supplied identifiable data
/// (e.g. in a free text field) then you are in big trouble.  For this reason the 'Release' process is a tightly controlled sequence which the RDMP undertakes to try to reduce error.
/// 
/// <para>In this control you will see all the currently selected datasets in a project's configuration(s) and the state of the dataset extraction (from the RDMP's perspective) as well
/// as the status of the 'Environment' (Ticketing System).  Right clicking on a dataset will give you options appropriate to its state.</para>
/// 
/// <para>Extraction of large datasets can take days or weeks and a project extraction is an ongoing exercise.  It is possible that by the time you come to release a project some of the
/// early datasets have been changed or the files deleted etc.  The status of each extracted dataset is shown in the list box.  You can only do an extraction once all the datasets in
/// the configuration are releasable.</para>
/// 
/// <para>In addition to verifying the datasets you can tie the RDMP into your ticketing system.  For example if you have tickets for each project extraction with stages for validation
/// (so that data analysts can log time against validation and sign off on it etc) then you can setup Data Export Manager when the 'Release' Ticket is at a certain state (e.g. validated).
/// To configure a ticketing system see TicketingSystemConfigurationUI.</para>
/// 
/// <para>If you haven't configured a Ticketing System then you shouldn't have to worry about the Environment State.</para>
/// 
/// <para> Once you have selected all the configurations you want to release click Release.</para>
/// </summary>
public partial class DataReleaseUI : DataReleaseUI_Design
{
    private Project _project;

    private bool _isFirstTime = true;

    private IPipelineSelectionUI _pipelineSelectionUI1;
    private IMapsDirectlyToDatabaseTable[] _globals;
    private DataExportChildProvider _childProvider;

    private ArbitraryFolderNode _globalsNode = new(ExtractionDirectory.GLOBALS_DATA_NAME, -500);


    private bool _isExecuting;
    private RDMPCollectionCommonFunctionality _commonFunctionality;
    private IEnumerable<ExtractionConfiguration> _configurations = Array.Empty<ExtractionConfiguration>();
    private IEnumerable<ISelectedDataSets> _selectedDataSets = Array.Empty<ISelectedDataSets>();

    private ToolStripControlHost _pipelinePanel;

    public DataReleaseUI()
    {
        InitializeComponent();

        AssociatedCollection = RDMPCollection.DataExport;

        tlvReleasePotentials.CanExpandGetter = CanExpandGetter;
        tlvReleasePotentials.ChildrenGetter = ChildrenGetter;
        checkAndExecuteUI1.CommandGetter = CommandGetter;

        olvReleaseability.AspectGetter = Releaseability_AspectGetter;
        olvReleaseability.ImageGetter = Releaseability_ImageGetter;
        checkAndExecuteUI1.StateChanged += CheckAndExecuteUI1OnStateChanged;

        checkAndExecuteUI1.AllowsYesNoToAll = false;

        _commonFunctionality = new RDMPCollectionCommonFunctionality();

        checkAndExecuteUI1.BackColor = Color.FromArgb(240, 240, 240);
    }

    private void CheckAndExecuteUI1OnStateChanged(object sender, EventArgs eventArgs)
    {
        tlvReleasePotentials.RefreshObjects(tlvReleasePotentials.Objects.Cast<object>().ToArray());

        if (_isExecuting && !checkAndExecuteUI1.IsExecuting)
            //if it was executing before and now no longer executing the status of the ExtractionConfigurations / Projects might have changed
            Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_project));

        _isExecuting = checkAndExecuteUI1.IsExecuting;
    }

    private object Releaseability_ImageGetter(object rowObject)
    {
        var state = GetState(rowObject);
        return state == null ? null : Activator.CoreIconProvider.GetImage(state).ImageToBitmap();
    }

    private object Releaseability_AspectGetter(object rowObject)
    {
        var state = GetState(rowObject);
        return state?.ToString();
    }

    private object GetState(object rowObject)
    {
        return checkAndExecuteUI1.CurrentRunner is not ReleaseRunner releaseRunner
            ? null
            : rowObject switch
            {
                IExtractionConfiguration configuration => releaseRunner.GetState(configuration),
                ISelectedDataSets sds => releaseRunner.GetState(sds),
                SupportingDocument supportingDocument => releaseRunner.GetState(supportingDocument),
                SupportingSQLTable supportingSqlTable => releaseRunner.GetState(supportingSqlTable),
                _ => rowObject.Equals(_globalsNode) ? releaseRunner.GetGlobalReleaseState() : null
            };
    }

    private RDMPCommandLineOptions CommandGetter(CommandLineActivity activityRequested)
    {
        return new ReleaseOptions
        {
            Pipeline = _pipelineSelectionUI1.Pipeline == null ? "0" : _pipelineSelectionUI1.Pipeline.ID.ToString(),
            Configurations = ToIdList(
                _configurations
                    .Where(c => tlvReleasePotentials.IsChecked(c) || tlvReleasePotentials.IsCheckedIndeterminate(c))
                    .Select(ec => ec.ID).ToArray()
            ),
            SelectedDataSets = ToIdList(
                _selectedDataSets.All(tlvReleasePotentials.IsChecked)
                    ? Array.Empty<int>()
                    : tlvReleasePotentials.CheckedObjects.OfType<ISelectedDataSets>().Select(sds => sds.ID).ToArray()
            ),
            Command = activityRequested,
            ReleaseGlobals = tlvReleasePotentials.IsChecked(_globalsNode)
        };
    }

    private static string ToIdList(int[] ints)
    {
        return string.Join(",", ints.Select(i => i.ToString()));
    }

    private IEnumerable ChildrenGetter(object model)
    {
        return model switch
        {
            Project p => _configurations = _childProvider.GetActiveConfigurationsOnly(p),
            ExtractionConfiguration ec =>
                _selectedDataSets = _childProvider.GetChildren(ec).OfType<ISelectedDataSets>(),
            _ => Equals(model, _globalsNode) ? _globals : null
        };
    }

    private bool CanExpandGetter(object model)
    {
        var c = ChildrenGetter(model);

        return c != null && c.Cast<object>().Any();
    }

    public override void SetDatabaseObject(IActivateItems activator, Project databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);

        if (!_commonFunctionality.IsSetup)
        {
            _commonFunctionality.SetUp(RDMPCollection.None, tlvReleasePotentials, Activator, olvName, null,
                new RDMPCollectionCommonFunctionalitySettings
                {
                    AddFavouriteColumn = false,
                    SuppressChildrenAdder = true,
                    AddCheckColumn = false
                });

            _commonFunctionality.SetupColumnTracking(olvName, new Guid("2d09c1d2-b4a7-400f-9003-d23e43cd3d75"));
            _commonFunctionality.SetupColumnTracking(olvReleaseability,
                new Guid("2f0ca398-a0d5-4e13-bb40-a2c817d4179a"));
        }

        _childProvider = (DataExportChildProvider)Activator.CoreChildProvider;
        _project = databaseObject;

        //figure out the globals
        var ec = _project.ExtractionConfigurations.FirstOrDefault();
        _globals = ec != null ? ec.GetGlobals() : Array.Empty<IMapsDirectlyToDatabaseTable>();

        if (_pipelineSelectionUI1 == null)
        {
            var context = ReleaseUseCase.DesignTime();
            _pipelineSelectionUI1 =
                new PipelineSelectionUIFactory(Activator.RepositoryLocator.CatalogueRepository, null, context).Create(
                    Activator, "Release", DockStyle.Fill);
            _pipelineSelectionUI1.CollapseToSingleLineMode();
            _pipelineSelectionUI1.Pipeline = null;
            _pipelineSelectionUI1.PipelineChanged += ResetChecksUI;

            _pipelinePanel = new ToolStripControlHost((Control)_pipelineSelectionUI1);
        }

        CommonFunctionality.Add(new ToolStripLabel("Release Pipeline:"));
        CommonFunctionality.Add(_pipelinePanel);
        CommonFunctionality.AddHelpStringToToolStrip("Release Pipeline",
            "The sequence of components that will be executed in order to gather the extracted artefacts and assemble them into a single release folder/database. This will start with a source component that gathers the artefacts (from wherever they were extracted to) followed by subsequent components (if any) and then a destination component that generates the final releasable file/folder.");

        checkAndExecuteUI1.SetItemActivator(activator);

        var checkedBefore = tlvReleasePotentials.CheckedObjects;

        tlvReleasePotentials.ClearObjects();
        tlvReleasePotentials.AddObject(_globalsNode);
        tlvReleasePotentials.AddObject(_project);
        tlvReleasePotentials.ExpandAll();

        if (_isFirstTime)
            tlvReleasePotentials.CheckAll();
        else if (checkedBefore.Count > 0)
            tlvReleasePotentials.CheckObjects(checkedBefore);

        _isFirstTime = false;

        tlvReleasePotentials.DisableObjects(_globals);
        //tlvReleasePotentials.DisableObject(_globalsNode);
    }

    public override void ConsultAboutClosing(object sender, FormClosingEventArgs e)
    {
        base.ConsultAboutClosing(sender, e);
        checkAndExecuteUI1.ConsultAboutClosing(sender, e);
    }

    public override string GetTabName() => $"Release: {_project}";

    public void TickAllFor(ExtractionConfiguration configuration)
    {
        tlvReleasePotentials.UncheckAll();
        tlvReleasePotentials.CheckObject(configuration);
        tlvReleasePotentials.CheckObject(_globalsNode);
    }

    public void Tick(ISelectedDataSets selectedDataSet)
    {
        tlvReleasePotentials.UncheckAll();
        tlvReleasePotentials.CheckObject(selectedDataSet);
        tlvReleasePotentials.CheckObject(_globalsNode);
    }

    private void ResetChecksUI(object sender, EventArgs e)
    {
        if (!checkAndExecuteUI1.IsExecuting)
            checkAndExecuteUI1.Reset();
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<DataReleaseUI_Design, UserControl>))]
public abstract class DataReleaseUI_Design : RDMPSingleDatabaseObjectControl<Project>;