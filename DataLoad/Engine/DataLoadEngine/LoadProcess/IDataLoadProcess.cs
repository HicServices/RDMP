using System;
using System.Diagnostics.Contracts;
using CatalogueLibrary;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.LoadExecution;

namespace DataLoadEngine.LoadProcess
{
    public interface IDataLoadProcess 
    {
        ExitCodeType Run(GracefulCancellationToken loadCancellationToken);
        IDataLoadExecution LoadExecution { get; }
    }

}