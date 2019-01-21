using System;
using System.Reflection;
using CatalogueLibrary.Data.Pipelines;

namespace CatalogueLibrary.DataFlowPipeline.Requirements.Exceptions
{
    /// <summary>
    /// Thrown when a component blueprint (<see cref="PipelineComponent"/>) could not be resolved into an instance because a given property on the
    /// class was not set up correctly (<see cref="PropertyInfo"/>)
    /// </summary>
    internal class PropertyDemandNotMetException : Exception
    {
        public IPipelineComponent PipelineComponent { get; private set; }
        public PropertyInfo PropertyInfo { get; private set; }

        public PropertyDemandNotMetException(string message, IPipelineComponent pipelineComponent, PropertyInfo propertyInfo)
            : base(message)
        {
            PipelineComponent = pipelineComponent;
            PropertyInfo = propertyInfo;
        }
    }
}