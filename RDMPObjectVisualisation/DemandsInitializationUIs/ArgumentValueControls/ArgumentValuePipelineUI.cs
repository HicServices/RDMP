using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline.Requirements.Exceptions;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using RDMPObjectVisualisation.Pipelines;
using ReusableLibraryCode.Reflection;
using ReusableUIComponents;

namespace RDMPObjectVisualisation.DemandsInitializationUIs.ArgumentValueControls
{
    /// <summary>
    /// Allows you to specify the value of an IArugment (the database persistence value of a [DemandsInitialization] decorated Property on a MEF class e.g. a Pipeline components public property that the user can set)
    /// 
    /// This Control is for setting Properties that are Pipeline, (Requires the class to implement IDemandToUseAPipeline<T>.
    /// </summary>
    [TechnicalUI]
    public partial class ArgumentValuePipelineUI : UserControl, IArgumentValueUI
    {
        private Argument _argument;
        private DemandsInitialization _demand;
        private bool _bLoading = true;

        /// <summary>
        /// is a typeof(PipelineSelectionUI<>)
        /// </summary>
        private Type _pipelineSelectionUIType;
        private object _pipelineSelectionUIInstance;


        public ArgumentValuePipelineUI(CatalogueRepository catalogueRepository, IArgumentHost parent, Type argumentType)
        {
            InitializeComponent();
            string typeName = parent.GetClassNameWhoArgumentsAreFor();

            Type typeOfUnderlyingClass = catalogueRepository.MEF.GetTypeByNameFromAnyLoadedAssembly(typeName);

            if (typeOfUnderlyingClass == null)
                throw new Exception("Could not identify a Type called " + typeName + " in any loaded assemblies");

            var interfaces = typeOfUnderlyingClass.GetInterfaces().Where(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition()
                == typeof(IDemandToUseAPipeline<>)).ToArray();

            if (interfaces.Length == 0)
                throw new NotSupportedException("Class " + typeName + " does not implement interface IDemandToUseAPipeline<> despite having a property which is a Pipeline");

            if (interfaces.Length > 1)
                throw new MultipleMatchingImplmentationException("Class " + typeName + " has multiple interfaces matching IDemandToUseAPipeline<>, a given class can only demand a single Pipeline of a single flow type <T>");

            var instanceOfParentType = Activator.CreateInstance(typeOfUnderlyingClass);
            var flowType = interfaces[0].GenericTypeArguments[0];

            _pipelineSelectionUIType = typeof(PipelineSelectionUI<>).MakeGenericType(flowType);
            var uiConstructor = _pipelineSelectionUIType.GetConstructors().Single();

            var nfDemander = new LamdaMemberFinder<IDemandToUseAPipeline<object>>();
            var nfUi = new LamdaMemberFinder<PipelineSelectionUI<object>>();

            var context = interfaces[0].GetMethod(nfDemander.GetMethod(x => x.GetContext())).Invoke(instanceOfParentType, null);
            var source = interfaces[0].GetMethod(nfDemander.GetMethod(x => x.GetFixedSourceIfAny())).Invoke(instanceOfParentType, null);
            var destination = interfaces[0].GetMethod(nfDemander.GetMethod(x => x.GetFixedDestinationIfAny())).Invoke(instanceOfParentType, null);
            var inputObjects = interfaces[0].GetMethod(nfDemander.GetMethod(x => x.GetInputObjectsForPreviewPipeline())).Invoke(instanceOfParentType, null);

            _pipelineSelectionUIInstance = uiConstructor.Invoke(new object[] { source, destination, catalogueRepository });
            _pipelineSelectionUIType.GetProperty(nfUi.GetProperty(x => x.Context)).SetValue(_pipelineSelectionUIInstance, context);
            _pipelineSelectionUIType.GetProperty(nfUi.GetProperty(x => x.InitializationObjectsForPreviewPipeline)).SetValue(_pipelineSelectionUIInstance, inputObjects);

            _pipelineSelectionUIType.GetEvent("PipelineChanged").AddEventHandler(_pipelineSelectionUIInstance, Delegate.CreateDelegate(typeof(EventHandler), this, "OnPipelineChanged"));
            
            var c = (Control) _pipelineSelectionUIInstance;
            c.Width = ragSmiley1.Left;
            c.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            _pipelineSelectionUIType.GetMethod("CollapseToSingleLineMode").Invoke(_pipelineSelectionUIInstance, new object[0]);

            var uiInstanceAsControl = (Control)_pipelineSelectionUIInstance;
            Controls.Add(uiInstanceAsControl);
        }
        
        public void OnPipelineChanged(object sender, EventArgs e)
        {
            if(_bLoading)
                return;

            var value = sender.GetType().GetProperty("Pipeline").GetValue(sender) as Pipeline;

            if (value == null)
                _argument.Value = null;
            else
                _argument.SetValue(value);
            _argument.SaveToDatabase();
        }

        public void SetUp(Argument argument, DemandsInitialization demand, DataTable previewIfAny)
        {
            _bLoading = true;
            _argument = argument;
            _demand = demand;
            
            try
            {
                Pipeline p = null;
                try
                {
                    p = (Pipeline)argument.GetValueAsSystemType();
                }
                catch (Exception e)
                {
                    ExceptionViewer.Show(e);
                }
                _pipelineSelectionUIType.GetProperty("Pipeline").SetValue(_pipelineSelectionUIInstance, p);
            }
            catch (Exception e)
            {
                ragSmiley1.Fatal(e);
            }

            BombIfMandatoryAndEmpty();
            _bLoading = false;
        }

        private void BombIfMandatoryAndEmpty()
        {
            var pipe = _pipelineSelectionUIType.GetProperty("Pipeline").GetValue(_pipelineSelectionUIInstance);

            if (_demand.Mandatory && pipe == null)
                ragSmiley1.Fatal(new Exception("Property is Mandatory which means it you have to Type an appropriate input in"));
        }
    }
}
