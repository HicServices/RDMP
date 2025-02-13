// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Rdmp.UI.CommandExecution.AtomicCommands.UIFactory;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.PipelineUIs.DataObjects;
using Rdmp.UI.PipelineUIs.Pipelines.Models;
using Rdmp.UI.SimpleDialogs;
using RAGSmiley = Rdmp.UI.ChecksUI.RAGSmiley;

namespace Rdmp.UI.PipelineUIs.Pipelines;

public delegate void PipelineComponentSelectedHandler(object sender, IPipelineComponent selected);

/// <summary>
/// Used to visualise an IPipeline (See ConfigurePipelineUI and ConfigureAndExecutePipelineUI for what these are).  This control has a readonly/editable setting on it.  In dialogs where
/// you are selecting an IPipeline you will see the diagram rendered readonly.  If you are editing  (See PipelineWorkAreaUI and ConfigurePipelineUI) then you will be able to select
/// and drag and drop in new components to make an IPipeline configuration.  On such a dialog you can also select a component to change the components arguments (See ArgumentCollection).
/// </summary>
public sealed partial class PipelineDiagramUI : UserControl
{
    private IPipeline _pipeline;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool AllowSelection { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool AllowReOrdering { get; set; }

    private readonly RAGSmiley pipelineSmiley = new();

    public IPipelineComponent SelectedComponent;
    public event PipelineComponentSelectedHandler SelectedComponentChanged;

    private IPipelineUseCase _useCase;
    private DataFlowPipelineEngineFactory _pipelineFactory;

    private readonly ToolStripMenuItem _deleteSelectedMenuItem;
    private readonly IActivateItems _activator;

    public PipelineDiagramUI(IActivateItems activator)
    {
        _activator = activator;

        InitializeComponent();
        AllowSelection = false;

        Controls.Add(pipelineSmiley);
        pipelineSmiley.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        pipelineSmiley.Left = Width - pipelineSmiley.Width - 1;
        pipelineSmiley.Top = 1;
        pipelineSmiley.BringToFront();

        _deleteSelectedMenuItem = new ToolStripMenuItem("Delete selected component", null, DeleteSelectedComponent)
        {
            ShortcutKeys = Keys.Delete,
            Enabled = false
        };
        var cms = new ContextMenuStrip();
        cms.Items.Add(_deleteSelectedMenuItem);
        ContextMenuStrip = cms;
    }

    private void DeleteSelectedComponent(object sender, EventArgs e)
    {
        if (SelectedComponent == null || MessageBox.Show($"Do you want to delete {SelectedComponent.Class}?",
                "Confirm Delete",
                MessageBoxButtons.YesNo) != DialogResult.Yes) return;

        //if they are deleting the destination
        if (SelectedComponent.ID == _pipeline.DestinationPipelineComponent_ID)
        {
            _pipeline.DestinationPipelineComponent_ID = null;
            _pipeline.SaveToDatabase();
        }

        //if they are deleting the source
        if (SelectedComponent.ID == _pipeline.SourcePipelineComponent_ID)
        {
            _pipeline.SourcePipelineComponent_ID = null;
            _pipeline.SaveToDatabase();
        }

        SelectedComponent.DeleteInDatabase();
        RefreshUIFromDatabase();

        SelectedComponent = null;
        _deleteSelectedMenuItem.Enabled = false;
    }


    public void RefreshUIFromDatabase()
    {
        SetTo(_pipeline, _useCase);
    }


    public void Clear()
    {
        _pipeline = null;
        if (_useCase == null) //it is already cleared?
            return;
        SetTo(null, _useCase);
    }

    public void SetTo(IPipeline pipeline, IPipelineUseCase useCase)
    {
        _useCase = useCase;

        _pipeline = pipeline;
        _pipeline?.ClearAllInjections();

        //clear the diagram
        flpPipelineDiagram.Controls.Clear();

        pipelineSmiley.Reset();

        pInitializationObjects.Controls.Clear();

        var factory = new AtomicCommandUIFactory(_activator);

        foreach (var o in _useCase.GetInitializationObjects().Reverse())
        {
            var b = factory.CreateButton(new ExecuteCommandDescribe(_activator, o));
            pInitializationObjects.Controls.Add(b);
        }

        try
        {
            //if there is a pipeline
            if (_pipeline != null)
            {
                try
                {
                    _pipelineFactory = new DataFlowPipelineEngineFactory(_useCase, _pipeline);

                    //create it
                    var pipelineInstance =
                        _pipelineFactory.Create(pipeline, ThrowImmediatelyDataLoadEventListener.Quiet);

                    //initialize it (unless it is design time)
                    if (!_useCase.IsDesignTime)
                        pipelineInstance.Initialize(_useCase.GetInitializationObjects().ToArray());
                }
                catch (Exception ex)
                {
                    pipelineSmiley.Fatal(ex);
                }

                //There is a pipeline set but we might have been unable to fully realize it so setup stuff based on PipelineComponents

                //was there an explicit instance?
                if (_useCase.ExplicitSource != null)
                    AddExplicit(_useCase.ExplicitSource); //if so add it
                else
                //there wasn't an explicit one so there was a PipelineComponent maybe? albeit one that might be broken?
                if (pipeline.SourcePipelineComponent_ID != null)
                    AddPipelineComponent((int)pipeline.SourcePipelineComponent_ID, PipelineComponentRole.Source,
                        pipeline.Repository); //add the possibly broken PipelineComponent to the diagram
                else
                    AddBlankComponent(PipelineComponentRole.Source); //the user hasn't put one in yet


                foreach (var middleComponent in pipeline.PipelineComponents.Where(c =>
                             c.ID != pipeline.SourcePipelineComponent_ID &&
                             c.ID != pipeline.DestinationPipelineComponent_ID).OrderBy(comp => comp.Order))
                    AddPipelineComponent(middleComponent,
                        PipelineComponentRole.Middle); //add the possibly broken PipelineComponent to the diagram

                //was there an explicit instance?
                if (_useCase.ExplicitDestination != null)
                {
                    AddDividerIfReorderingAvailable();
                    AddExplicit(_useCase.ExplicitDestination); //if so add it
                }
                else
                //there wasn't an explicit one so there was a PipelineComponent maybe? albeit one that might be broken?
                if (pipeline.DestinationPipelineComponent_ID != null)
                {
                    AddPipelineComponent((int)pipeline.DestinationPipelineComponent_ID,
                        PipelineComponentRole.Destination,
                        pipeline.Repository); //add the possibly broken PipelineComponent to the diagram
                }
                else
                {
                    AddDividerIfReorderingAvailable();
                    AddBlankComponent(PipelineComponentRole.Destination); //the user hasn't put one in yet
                }


                return;
            }


            //Fallback
            //user has not picked a pipeline yet, show him the shell (factory)
            //factory has no source, add empty source
            if (_useCase.ExplicitSource == null)
                AddBlankComponent(PipelineComponentRole.Source);
            else
                AddExplicit(_useCase.ExplicitSource);


            //factory has no source, add empty source
            if (_useCase.ExplicitDestination == null)
                AddBlankComponent(PipelineComponentRole.Destination);
            else
                AddExplicit(_useCase.ExplicitDestination);
        }
        finally
        {
            Invalidate();
        }
    }

    //by ID overload
    private void AddPipelineComponent(int componentID, PipelineComponentRole role, IRepository repository)
    {
        AddPipelineComponent(repository.GetObjectByID<PipelineComponent>(componentID), role);
    }

    private void AddPipelineComponent(IPipelineComponent toRealize, PipelineComponentRole role)
    {
        //create the pipeline realization (might fail
        var value = DataFlowPipelineEngineFactory.TryCreateComponent(toRealize, out var exConstruction);

        if (role != PipelineComponentRole.Source)
            AddDividerIfReorderingAvailable();

        //create the visualization
        var component =
            new PipelineComponentVisualisation(toRealize, role, value, exConstruction, component_shouldAllowDrop);
        component.DragDrop += component_DragDrop;
        flpPipelineDiagram.Controls.Add(component); //add the component

        //try to initialize the realization (we do this after creating visualization so that when the events come in we can record where on UI the consumption events happen and also because later on it might be that initialization takes ages and we want to use a Thread)
        if (value != null)
            try
            {
                if (!_useCase.IsDesignTime)
                {
                    _useCase.GetContext().PreInitializeGeneric(ThrowImmediatelyDataLoadEventListener.Quiet, value,
                        _useCase.GetInitializationObjects().ToArray());
                    component.Check();
                }

                component.CheckMandatoryProperties();
            }
            catch (Exception exInit)
            {
                //initialization failed
                component.ExInitialization = exInit;
            }

        component.AllowDrag = AllowReOrdering;

        if (AllowSelection)
        {
            component.AllowSelection = true;
            component.ComponentSelected += component_Selected;
        }


        //PipelineComponents can never be locked because they are user setup things
        component.IsLocked = false;
    }

    private void AddDividerIfReorderingAvailable()
    {
        if (!AllowReOrdering)
            return;

        var divider = new DividerLineControl(dividerLine_shouldAllowDrop);
        divider.DragDrop += divider_DragDrop;
        flpPipelineDiagram.Controls.Add(divider);
    }


    private void component_Selected(object sender, IPipelineComponent selected)
    {
        if (!AllowSelection)
            return;

        SelectedComponent = selected;

        //update the Del menu item
        _deleteSelectedMenuItem.Enabled = SelectedComponent != null;

        //clear old selections
        foreach (var componentVisualisation in flpPipelineDiagram.Controls.OfType<PipelineComponentVisualisation>())
            componentVisualisation.IsSelected = false;

        ((PipelineComponentVisualisation)sender).IsSelected = true;
        SelectedComponentChanged?.Invoke(this, selected);

        Focus();
    }

    private void AddExplicit(object value)
    {
        var role = PipelineComponent.GetRoleFor(value.GetType());

        var component = new DataFlowComponentVisualisation(role, value, null);
        flpPipelineDiagram.Controls.Add(component); //add the explicit component
        component.IsLocked = true;
        try
        {
            if (!_useCase.IsDesignTime)
                _useCase.GetContext().PreInitializeGeneric(ThrowImmediatelyDataLoadEventListener.Quiet, component.Value,
                    _useCase.GetInitializationObjects().ToArray());
        }
        catch (Exception e)
        {
            ExceptionViewer.Show(
                $"PreInitialize failed on Explicit (locked component) {component.Value.GetType().Name}", e);
        }
    }

    private void AddBlankComponent(PipelineComponentRole role)
    {
        var toAdd = new DataFlowComponentVisualisation(role, null, component_shouldAllowDrop);
        toAdd.DragDrop += component_DragDrop;
        flpPipelineDiagram.Controls.Add(toAdd);
    }

    private DragDropEffects component_shouldAllowDrop(DragEventArgs arg, DataFlowComponentVisualisation sender)
    {
        var obj = GetAdvertisedObjectFromDragOperation(arg);

        //if they are dragging a new component
        if (obj == null) return DragDropEffects.None;

        //of the correct role and the source/destination is empty
        if (sender.Value == null && obj.GetRole() == sender.GetRole())
            return DragDropEffects.Move;

        return DragDropEffects.None;
    }

    private void component_DragDrop(object sender, DragEventArgs e)
    {
        //so hacky, because we are rewiring source and destination controls to behave like the divider line drop targets we can just route drop events on these controls
        //into the first divider line we see (since they will pop right into the source/destination slots anyway).
        AddAdvertisedComponent(e, flpPipelineDiagram.Controls.OfType<DividerLineControl>().First());
    }


    private DragDropEffects dividerLine_shouldAllowDrop(DragEventArgs arg)
    {
        //if they are dropping a new component
        if (GetAdvertisedObjectFromDragOperation(arg) != null)
            return DragDropEffects.Copy;

        //if its something else entirely
        if (!arg.Data.GetDataPresent(typeof(PipelineComponentVisualisation)))
            return DragDropEffects.None;

        //they are dragging something already on the control (make sure it isn't a source/destination)
        var vis = (PipelineComponentVisualisation)arg.Data.GetData(typeof(PipelineComponentVisualisation));

        //only middle components can be reordered
        return vis.GetRole() == PipelineComponentRole.Middle ? DragDropEffects.Move : DragDropEffects.None;
    }

    private void divider_DragDrop(object sender, DragEventArgs e)
    {
        //get the divider which caused the drop event
        var divider = (DividerLineControl)sender;

        if (GetAdvertisedObjectFromDragOperation(e) != null)
        {
            AddAdvertisedComponent(e, divider);
        }
        else
        {
            var forReorder = (PipelineComponentVisualisation)e.Data.GetData(typeof(PipelineComponentVisualisation));
            HandleReorder(forReorder, divider);
        }

        RefreshUIFromDatabase();
    }

    private void HandleReorder(PipelineComponentVisualisation forReorder, DividerLineControl divider)
    {
        forReorder.PipelineComponent.Order = GetOrderMakingSpaceIfNessesary(forReorder, divider);
        forReorder.PipelineComponent.SaveToDatabase();
    }

    private void AddAdvertisedComponent(DragEventArgs e, DividerLineControl divider)
    {
        var advert = GetAdvertisedObjectFromDragOperation(e);

        if (
            //if user is trying to add a source
            advert.GetRole() == PipelineComponentRole.Source
            //and there is already an explicit source or a configured one
            && (_useCase.ExplicitSource != null || _pipeline.SourcePipelineComponent_ID != null))
        {
            MessageBox.Show("There is already a source in this pipeline");
            return;
        }

        if (
            //if user is trying to add a destination
            advert.GetRole() == PipelineComponentRole.Destination
            //and there is already an explicit destination or a configured one
            && (_useCase.ExplicitDestination != null || _pipeline.DestinationPipelineComponent_ID != null))
        {
            MessageBox.Show("There is already a destination in this pipeline");
            return;
        }

        var underlyingComponentType = advert.GetComponentType();

        //add the component to the pipeline
        var repository = (ICatalogueRepository)_pipeline.Repository;
        var newcomp = new PipelineComponent(repository, _pipeline, underlyingComponentType, -999, advert.ToString())
        {
            Order = GetOrderMakingSpaceIfNessesary(null, divider)
        };


        newcomp.CreateArgumentsForClassIfNotExists(underlyingComponentType);

        newcomp.SaveToDatabase();

        if (advert.GetRole() == PipelineComponentRole.Source)
        {
            _pipeline.SourcePipelineComponent_ID = newcomp.ID;
            _pipeline.SaveToDatabase();
        }

        if (advert.GetRole() == PipelineComponentRole.Destination)
        {
            _pipeline.DestinationPipelineComponent_ID = newcomp.ID;
            _pipeline.SaveToDatabase();
        }

        RefreshUIFromDatabase();


        var viz = flpPipelineDiagram.Controls.OfType<PipelineComponentVisualisation>()
            .Single(v => Equals(v.PipelineComponent, newcomp));

        component_Selected(viz, newcomp);
    }


    private int GetOrderMakingSpaceIfNessesary(PipelineComponentVisualisation beingReorderedIfAny,
        DividerLineControl divider)
    {
        var newOrder = 0;
        var toReturn = 0;

        //for each component in the diagram
        for (var i = 0; i < flpPipelineDiagram.Controls.Count; i++)
        {
            var controlAtIndex = flpPipelineDiagram.Controls[i];

            //do not set the order on the thing being reordered! note that this is null in the case of newly dragged in controls so will never execute continue for new drop operations
            if (controlAtIndex == beingReorderedIfAny)
                continue;

            //found pipeline component
            if (flpPipelineDiagram.Controls[i] is PipelineComponentVisualisation pipelineComponentVisAtIndex)
            {
                //increment the order
                pipelineComponentVisAtIndex.PipelineComponent.Order = newOrder;
                pipelineComponentVisAtIndex.PipelineComponent.SaveToDatabase();
                newOrder++;
            }

            //found the divider this is the order we are supposed to be inserting / reordering into
            if (controlAtIndex == divider)
            {
                //found divider  so mark this as the insertion order point
                toReturn = newOrder;
                newOrder++;
            }
        }

        return toReturn;
    }

    private static AdvertisedPipelineComponentTypeUnderContext GetAdvertisedObjectFromDragOperation(DragEventArgs e) =>
        e.Data is OLVDataObject dataObject && dataObject.ModelObjects.Count == 1 &&
        dataObject.ModelObjects[0] is AdvertisedPipelineComponentTypeUnderContext context
            ? context
            : null;
}