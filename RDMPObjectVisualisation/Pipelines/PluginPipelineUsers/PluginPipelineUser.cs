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
    public class PluginPipelineUser : IPipelineUser,IPipelineUseCase
    {
        private Type _flowType;
        private object _context;
        private object _source;
        private object _destination;
        private List<object> _inputObjects;

        public PluginPipelineUser(RequiredPropertyInfo demand,IArgument argument, object demanderInstance)
        {
            Getter = () =>
            {
                var p = (Pipeline) argument.GetValueAsSystemType();
                return p;
            };

            Setter = (v) =>
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

            _flowType = interfaces[0].GenericTypeArguments[0];

            var nfDemander = new LamdaMemberFinder<IDemandToUseAPipeline<object>>();
            
            _context = interfaces[0].GetMethod(nfDemander.GetMethod(x => x.GetContext())).Invoke(demanderInstance, null);
            _source = interfaces[0].GetMethod(nfDemander.GetMethod(x => x.GetFixedSourceIfAny())).Invoke(demanderInstance, null);
            _destination = interfaces[0].GetMethod(nfDemander.GetMethod(x => x.GetFixedDestinationIfAny())).Invoke(demanderInstance, null);
            _inputObjects = (List<object>)interfaces[0].GetMethod(nfDemander.GetMethod(x => x.GetInputObjectsForPreviewPipeline())).Invoke(demanderInstance, null);
            
        }

        public object[] GetInitializationObjects(ICatalogueRepository repository)
        {
            return _inputObjects.ToArray();
        }

        public IEnumerable<Pipeline> FilterCompatiblePipelines(IEnumerable<Pipeline> pipelines)
        {
            return pipelines.Where(((IDataFlowPipelineContext)_context).IsAllowable);
        }

        public IDataFlowPipelineContext GetContext()
        {
            return (IDataFlowPipelineContext) _context;
        }

        public object ExplicitSource { get { return _source; }}
        public object ExplicitDestination { get { return _destination; } }

        public PipelineGetter Getter { get; private set; }
        public PipelineSetter Setter { get; private set; }
    }
}