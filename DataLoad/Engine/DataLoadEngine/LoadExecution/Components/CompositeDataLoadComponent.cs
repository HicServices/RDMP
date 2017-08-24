using System.Collections.Generic;
using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Job;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.LoadExecution.Components
{
    public class CompositeDataLoadComponent : DataLoadComponent
    {
        protected readonly IList<IDataLoadComponent> _components = new List<IDataLoadComponent>();

        public CompositeDataLoadComponent(IList<IDataLoadComponent> components)
        {
            _components = components;

        }

        public override ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            if (Skip(job))
                return ExitCodeType.Error;

            foreach (var component in _components)
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