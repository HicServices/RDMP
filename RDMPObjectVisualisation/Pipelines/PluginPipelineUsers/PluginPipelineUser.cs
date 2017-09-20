using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.DataFlowPipeline.Requirements.Exceptions;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Reflection;

namespace RDMPObjectVisualisation.Pipelines.PluginPipelineUsers
{
    /// <summary>
    /// Turns an IDemandToUseAPipeline plugin class into an IPipelineUser and IPipelineUseCase (both) for use with PipelineSelectionUIFactory
    /// </summary>
    public class PluginPipelineUser : PipelineUseCase,IPipelineUser
    {
        private object _context;
        private List<object> _inputObjects;

        public PipelineGetter Getter { get; private set; }
        public PipelineSetter Setter { get; private set; }

        public PluginPipelineUser(RequiredPropertyInfo demand,IArgument argument, object demanderInstance)
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

            var demanderType = demanderInstance.GetType();
            var interfaces = demanderType.GetInterfaces().Where(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition()
                == typeof(IDemandToUseAPipeline<>)).ToArray();

            if (interfaces.Length == 0)
                throw new NotSupportedException("Class " + demanderType + " does not implement interface IDemandToUseAPipeline<> despite having a property which is a Pipeline");

            if (interfaces.Length > 1)
                throw new MultipleMatchingImplmentationException("Class " + demanderType + " has multiple interfaces matching IDemandToUseAPipeline<>, a given class can only demand a single Pipeline of a single flow type <T>");

            var nfDemander = new LamdaMemberFinder<IDemandToUseAPipeline<object>>();
            
            _context = interfaces[0].GetMethod(nfDemander.GetMethod(x => x.GetContext())).Invoke(demanderInstance, null);
            ExplicitSource = interfaces[0].GetMethod(nfDemander.GetMethod(x => x.GetFixedSourceIfAny())).Invoke(demanderInstance, null);
            ExplicitDestination = interfaces[0].GetMethod(nfDemander.GetMethod(x => x.GetFixedDestinationIfAny())).Invoke(demanderInstance, null);
            _inputObjects = (List<object>)interfaces[0].GetMethod(nfDemander.GetMethod(x => x.GetInputObjectsForPreviewPipeline())).Invoke(demanderInstance, null);
        }

        public override object[] GetInitializationObjects(ICatalogueRepository repository)
        {
            return _inputObjects.ToArray();
        }
        
        public override IDataFlowPipelineContext GetContext()
        {
            return (IDataFlowPipelineContext) _context;
        }
        
    }
}