// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Repositories;
using Rdmp.UI.Collections;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.PipelineUIs.DemandsInitializationUIs;
using Rdmp.UI.SimpleDialogs;
using WideMessageBox = Rdmp.UI.SimpleDialogs.WideMessageBox;

namespace Rdmp.UI.PipelineUIs.Pipelines;

/// <summary>
/// Main component control of ConfigurePipelineUI (See ConfigurePipelineUI for details).  Shows you all compatible components on the left including any plugin components.  Components in
/// red are not compatible with the current context for example a DelimitedFlatFileDataFlowSource requires a FlatFileToLoad and is therefore incompatible under any context where that object is
/// not available.
/// </summary>
public partial class PipelineWorkAreaUI : UserControl
{
    private PipelineDiagramUI _pipelineDiagram;
    private ArgumentCollectionUI _arumentsCollection1;
    private readonly IActivateItems _activator;
    private List<AdvertisedPipelineComponentTypeUnderContext> _allComponents;
    private readonly ICatalogueRepository _catalogueRepository;


    public PipelineWorkAreaUI(IActivateItems activator, IPipeline pipeline, IPipelineUseCase useCase,
        ICatalogueRepository catalogueRepository)
    {
        _activator = activator;
        var useCase1 = useCase;
        _catalogueRepository = catalogueRepository;

        InitializeComponent();

        olvComponents.BuildGroups(olvRole, SortOrder.Ascending);
        olvComponents.AlwaysGroupByColumn = olvRole;
        olvComponents.FullRowSelect = true;

        _pipelineDiagram = new PipelineDiagramUI(_activator)
        {
            AllowSelection = true,
            AllowReOrdering = true
        };
        _pipelineDiagram.SelectedComponentChanged += _pipelineDiagram_SelectedComponentChanged;
        _pipelineDiagram.Dock = DockStyle.Fill;
        diagramPanel.Controls.Add(_pipelineDiagram);

        _arumentsCollection1 = new ArgumentCollectionUI
        {
            Dock = DockStyle.Fill
        };
        gbArguments.Controls.Add(_arumentsCollection1);

        olvComponents.RowFormatter += RowFormatter;
        var context = useCase1.GetContext();

        try
        {
            //middle and destination components
            var allComponentTypes = MEF.GetGenericTypes(typeof(IDataFlowComponent<>), context.GetFlowType());

            //source components (list of all types with MEF exports of )
            var allSourceTypes = MEF.GetGenericTypes(typeof(IDataFlowSource<>), context.GetFlowType());

            _allComponents = new List<AdvertisedPipelineComponentTypeUnderContext>();

            _allComponents.AddRange(allComponentTypes
                .Select(t => new AdvertisedPipelineComponentTypeUnderContext(t, useCase1)).ToArray());
            _allComponents.AddRange(allSourceTypes
                .Select(t => new AdvertisedPipelineComponentTypeUnderContext(t, useCase)).ToArray());

            RefreshComponentList();
        }
        catch (Exception exception)
        {
            ExceptionViewer.Show("Failed to get list of supported MEF components that could be added to the pipeline ",
                exception);
        }

        gbArguments.Enabled = false;

        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvComponents, olvCompatible,
            new Guid("1b8737cb-75d6-401b-b8a2-441e3e4322ac"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvComponents, olvNamespace,
            new Guid("35c0497e-3c04-46be-a6d6-eb02111aadb3"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvComponents, olvRole,
            new Guid("fb1205f3-049e-4fe3-89c5-d07b55fa2e17"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvComponents, olvName,
            new Guid("b7e797e8-ef6a-45d9-b51d-c2f12dbacead"));
    }

    /// <summary>
    /// Refreshes the list of components visible in <see cref="olvComponents"/> list view to only those
    /// that are compatible (unless <see cref="cbShowIncompatible"/> is ticked)
    /// </summary>
    private void RefreshComponentList()
    {
        olvComponents.ClearObjects();
        olvComponents.AddObjects(_allComponents.Where(a => a.IsCompatible() || cbShowIncompatible.Checked).ToArray());
    }

    private void _pipelineDiagram_SelectedComponentChanged(object sender, IPipelineComponent selected)
    {
        gbArguments.Enabled = true;

        if (selected == null)
            _arumentsCollection1.Setup(_activator, null, null, _catalogueRepository);
        else
            _arumentsCollection1.Setup(_activator, selected, selected.GetClassAsSystemType(), _catalogueRepository);
    }

    private void RowFormatter(OLVListItem olvItem)
    {
        var advertised = (AdvertisedPipelineComponentTypeUnderContext)olvItem.RowObject;

        if (!advertised.IsCompatible())
            olvItem.ForeColor = Color.Red;
    }

    public void SetTo(IPipeline pipeline, IPipelineUseCase useCase)
    {
        _pipelineDiagram.SetTo(pipeline, useCase);
    }


    private void olvComponents_CellRightClick(object sender, CellRightClickEventArgs e)
    {
        var model = (AdvertisedPipelineComponentTypeUnderContext)e.Model;

        var RightClickMenu = new ContextMenuStrip();

        if (model != null)
            if (!model.IsCompatible())
                RightClickMenu.Items.Add("Component incompatible", null,
                    (s, v) => WideMessageBox.Show(model.ToString(), model.GetReasonIncompatible(),
                        WideMessageBoxTheme.Help));

        //show it
        if (RightClickMenu.Items.Count != 0)
            RightClickMenu.Show(this, e.Location);
    }

    private void btnReRunChecks_Click(object sender, EventArgs e)
    {
        _pipelineDiagram.RefreshUIFromDatabase();
    }

    private void tbSearchComponents_TextChanged(object sender, EventArgs e)
    {
        olvComponents.ModelFilter = new TextMatchFilter(olvComponents, tbSearchComponents.Text,
            StringComparison.CurrentCultureIgnoreCase);
        olvComponents.UseFiltering = !string.IsNullOrWhiteSpace(tbSearchComponents.Text);
    }

    private void cbShowIncompatible_CheckedChanged(object sender, EventArgs e)
    {
        RefreshComponentList();
    }
}