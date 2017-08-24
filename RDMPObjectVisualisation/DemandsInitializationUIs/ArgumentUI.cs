using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline.Requirements.Exceptions;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using RDMPObjectVisualisation.Copying;
using RDMPObjectVisualisation.DemandsInitializationUIs.ArgumentValueControls;
using RDMPObjectVisualisation.Pipelines;
using ReusableLibraryCode.Reflection;
using ReusableUIComponents;
using ReusableUIComponents.SqlDialogs;

namespace RDMPObjectVisualisation.DemandsInitializationUIs
{
    /// <summary>
    /// Allows you to populate a single DemandsInitialization property on an IArgumentHost (See ArgumentCollectionUI for more information)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class ArgumentUI : UserControl
    {
        private Argument _argument;
        private IArgumentHost _parent;
        public DataTable Preview { get; set; }
        
        public ArgumentUI()
        {
            InitializeComponent();
        }

        public void SetProcessTaskArgument(CatalogueRepository catalogueRepository, IArgumentHost parent, Argument argument, DemandsInitialization demand)
        {
            _parent = parent;
            _argument = argument;
            cbRelatedToLoadMetadata.Visible = false;
            tbDescription.Visible = true;
            lblDescription.Visible = true;
            
            pipelineSelectionPanel.Controls.Clear();
            pValueControl.Controls.Clear();

            if (argument == null)
            {
                tbID.Text = "";
                tbName.Text = "";
                tbName.Enabled = false;
                
                tbType.Text = "";

                tbDescription.Text = "";
            }
            else
            {
                ArgumentValueUIFactory valueFactory = new ArgumentValueUIFactory();
                pValueControl.Controls.Add((Control)valueFactory.Create(parent,argument, demand,Preview));

            
                tbID.Text = argument.ID.ToString();
                tbName.Text = argument.Name;
                tbName.Enabled = true;
                
                Type valueAsSystemType = argument.GetSystemType();
                
                //if it is a custom UI driven class display the launch button only
                if (typeof (Pipeline).IsAssignableFrom(valueAsSystemType))
                    HandleDemandForAPipeline(catalogueRepository,argument);

                tbType.Text = argument.Type;
                tbDescription.Text = argument.Description;

            }
        }

        private void HandleDemandForAPipeline(CatalogueRepository catalogueRepository, Argument argument)
        {
            tbDescription.Visible = false;
            lblDescription.Visible = false;
            pipelineSelectionPanel.Visible = true;
            
            if (_parent == null)
                throw new NullReferenceException(
                    "_parent not set when trying to fulfil DemandsInitialization for Property of type Pipeline");

            string typeName = _parent.GetClassNameWhoArgumentsAreFor();

            if (string.IsNullOrWhiteSpace(typeName))
                throw new Exception(_parent.GetType().Name +
                                    " returned an empty string when asked for it's GetClassNameWhoArgumentsAreFor");

            Type typeOfUnderlyingClass = catalogueRepository.MEF.GetTypeByNameFromAnyLoadedAssembly(typeName);

            if (typeOfUnderlyingClass == null)
                throw new Exception("Could not identify a Type called " + typeName + " in any loaded assemblies");

            var interfaces = typeOfUnderlyingClass.GetInterfaces().Where(i =>
                i.IsGenericType && 
                i.GetGenericTypeDefinition() 
                == typeof(IDemandToUseAPipeline<>)).ToArray();

            if(interfaces.Length == 0)
                throw new NotSupportedException("Class " + typeName + " does not implement interface IDemandToUseAPipeline<> despite having a property " + argument.Name + " which is of type Pipeline");

            if(interfaces.Length > 1)
                throw new MultipleMatchingImplmentationException("Class " + typeName + " has multiple interfaces matching IDemandToUseAPipeline<>, a given class can only demand a single Pipeline of a single flow type <T>");

            var instance = Activator.CreateInstance(typeOfUnderlyingClass);
            var flowType = interfaces[0].GenericTypeArguments[0];

            var uiType = typeof (PipelineSelectionUI<>).MakeGenericType(flowType);
            var uiConstructor = uiType.GetConstructors().Single();

            var nfDemander = new LamdaMemberFinder<IDemandToUseAPipeline<object>>();
            var nfUi = new LamdaMemberFinder<PipelineSelectionUI<object>>();
       
            var context = interfaces[0].GetMethod(nfDemander.GetMethod(x => x.GetContext())).Invoke(instance, null);
            var source = interfaces[0].GetMethod(nfDemander.GetMethod(x => x.GetFixedSourceIfAny())).Invoke(instance, null);
            var destination = interfaces[0].GetMethod(nfDemander.GetMethod(x => x.GetFixedDestinationIfAny())).Invoke(instance, null);
            var inputObjects = interfaces[0].GetMethod(nfDemander.GetMethod(x => x.GetInputObjectsForPreviewPipeline())).Invoke(instance, null);
            
            var uiInstance = uiConstructor.Invoke(new object[] { source, destination, catalogueRepository });
            uiType.GetProperty(nfUi.GetProperty(x=>x.Context)).SetValue(uiInstance,context);
            uiType.GetProperty(nfUi.GetProperty(x=>x.InitializationObjectsForPreviewPipeline)).SetValue(uiInstance, inputObjects);

            uiType.GetEvent("PipelineChanged").AddEventHandler(uiInstance, Delegate.CreateDelegate(typeof(EventHandler),this,"OnPipelineChanged"));

            Pipeline p = null;
            try
            {
                p = (Pipeline)argument.GetValueAsSystemType();
            }
            catch (Exception e)
            {
                ExceptionViewer.Show(e);
            }
            uiType.GetProperty("Pipeline").SetValue(uiInstance, p);
            

            var uiInstanceAsControl = (Control) uiInstance;
            uiInstanceAsControl.Dock = DockStyle.Fill;
            pipelineSelectionPanel.Controls.Add(uiInstanceAsControl);

        }


        public void OnPipelineChanged(object sender, EventArgs e)
        {
            var value = sender.GetType().GetProperty("Pipeline").GetValue(sender) as Pipeline;
            
            if(value == null)
                Argument.Value = null;
            else
                Argument.SetValue(value);
            Argument.SaveToDatabase();
        }

        
        public Argument Argument
        {
            get { return _argument; }
        }
        
        private void cbxValue_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

    }
}
