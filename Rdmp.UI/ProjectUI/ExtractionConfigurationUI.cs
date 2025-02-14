// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Pipeline;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.PipelineUIs.Pipelines;
using Rdmp.UI.PipelineUIs.Pipelines.PluginPipelineUsers;
using Rdmp.UI.Rules;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.ProjectUI;

/// <summary>
/// Allows you to change high level attributes of an ExtractionConfiguration in a data extraction Project.  Executing an ExtractionConfiguration involves joining the
/// selected datasets against the selected cohort (and substituting the private identifiers for project specific anonymous release identifiers) as well as applying any
/// configured filters (See ConfigureDatasetUI).  You can have multiple active configurations in a project, for example you might extract 'Prescribing', 'Biochemistry' and 'Demography' for the cohort 'CasesForProject123' and
/// only datasets 'Biochemistry' and 'Demography' for the cohort 'ControlsForProject123'.
/// 
/// <para>The attributes you can change include the name, description, ticketting system tickets etc.</para>
/// 
/// <para>You can also define global SQL parameters which will be available to all Filters in all datasets extracted as part of the configuration.</para>
/// 
/// <para>You can associate a specific CohortIdentificationConfiguration with the ExtractionConfiguration.  This will allow you to do a 'cohort refresh' (replace the current saved cohort
/// identifier list with a new version built by executing the query - helpful if you have new data being loaded regularly and this results in the study cohort changing).</para>
/// </summary>
public partial class ExtractionConfigurationUI : ExtractionConfigurationUI_Design, ISaveableUI
{
    private ExtractionConfiguration _extractionConfiguration;
    private IPipelineSelectionUI _extractionPipelineSelectionUI;

    private IPipelineSelectionUI _cohortRefreshingPipelineSelectionUI;

    public ExtractionConfigurationUI()
    {
        InitializeComponent();

        tcRequest.Title = "Request Ticket";
        tcRequest.TicketTextChanged += tcRequest_TicketTextChanged;

        tcRelease.Title = "Release Ticket";
        tcRelease.TicketTextChanged += tcRelease_TicketTextChanged;

        cbxCohortIdentificationConfiguration.PropertySelector = sel =>
            sel.Cast<CohortIdentificationConfiguration>().Select(cic => cic == null ? "<<None>>" : cic.Name);
        AssociatedCollection = RDMPCollection.DataExport;
    }

    private void tcRequest_TicketTextChanged(object sender, EventArgs e)
    {
        if (_extractionConfiguration == null)
            return;

        //don't change if it is already that
        if (_extractionConfiguration.RequestTicket != null &&
            _extractionConfiguration.RequestTicket.Equals(tcRequest.TicketText))
            return;

        _extractionConfiguration.RequestTicket = tcRequest.TicketText;

        _extractionConfiguration.SaveToDatabase();
    }

    private void tcRelease_TicketTextChanged(object sender, EventArgs e)
    {
        if (_extractionConfiguration == null)
            return;

        //don't change if it is already that
        if (_extractionConfiguration.ReleaseTicket != null &&
            _extractionConfiguration.ReleaseTicket.Equals(tcRelease.TicketText))
            return;

        _extractionConfiguration.ReleaseTicket = tcRelease.TicketText;
        _extractionConfiguration.SaveToDatabase();
    }

    private bool _bLoading;

    public override void SetDatabaseObject(IActivateItems activator, ExtractionConfiguration databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        _bLoading = true;
        _extractionConfiguration = databaseObject;

        SetupCohortIdentificationConfiguration();

        SetupPipelineSelectionExtraction();
        SetupPipelineSelectionCohortRefresh();

        pbCic.Image = activator.CoreIconProvider
            .GetImage(RDMPConcept.CohortIdentificationConfiguration, OverlayKind.Link).ImageToBitmap();

        tbCreated.Text = _extractionConfiguration.dtCreated.ToString();
        tcRelease.TicketText = _extractionConfiguration.ReleaseTicket;
        tcRequest.TicketText = _extractionConfiguration.RequestTicket;

        _bLoading = false;
    }

    protected override void SetBindings(BinderWithErrorProviderFactory rules, ExtractionConfiguration databaseObject)
    {
        base.SetBindings(rules, databaseObject);

        Bind(tbUsername, "Text", "Username", c => c.Username);
        Bind(tbID, "Text", "ID", c => c.ID);
        Bind(tbDescription, "Text", "Description", c => c.Description);
    }

    public override void SetItemActivator(IActivateItems activator)
    {
        base.SetItemActivator(activator);
        tcRelease.SetItemActivator(activator);
        tcRequest.SetItemActivator(activator);
    }

    private void SetupCohortIdentificationConfiguration()
    {
        cbxCohortIdentificationConfiguration.DataSource =
            Activator.CoreChildProvider.AllCohortIdentificationConfigurations;
        cbxCohortIdentificationConfiguration.SelectedItem = _extractionConfiguration.CohortIdentificationConfiguration;
    }

    private void SetupPipelineSelectionCohortRefresh()
    {
        ragSmiley1Refresh.Reset();

        if (_cohortRefreshingPipelineSelectionUI != null)
            return;
        try
        {
            //the use case is extracting a dataset
            var useCase = new CohortCreationRequest(_extractionConfiguration);

            //the user is DefaultPipeline_ID field of ExtractionConfiguration
            var user = new PipelineUser(typeof(ExtractionConfiguration).GetProperty("CohortRefreshPipeline_ID"),
                _extractionConfiguration);

            //create the UI for this situation
            var factory =
                new PipelineSelectionUIFactory(Activator.RepositoryLocator.CatalogueRepository, user, useCase);
            _cohortRefreshingPipelineSelectionUI = factory.Create(Activator, "Cohort Refresh Pipeline", DockStyle.Fill,
                pChooseCohortRefreshPipeline);
            _cohortRefreshingPipelineSelectionUI.Pipeline = _extractionConfiguration.CohortRefreshPipeline;
            _cohortRefreshingPipelineSelectionUI.PipelineChanged +=
                _cohortRefreshingPipelineSelectionUI_PipelineChanged;
            _cohortRefreshingPipelineSelectionUI.CollapseToSingleLineMode();
        }
        catch (Exception e)
        {
            ragSmiley1Refresh.Fatal(e);
        }
    }

    private void _cohortRefreshingPipelineSelectionUI_PipelineChanged(object sender, EventArgs e)
    {
        ragSmiley1Refresh.Reset();
        try
        {
            new CohortCreationRequest(_extractionConfiguration).GetEngine(_cohortRefreshingPipelineSelectionUI.Pipeline,
                ThrowImmediatelyDataLoadEventListener.Quiet);
        }
        catch (Exception ex)
        {
            ragSmiley1Refresh.Fatal(ex);
        }
    }

    private void SetupPipelineSelectionExtraction()
    {
        //already set i tup
        if (_extractionPipelineSelectionUI != null)
            return;

        //the use case is extracting a dataset
        var useCase = ExtractionPipelineUseCase.DesignTime();

        //the user is DefaultPipeline_ID field of ExtractionConfiguration
        var user = new PipelineUser(typeof(ExtractionConfiguration).GetProperty("DefaultPipeline_ID"),
            _extractionConfiguration,Activator.RepositoryLocator.CatalogueRepository);

        //create the UI for this situation
        var factory = new PipelineSelectionUIFactory(Activator.RepositoryLocator.CatalogueRepository, user, useCase);
        _extractionPipelineSelectionUI =
            factory.Create(Activator, "Extraction Pipeline", DockStyle.Fill, pChooseExtractionPipeline);
        _extractionPipelineSelectionUI.CollapseToSingleLineMode();
    }

    private void cbxCohortIdentificationConfiguration_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_bLoading)
            return;

        if (cbxCohortIdentificationConfiguration.SelectedItem is not CohortIdentificationConfiguration cic)
            _extractionConfiguration.CohortIdentificationConfiguration_ID = null;
        else
            _extractionConfiguration.CohortIdentificationConfiguration_ID = cic.ID;

        SetupPipelineSelectionCohortRefresh();
    }

    private void btnClearCic_Click(object sender, EventArgs e)
    {
        cbxCohortIdentificationConfiguration.SelectedItem = null;
    }

    public override string GetTabName() => $"{_extractionConfiguration.GetProjectHint(true)} {base.GetTabName()}";

    public override string GetTabToolTip() =>
        $"'{base.GetTabName()}' - {_extractionConfiguration.GetProjectHint(false)}";
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ExtractionConfigurationUI_Design, UserControl>))]
public abstract class ExtractionConfigurationUI_Design : RDMPSingleDatabaseObjectControl<ExtractionConfiguration>;