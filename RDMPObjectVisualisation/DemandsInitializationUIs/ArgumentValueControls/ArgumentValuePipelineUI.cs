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
using RDMPObjectVisualisation.Pipelines.PluginPipelineUsers;
using ReusableUIComponents;

namespace RDMPObjectVisualisation.DemandsInitializationUIs.ArgumentValueControls
{
    /// <summary>
    /// Allows you to specify the value of an IArugment (the database persistence value of a [DemandsInitialization] decorated Property on a MEF class e.g. a Pipeline components public property that the user can set)
    /// 
    /// <para>This Control is for setting Properties that are Pipeline, (Requires the class to implement IDemandToUseAPipeline<T>).</para>
    /// </summary>
    [TechnicalUI]
    public partial class ArgumentValuePipelineUI : UserControl, IArgumentValueUI
    {
        private readonly CatalogueRepository _catalogueRepository;
        private Argument _argument;
        private DemandsInitializationAttribute _demand;
        private bool _bLoading = true;

        /// <summary>
        /// is a typeof(PipelineSelectionUI<>)
        /// </summary>
        private IPipelineSelectionUI _pipelineSelectionUIInstance;
        private Type _typeOfUnderlyingClass;


        public ArgumentValuePipelineUI(CatalogueRepository catalogueRepository, IArgumentHost parent, Type argumentType)
        {
            _catalogueRepository = catalogueRepository;
            InitializeComponent();

            string typeName = parent.GetClassNameWhoArgumentsAreFor();

            _typeOfUnderlyingClass = catalogueRepository.MEF.GetTypeByNameFromAnyLoadedAssembly(typeName);

            if (_typeOfUnderlyingClass == null)
                throw new Exception("Could not identify a Type called " + typeName + " in any loaded assemblies");

        }
        
        public void SetUp(Argument argument, RequiredPropertyInfo requirement, DataTable previewIfAny)
        {
            _bLoading = true;
            _argument = argument;
            _demand = requirement.Demand;

            var instanceOfParentType = Activator.CreateInstance(_typeOfUnderlyingClass);

            var factory = new PipelineSelectionUIFactory(_catalogueRepository,requirement,argument, instanceOfParentType);
            _pipelineSelectionUIInstance = factory.Create();
            _pipelineSelectionUIInstance.CollapseToSingleLineMode();
            _pipelineSelectionUIInstance.PipelineChanged += (s, e) => BombIfMandatoryAndEmpty();

            var c = (Control)_pipelineSelectionUIInstance;
            c.Width = ragSmiley1.Left;

            ragSmiley1.Reset();

            Controls.Add(c);

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
                
                BombIfMandatoryAndEmpty();
            }
            catch (Exception e)
            {
                ragSmiley1.Fatal(e);
            }

            _bLoading = false;
        }

        private void BombIfMandatoryAndEmpty()
        {
            ragSmiley1.Reset();

            var pipe = _argument.GetValueAsSystemType();

            if (_demand.Mandatory && pipe == null)
                ragSmiley1.Fatal(new Exception("Property is Mandatory which means it you have to Type an appropriate input in"));
        }
    }
}
