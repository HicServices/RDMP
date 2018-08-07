using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using CatalogueManager.PipelineUIs.DataObjects;
using CatalogueManager.PipelineUIs.DemandsInitializationUIs;
using CatalogueManager.PipelineUIs.Pipelines.Models;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using ReusableUIComponents;
using ReusableUIComponents.ChecksUI;

namespace CatalogueManager.PipelineUIs.Pipelines
{

    public delegate void PipelineComponentSelectedHandler(object sender, IPipelineComponent selected);

    /// <summary>
    /// Used to visualise an IPipeline (See ConfigurePipelineUI and ConfigureAndExecutePipeline for what these are).  This control has a readonly/editable setting on it.  In dialogs where
    /// you are selecting an IPipeline you will see the diagram rendered readonly.  If you are editting  (See PipelineWorkArea and ConfigurePipelineUI) then you will be able to select
    /// and drag and drop in new components to make an IPipeline configuration.  On such a dialog you can also select a component to change the components arguments (See ArgumentCollection).
    /// </summary>
    public partial class PipelineDiagram : UserControl
    {
        Label pipelineBroken = new Label();

        private IPipeline _pipeline;
        
        public bool AllowSelection { get; set; }
        public bool AllowReOrdering { get; set; }

        /// <summary>
        /// Used only when dropping in new components, can be left as null if you are unsure how to generate an appropriate preview for components in your pipeline
        /// </summary>
        public DataTable Preview { get; set; }

        public IPipelineComponent SelectedComponent;
        public event PipelineComponentSelectedHandler SelectedComponentChanged;

        private IPipelineUseCase _useCase;
        private DataFlowPipelineEngineFactory _pipelineFactory;

        public PipelineDiagram()
        {
            InitializeComponent();
            AllowSelection = false;


            this.Controls.Add(pipelineBroken);
            pipelineBroken.AutoSize = true;
            pipelineBroken.Anchor  = AnchorStyles.None;
            pipelineBroken.Left = this.Width/2;
            pipelineBroken.Top = this.Height/2;
            pipelineBroken.ForeColor = Color.Red;
            pipelineBroken.Visible = false;
            pipelineBroken.Text = "Pipeline Broken";
            pipelineBroken.Click += pipelineBroken_Click;
        }


        public void RefreshUIFromDatabase()
        {
            SetTo(_pipeline,_useCase);
        }


        public void Clear()
        {
            _pipeline = null;
            if (_useCase == null)//it is already cleared?
                return;
            SetTo(null,_useCase);
        }

        public void SetTo(IPipeline pipeline, IPipelineUseCase useCase)
        {
            _useCase = useCase;

            _pipeline = pipeline;
            _pipeline.ClearAllInjections();
            
            //clear the diagram
            flpPipelineDiagram.Controls.Clear();
            pipelineBroken.Visible = false;
            
            bool pipelineCheckingFailed = false;
            try
            {
                //if there is a pipeline
                if (_pipeline != null)
                {
                    try
                    {
                        _pipelineFactory = new DataFlowPipelineEngineFactory(_useCase, _pipeline);

                        //create it
                        IDataFlowPipelineEngine pipelineInstance = _pipelineFactory.Create(pipeline, new ThrowImmediatelyDataLoadEventListener());
                    
                        try
                        {
                            //initialize it (unless it is design time)
                            if(!_useCase.IsDesignTime)
                                pipelineInstance.Initialize(_useCase.GetInitializationObjects());
                        }
                        catch (Exception )
                        {
                            pipelineCheckingFailed = true;
                        }
                    }
                    catch (Exception )
                    {
                        pipelineCheckingFailed = true;
                    }
                

                    //if it failed to passed checks
                    if (pipelineCheckingFailed)
                    {
                        pipelineBroken.BringToFront();
                        pipelineBroken.Visible = true;
                    }

                    //There is a pipeline set but we might have been unable to fully realize it so setup stuff based on PipelineComponents
                
                    //was there an explicit instance?
                    if (_useCase.ExplicitSource != null)
                        AddExplicit(_useCase.ExplicitSource);//if so add it
                    else 
                        //there wasn't an explicit one so there was a PipelineComponent maybe? albiet one that might be broken?
                        if (pipeline.SourcePipelineComponent_ID != null)
                            AddPipelineComponent((int) pipeline.SourcePipelineComponent_ID,DataFlowComponentVisualisation.Role.Source, pipeline.Repository);//add the possibly broken PipelineComponent to the diagram
                        else
                            AddBlankComponent(DataFlowComponentVisualisation.Role.Source);//the user hasn't put one in yet 


                    foreach (var middleComponent in pipeline.PipelineComponents.Where( c=>c.ID!= pipeline.SourcePipelineComponent_ID && c.ID != pipeline.DestinationPipelineComponent_ID).OrderBy(comp=>comp.Order))
                        AddPipelineComponent(middleComponent, DataFlowComponentVisualisation.Role.Middle);//add the possibly broken PipelineComponent to the diagram
               
                    //was there an explicit instance?
                    if (_useCase.ExplicitDestination != null)
                    {
                        AddDividerIfReorderingAvailable();
                        AddExplicit(_useCase.ExplicitDestination);//if so add it
                    }
                    else
                        //there wasn't an explicit one so there was a PipelineComponent maybe? albiet one that might be broken?
                        if (pipeline.DestinationPipelineComponent_ID != null)
                            AddPipelineComponent((int)pipeline.DestinationPipelineComponent_ID, DataFlowComponentVisualisation.Role.Destination, pipeline.Repository);//add the possibly broken PipelineComponent to the diagram
                        else
                        {
                            AddDividerIfReorderingAvailable();
                            AddBlankComponent(DataFlowComponentVisualisation.Role.Destination);//the user hasn't put one in yet 
                        }


                    return;
                }
            

                //Fallback 
                //user has not picked a pipeline yet, show him the shell (factory)
                //factory has no source, add empty source
                if (_useCase.ExplicitSource == null)
                    AddBlankComponent(DataFlowComponentVisualisation.Role.Source);
                else
                    AddExplicit(_useCase.ExplicitSource);


                //factory has no source, add empty source
                if (_useCase.ExplicitDestination == null)
                    AddBlankComponent(DataFlowComponentVisualisation.Role.Destination);
                else
                    AddExplicit(_useCase.ExplicitDestination);
            }
            finally
            {
                Invalidate();
            }
        }

        //by ID overload
        private void AddPipelineComponent(int componentID, DataFlowComponentVisualisation.Role role, IRepository repository)
        {
            AddPipelineComponent(repository.GetObjectByID<PipelineComponent>(componentID), role);
        }

        private void AddPipelineComponent(IPipelineComponent toRealize, DataFlowComponentVisualisation.Role role)
        {
            Exception exConstruction;
            
            //create the pipeline realization (might fail
            var value = _pipelineFactory.TryCreateComponent(_pipeline, toRealize, out exConstruction);

            if (role != DataFlowComponentVisualisation.Role.Source)
                AddDividerIfReorderingAvailable();

            //create the visualization
            var component = new PipelineComponentVisualisation(toRealize, role, value, exConstruction, component_shouldAllowDrop);
            component.DragDrop += component_DragDrop;
            flpPipelineDiagram.Controls.Add(component);//add the component

            //try to initialize the realization (we do this after creating visualization so that when the events come in we can record where on UI the consumption events happen and also because later on it might be that initialization takes ages and we want to use a Thread)
            if (value != null)
                try
                {
                    if (!_useCase.IsDesignTime)
                    {
                        _useCase.GetContext().PreInitializeGeneric(new ThrowImmediatelyDataLoadEventListener(), value, _useCase.GetInitializationObjects());
                        component.Clear(); //initialization worked, changes N/A to Successful
                        component.Check();
                    }
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
             if(!AllowReOrdering)
                 return;

            DividerLineControl divider = new DividerLineControl(dividerLine_shouldAllowDrop);
            divider.DragDrop += divider_DragDrop;
            flpPipelineDiagram.Controls.Add(divider);
        }


        void component_Selected(object sender, IPipelineComponent selected)
        {
            if (!AllowSelection)
                return;

            SelectedComponent = selected;
            
            //clear old selections
            foreach (PipelineComponentVisualisation componentVisualisation in flpPipelineDiagram.Controls.OfType<PipelineComponentVisualisation>())
                componentVisualisation.IsSelected = false;

            ((PipelineComponentVisualisation) sender).IsSelected = true;
            SelectedComponentChanged(this, selected);

            this.Focus();
        }

        private void AddExplicit(object value)
        {

            var role = DataFlowComponentVisualisation.GetRoleFor(value.GetType());

            var component = new DataFlowComponentVisualisation(role,value,null);
            flpPipelineDiagram.Controls.Add(component);//add the explicit component
            component.IsLocked = true;
            try
            {
                if (!_useCase.IsDesignTime)
                    _useCase.GetContext().PreInitializeGeneric(new ThrowImmediatelyDataLoadEventListener(), component.Value, _useCase.GetInitializationObjects());
            }
            catch (Exception e)
            {
                ExceptionViewer.Show("PreInitialize failed on Explicit (locked component) " + component.Value.GetType().Name ,e);
            }
        }
        
        private void AddBlankComponent(DataFlowComponentVisualisation.Role role)
        {
            DataFlowComponentVisualisation toAdd = new DataFlowComponentVisualisation(role, null, component_shouldAllowDrop);
            toAdd.DragDrop += component_DragDrop;
            flpPipelineDiagram.Controls.Add(toAdd);
        }

        void pipelineBroken_Click(object sender, EventArgs e)
        {
            PopupChecksUI popupChecksUI = new PopupChecksUI("Why is pipeline broken?",false);
            try
            {

                _pipelineFactory.Check(_pipeline, popupChecksUI, _useCase.GetInitializationObjects().ToArray());
            }
            catch (Exception exception)
            {
                popupChecksUI.OnCheckPerformed(new CheckEventArgs("Checking crashed", CheckResult.Fail, exception));
            }

            try
            {
                _pipelineFactory.Create(_pipeline, new FromCheckNotifierToDataLoadEventListener(popupChecksUI));
            }
            catch (Exception exception)
            {
                popupChecksUI.OnCheckPerformed(new CheckEventArgs("Instance instantiation crashed", CheckResult.Fail, exception));
            }

        }
        
        private DragDropEffects component_shouldAllowDrop(DragEventArgs arg,DataFlowComponentVisualisation sender)
        {
            var obj = GetAdvertisedObjectFromDragOperation(arg);
            
            //if they are dragging a new component
            if(obj != null)
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
            if (vis.GetRole() == DataFlowComponentVisualisation.Role.Middle)
                return DragDropEffects.Move;
            
            return DragDropEffects.None;
        }

        void divider_DragDrop(object sender, DragEventArgs e)
        {
            //get the divider which caused the drop event
            var divider = (DividerLineControl)sender;

            if (GetAdvertisedObjectFromDragOperation(e) != null)
                AddAdvertisedComponent(e,divider);
            else
            {
                var forReorder = (PipelineComponentVisualisation)e.Data.GetData(typeof(PipelineComponentVisualisation));
                HandleReorder(forReorder,divider);
            }

            RefreshUIFromDatabase();
        }
        
        private void HandleReorder(PipelineComponentVisualisation forReorder, DividerLineControl divider)
        {
            forReorder.PipelineComponent.Order =  GetOrderMakingSpaceIfNessesary(forReorder,divider);
            forReorder.PipelineComponent.SaveToDatabase();
        }

        private void AddAdvertisedComponent(DragEventArgs e,DividerLineControl divider)
        {
            var advert = GetAdvertisedObjectFromDragOperation(e);

            if (
                //if user is trying to add a source
                advert.GetRole() == DataFlowComponentVisualisation.Role.Source
                //and there is already an explicit source or a configured one
                && (_useCase.ExplicitSource != null || _pipeline.SourcePipelineComponent_ID != null))
            {
                MessageBox.Show("There is already a source in this pipeline");
                return;
            }

            if (
                //if user is trying to add a destination
               advert.GetRole() == DataFlowComponentVisualisation.Role.Destination
                //and there is already an explicit destination or a configured one
               && (_useCase.ExplicitDestination != null || _pipeline.DestinationPipelineComponent_ID != null))
            {
                MessageBox.Show("There is already a destination in this pipeline");
                return;
            }

            Type underlyingComponentType = advert.GetComponentType();

            //add the component to the pipeline
            var repository = (ICatalogueRepository)_pipeline.Repository;
            var newcomp = new PipelineComponent(repository, _pipeline, underlyingComponentType, -999, advert.ToString())
            {
                Order = GetOrderMakingSpaceIfNessesary(null, divider)
            };

           
            newcomp.CreateArgumentsForClassIfNotExists(underlyingComponentType);
          
            newcomp.SaveToDatabase();
            
            ArgumentCollection.ShowDialogIfAnyArgs((CatalogueRepository)repository, newcomp, underlyingComponentType, Preview);
            
            if (advert.GetRole() == DataFlowComponentVisualisation.Role.Source)
            {
                _pipeline.SourcePipelineComponent_ID = newcomp.ID;
                _pipeline.SaveToDatabase();
            }

            if (advert.GetRole() == DataFlowComponentVisualisation.Role.Destination)
            {
                _pipeline.DestinationPipelineComponent_ID = newcomp.ID;
                _pipeline.SaveToDatabase();
            } 

            RefreshUIFromDatabase();
        }

        private int GetOrderMakingSpaceIfNessesary(PipelineComponentVisualisation beingReorderedIfAny, DividerLineControl divider)
        {
            int newOrder = 0;
            int toReturn = 0;

            //for each component in the diagram
            for (int i = 0; i < flpPipelineDiagram.Controls.Count; i++)
            {
                var controlAtIndex = flpPipelineDiagram.Controls[i];
                var pipelineComponentVisAtIndex = flpPipelineDiagram.Controls[i] as PipelineComponentVisualisation;

                //do not set the order on the thing being reordered! note that this is null in the case of newly dragged in controls so will never execute continue for new drop operations
                if (controlAtIndex == beingReorderedIfAny)
                    continue;

                //found pipeline component
                if (pipelineComponentVisAtIndex != null)
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

        private AdvertisedPipelineComponentTypeUnderContext GetAdvertisedObjectFromDragOperation(DragEventArgs e)
        {
            OLVDataObject dataObject = e.Data as OLVDataObject;
            if (dataObject != null)
            {
                if (dataObject.ModelObjects.Count == 1 &&
                    dataObject.ModelObjects[0] is AdvertisedPipelineComponentTypeUnderContext)
                    return (AdvertisedPipelineComponentTypeUnderContext)dataObject.ModelObjects[0];

                return null;
            }
            return null;
        }

        protected override bool ProcessKeyPreview(ref Message m)
        {
            PreviewKey p = new PreviewKey(ref m, ModifierKeys);

            if (p.IsKeyDownMessage && p.e.KeyCode == Keys.Delete)
            {
                if (SelectedComponent != null)
                {
                    if(MessageBox.Show("Do you want to delete " + SelectedComponent.Class + "?","Confirm Delete",MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
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
                    }

                    p.Trap(this);
                }
            }

            return base.ProcessKeyPreview(ref m);
        }
    }
}

