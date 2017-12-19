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
using RDMPObjectVisualisation.DataObjects;
using RDMPObjectVisualisation.DemandsInitializationUIs;
using RDMPObjectVisualisation.Pipelines.Models;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using ReusableUIComponents;
using ReusableUIComponents.ChecksUI;

namespace RDMPObjectVisualisation.Pipelines
{

    public delegate void PipelineComponentSelectedHandler(object sender, IPipelineComponent selected);

    /// <summary>
    /// Used to visualise an IPipeline (See ConfigurePipelineUI and ConfigureAndExecutePipeline for what these are).  This control has a readonly/editable setting on it.  In dialogs where
    /// you are selecting an IPipeline you will see the diagram rendered readonly.  If you are editting  (See PipelineWorkArea and ConfigurePipelineUI) then you will be able to select
    /// and drag and drop in new components to make an IPipeline configuration.  On such a dialog you can also select a component to change the components arguments (See ArgumentCollection).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class PipelineDiagram<T> : UserControl
    {
        DataObjectVisualisationFactory _visualisationFactory = new DataObjectVisualisationFactory();

        Label pipelineBroken = new Label();

        private DataFlowPipelineEngineFactory<T> _pipelineFactory;
        private IPipeline _pipeline;
        private object[] _initializationObjects;
        
        public bool AllowSelection { get; set; }
        public bool AllowReOrdering { get; set; }

        /// <summary>
        /// Used only when dropping in new components, can be left as null if you are unsure how to generate an appropriate preview for components in your pipeline
        /// </summary>
        public DataTable Preview { get; set; }

        public IPipelineComponent SelectedComponent;
        public event PipelineComponentSelectedHandler SelectedComponentChanged;

        private readonly Dictionary<object, HashSet<Control>> _initializationObjectsConsumedAnchorPoints_ClientCoordinates = new Dictionary<object, HashSet<Control>>();

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
            SetTo(_pipelineFactory,_pipeline,_initializationObjects);
        }


        public void Clear()
        {
            _pipeline = null;
            if (_pipelineFactory == null)//it is already cleared?
                return;
            SetTo(_pipelineFactory, null, _initializationObjects);
        }

        public void SetTo(DataFlowPipelineEngineFactory<T> pipelineFactory, IPipeline pipeline,object [] initializationObjects)
        {
            if(_pipelineFactory != pipelineFactory)
            {
                _pipelineFactory = pipelineFactory;
                _pipelineFactory.Context.ObjectInitialized += ContextOnObjectInitialized;
            }
            
            _pipeline = pipeline;
            _initializationObjectsConsumedAnchorPoints_ClientCoordinates.Clear();//nothing has been consumed yet

            _initializationObjects = initializationObjects;

            //clear the diagram
            flpPipelineDiagram.Controls.Clear();
            pipelineBroken.Visible = false;

            if(pipelineFactory == null)
                throw new Exception("Pipeline Factory must be set");

            bool pipelineCheckingFailed = false;
            IDataFlowPipelineEngine pipelineInstance = null;
            try
            {
                //if there is a pipeline
                if(pipeline != null)
                {
                    try
                    {
                        //create it
                        pipelineInstance = pipelineFactory.Create(pipeline, new ThrowImmediatelyDataLoadEventListener());
                    
                        try
                        {
                            //initialize it
                            pipelineInstance.Initialize(_initializationObjects);
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
                    if (pipelineFactory.ExplicitSource != null)
                        AddExplicit(pipelineFactory.ExplicitSource);//if so add it
                    else 
                        //there wasn't an explicit one so there was a PipelineComponent maybe? albiet one that might be broken?
                        if (pipeline.SourcePipelineComponent_ID != null)
                            AddPipelineComponent((int) pipeline.SourcePipelineComponent_ID,DataFlowComponentVisualisation.Role.Source, pipeline.Repository);//add the possibly broken PipelineComponent to the diagram
                        else
                            AddBlankComponent(DataFlowComponentVisualisation.Role.Source);//the user hasn't put one in yet 


                    foreach (var middleComponent in pipeline.PipelineComponents.Where( c=>c.ID!= pipeline.SourcePipelineComponent_ID && c.ID != pipeline.DestinationPipelineComponent_ID).OrderBy(comp=>comp.Order))
                        AddPipelineComponent(middleComponent, DataFlowComponentVisualisation.Role.Middle);//add the possibly broken PipelineComponent to the diagram
               
                    //was there an explicit instance?
                    if (pipelineFactory.ExplicitDestination != null)
                    {
                        AddDividerIfReorderingAvailable();
                        AddExplicit(pipelineFactory.ExplicitDestination);//if so add it
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
                if (pipelineFactory.ExplicitSource == null)
                    AddBlankComponent(DataFlowComponentVisualisation.Role.Source);
                else
                    AddExplicit(pipelineFactory.ExplicitSource);


                //factory has no source, add empty source
                if (pipelineFactory.ExplicitDestination == null)
                    AddBlankComponent(DataFlowComponentVisualisation.Role.Destination);
                else
                    AddExplicit(pipelineFactory.ExplicitDestination);
            }
            finally
            {
                Invalidate();
            }
        }

        private void ContextOnObjectInitialized(object componentBeingInitialized, object valueBeingConsumed)
        {
            //the context ran PreInitialize successfully on a component!
            foreach (
                Control component in
                    flpPipelineDiagram.Controls)
            {

                var visDataFlowComponent = component as DataFlowComponentVisualisation;
                var visPipelineComponent = component as PipelineComponentVisualisation;

                //see if the thing being initialized was:
                if (
                    //a fixed data flow component was being initialized!
                    (visDataFlowComponent != null && visDataFlowComponent.Value == componentBeingInitialized)
                    ||
                    //a plugin pipeline component was being initialized!
                    (visPipelineComponent != null && visPipelineComponent.PipelineComponent == componentBeingInitialized))
                {
                    
                    //either way this control was responsible for the consumption so mark the point of the control as one of the consume points of the input

                    //add it to the dictionary if it isn't there yet
                    if (!_initializationObjectsConsumedAnchorPoints_ClientCoordinates.ContainsKey(valueBeingConsumed))
                        _initializationObjectsConsumedAnchorPoints_ClientCoordinates.Add(valueBeingConsumed,new HashSet<Control>());

                    //and then calcualte a point along the top of the control
                    _initializationObjectsConsumedAnchorPoints_ClientCoordinates[valueBeingConsumed].Add(component);
                }
            }
        }

        //by ID overload
        private void AddPipelineComponent(int componentID, DataFlowComponentVisualisation.Role role, IRepository repository)
        {
            AddPipelineComponent(repository.GetObjectByID<PipelineComponent>(componentID), role);
        }

        private Point ControlToClientPoint(Control component)
        {
            return new Point(component.Left + (component.Width / 2), component.Top + 10);
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
                    _pipelineFactory.Context.PreInitializeGeneric(new ThrowImmediatelyDataLoadEventListener(), value, _initializationObjects);
                    component.ExInitialization = null;//initialization worked, changes N/A to Successful
                    component.Check();
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
            var component = (DataFlowComponentVisualisation)_visualisationFactory.Create(value);
            flpPipelineDiagram.Controls.Add(component);//add the explicit component
            component.IsLocked = true;
            try
            {
                _pipelineFactory.Context.PreInitializeGeneric(new ThrowImmediatelyDataLoadEventListener(),component.Value,_initializationObjects);
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
                _pipelineFactory.Check(_pipeline,popupChecksUI,_initializationObjects);
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
                && (_pipelineFactory.ExplicitSource != null || _pipeline.SourcePipelineComponent_ID != null))
            {
                MessageBox.Show("There is already a source in this pipeline");
                return;
            }

            if (
                //if user is trying to add a destination
               advert.GetRole() == DataFlowComponentVisualisation.Role.Destination
                //and there is already an explicit destination or a configured one
               && (_pipelineFactory.ExplicitDestination != null || _pipeline.DestinationPipelineComponent_ID != null))
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

        private AdvertisedPipelineComponentTypeUnderContext<T> GetAdvertisedObjectFromDragOperation(DragEventArgs e)
        {
            OLVDataObject dataObject = e.Data as OLVDataObject;
            if (dataObject != null)
            {
                if (dataObject.ModelObjects.Count == 1 &&
                    dataObject.ModelObjects[0] is AdvertisedPipelineComponentTypeUnderContext<T>)
                    return (AdvertisedPipelineComponentTypeUnderContext<T>)dataObject.ModelObjects[0];

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

        private void flpPipelineDiagram_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            foreach (HashSet<Control> anchorPoints in _initializationObjectsConsumedAnchorPoints_ClientCoordinates.Values)
            {
                foreach (Point anchorPoint in anchorPoints.Select(ControlToClientPoint))
                {
                    g.FillEllipse(new SolidBrush(Color.Black), new Rectangle(anchorPoint,new Size(5,5)));        
                }

            }
            
        }

        public Dictionary<object, HashSet<Point>> GetAnchorPointsInScreenSpace()
        {
            var toReturn = new Dictionary<object, HashSet<Point>>();

            //for each client coordinate
            foreach (KeyValuePair<object, HashSet<Control>> kvp in _initializationObjectsConsumedAnchorPoints_ClientCoordinates)
            {
                //add it to the return dictionary
                toReturn.Add(kvp.Key,new HashSet<Point>());

                //wheres the point Mr Wolf? - depends where the user is dragging around the control!
                foreach (Point point in kvp.Value.Select(ControlToClientPoint))
                    toReturn[kvp.Key].Add(flpPipelineDiagram.PointToScreen(point));
            }

            return toReturn;
        }

    }
}

