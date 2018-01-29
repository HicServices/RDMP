using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatalogueLibrary;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Job;
using DataLoadEngine.LoadExecution.Components;
using DataLoadEngine.LoadExecution.Delegates;

namespace DataLoadEngine.LoadExecution
{
    /// <summary>
    /// See SingleJobExecution
    /// </summary>
    public interface IDataLoadExecution
    {
        List<DataLoadComponent> Components { get; }
        ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken executionCancellationToken);
    }

    
}