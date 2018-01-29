using System;
using System.Diagnostics.Contracts;
using CatalogueLibrary;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.LoadExecution;

namespace DataLoadEngine.LoadProcess
{
    /// <summary>
    /// See DataLoadProcess
    /// </summary>
    public interface IDataLoadProcess 
    {
        ExitCodeType Run(GracefulCancellationToken loadCancellationToken);
        IDataLoadExecution LoadExecution { get; }
    }

}