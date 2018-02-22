using System.Collections.Generic;
using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Job;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.LoadExecution.Components
{
    /// <summary>
    /// DataLoadComponent (DLE) that consists of running multiple subcomponents (also DataLoadComponents).  This is used for composite stages e.g. 
    /// adjustStagingAndMigrateToLive where you want to run all or none (skip) of the components and pass the collection around as a single object.
    /// </summary>
    public class CompositeDataLoadComponent : DataLoadComponent
    {
        public IList<IDataLoadComponent> Components {get;private set;}

        public CompositeDataLoadComponent(IList<IDataLoadComponent> components)
        {
            Components = components?? new List<IDataLoadComponent>();
        }

        public override ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            if (Skip(job))
                return ExitCodeType.Error;

            foreach (var component in Components)
            {
                var result = component.Run(job, cancellationToken);
                
                job.PushForDisposal(component);
                
                if (result != ExitCodeType.Success)
                    return result;
            }

            return ExitCodeType.Success;
        }
    }
}