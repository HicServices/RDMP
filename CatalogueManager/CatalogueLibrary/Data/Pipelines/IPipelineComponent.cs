using System;
using System.Collections.Generic;
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
    public interface IPipelineComponent : IArgumentHost, ISaveable, IDeleteable, IMapsDirectlyToDatabaseTable,IHasDependencies
    {
        
        string Name { get; set; }
        int Order { get; set; }
        int Pipeline_ID { get; set; }
        string Class { get; set; }
        IEnumerable<IPipelineComponentArgument> PipelineComponentArguments { get; }

        Type GetClassAsSystemType();
        string GetClassNameLastPart();
        PipelineComponent Clone(Pipeline intoTargetPipeline);
    }
}