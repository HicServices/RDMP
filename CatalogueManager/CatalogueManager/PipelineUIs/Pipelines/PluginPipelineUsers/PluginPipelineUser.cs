using System;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline.Requirements;

namespace CatalogueManager.PipelineUIs.Pipelines.PluginPipelineUsers
{
    /// <summary>
    /// Turns an IDemandToUseAPipeline plugin class into an IPipelineUser and IPipelineUseCase (both) for use with PipelineSelectionUIFactory
    /// </summary>
    public sealed class PluginPipelineUser : PipelineUseCase,IPipelineUser
    {
        private IPipelineUseCase _useCase;
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

            _useCase = pipeDemander.GetDesignTimePipelineUseCase(demand);
            
            ExplicitSource = _useCase.ExplicitSource;
            ExplicitDestination = _useCase.ExplicitDestination;

            foreach (var o in GetInitializationObjects())
                AddInitializationObject(o);

            GenerateContext();
        }
        
        protected override IDataFlowPipelineContext GenerateContextImpl()
        {
            return _useCase.GetContext();
        }
    }
}