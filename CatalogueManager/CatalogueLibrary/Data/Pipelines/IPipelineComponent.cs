using System;
using System.Collections.Generic;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.DataLoad;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data.Pipelines
{
    /// <summary>
    /// Each Pipeline has 0 or more PipelineComponents.  A Pipeline Component is a persistence record for a user configuration of a class implementing IDataFlowComponent with
    /// zero or more [DemandsInitialization] properties.  The class the user has chosen is stored in Class property and a PipelineComponentArgument will exist for each 
    /// [DemandsInitialization] property.  
    /// 
    /// <para>PipelineComponents are turned into IDataFlowComponents when stamping out the Pipeline for use at a given time (See DataFlowPipelineEngineFactory.Create) </para>
    /// 
    /// <para>PipelineComponent is the Design time class (where it appears in Pipeline, what argument values it should be hydrated with etc) while IDataFlowComponent is 
    /// the runtime instance of the configuration. </para>
    /// </summary>
    public interface IPipelineComponent : IArgumentHost, ISaveable, IMapsDirectlyToDatabaseTable,IHasDependencies,IOrderable
    {
        /// <summary>
        /// Obsolete
        /// </summary>
        [Obsolete("Functionally identical to Class")]
        string Name { get; set; }

        /// <summary>
        /// The <see cref="Pipeline"/> in which the component is configured
        /// </summary>
        int Pipeline_ID { get; set; }

        /// <summary>
        /// The full name of the C# class Type which should be isntantiated and hydrated when using the <see cref="Pipeline"/> in which this component is configured.
        /// </summary>
        string Class { get; set; }

        /// <summary>
        /// All the arguments for hydrating <see cref="Class"/>
        /// </summary>
        IEnumerable<IPipelineComponentArgument> PipelineComponentArguments { get; }

        /// <summary>
        /// Returns <see cref="Class"/> as a resolved System.Type
        /// </summary>
        /// <returns></returns>
        Type GetClassAsSystemType();

        /// <summary>
        /// Returns the name only (without namespace) of the <see cref="Class"/>
        /// </summary>
        /// <returns></returns>
        string GetClassNameLastPart();

        /// <summary>
        /// Creates a new copy of the current <see cref="IPipelineComponent"/> into another <see cref="Pipeline"/> <see cref="intoTargetPipeline"/>
        /// <para>This is usually done as part of <see cref="IPipeline.Clone"/></para>
        /// </summary>
        /// <param name="intoTargetPipeline"></param>
        /// <returns></returns>
        PipelineComponent Clone(Pipeline intoTargetPipeline);
    }
}