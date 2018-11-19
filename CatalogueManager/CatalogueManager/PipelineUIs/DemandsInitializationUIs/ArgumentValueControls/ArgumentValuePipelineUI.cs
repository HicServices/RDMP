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
using CatalogueManager.PipelineUIs.Pipelines;
using CatalogueManager.PipelineUIs.Pipelines.PluginPipelineUsers;
using ReusableUIComponents;

namespace CatalogueManager.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls
{
    /// <summary>
    /// Allows you to specify the value of an IArugment (the database persistence value of a [DemandsInitialization] decorated Property on a MEF class e.g. a Pipeline components public property that the user can set)
    /// 
    /// <para>This Control is for setting Properties that are Pipeline, (Requires the class to implement <see cref="IDemandToUseAPipeline"/>).</para>
    /// </summary>
    [TechnicalUI]
    public partial class ArgumentValuePipelineUI : UserControl, IArgumentValueUI
    {
        private IPipelineSelectionUI _pipelineSelectionUIInstance;
        private Type _typeOfUnderlyingClass;


        public ArgumentValuePipelineUI(CatalogueRepository catalogueRepository, IArgumentHost parent, Type argumentType)
        {
            InitializeComponent();

            string typeName = parent.GetClassNameWhoArgumentsAreFor();

            _typeOfUnderlyingClass = catalogueRepository.MEF.GetTypeByNameFromAnyLoadedAssembly(typeName);

            if (_typeOfUnderlyingClass == null)
                throw new Exception("Could not identify a Type called " + typeName + " in any loaded assemblies");

        }
        
        public void SetUp(ArgumentValueUIArgs args)
        {
            var instanceOfParentType = Activator.CreateInstance(_typeOfUnderlyingClass);

            var factory = new PipelineSelectionUIFactory(args.CatalogueRepository,args.Required,args, instanceOfParentType);
            _pipelineSelectionUIInstance = factory.Create();
            _pipelineSelectionUIInstance.CollapseToSingleLineMode();

            var c = (Control)_pipelineSelectionUIInstance;
            Controls.Add(c);

            try
            {
                Pipeline p = null;
                try
                {
                    p = (Pipeline)args.InitialValue;
                }
                catch (Exception e)
                {
                    ExceptionViewer.Show(e);
                }
            }
            catch (Exception e)
            {
                args.Fatal(e);
            }
        }
    }
}
