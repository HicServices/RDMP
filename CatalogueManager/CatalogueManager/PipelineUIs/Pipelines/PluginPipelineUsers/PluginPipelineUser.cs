using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.DataFlowPipeline.Requirements.Exceptions;
using CatalogueLibrary.Repositories;

namespace CatalogueManager.PipelineUIs.Pipelines.PluginPipelineUsers
{
    /// <summary>
    /// Turns an IDemandToUseAPipeline plugin class into an IPipelineUser and IPipelineUseCase (both) for use with PipelineSelectionUIFactory
    /// </summary>
    public class PluginPipelineUser : PipelineUseCase,IPipelineUser
    {
        private object _context;
        private object[] _inputObjects;

        public PipelineGetter Getter { get; private set; }
        public PipelineSetter Setter { get; private set; }

        public PluginPipelineUser( RequiredPropertyInfo demand,IArgument argument, object demanderInstance)
        {
            Getter = () =>
            {
                var p = (Pipeline) argument.GetValueAsSystemType();
                return p;
            };

            Setter = v =>
            {
                argument.SetValue(v);
                demand.PropertyInfo.SetValue(demanderInstance, v);
                argument.SaveToDatabase();
            };

            var pipeDemander = demanderInstance as IDemandToUseAPipeline;

            if (pipeDemander == null)
                throw new NotSupportedException("Class " + pipeDemander.GetType().Name + " does not implement interface IDemandToUseAPipeline despite having a property which is a Pipeline");

            var useCase = pipeDemander.GetDesignTimePipelineUseCase(demand);
            
            _context = useCase.GetContext();
            ExplicitSource = useCase.ExplicitSource;
            ExplicitDestination = useCase.ExplicitDestination;
            _inputObjects = useCase.GetInitializationObjects();
        }

        public override object[] GetInitializationObjects()
        {
            return _inputObjects ??new Object[0];
        }
        
        public override IDataFlowPipelineContext GetContext()
        {
            return (IDataFlowPipelineContext) _context;
        }
        
    }
}