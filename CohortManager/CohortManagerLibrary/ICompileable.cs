using System;
using System.Diagnostics;
using System.Threading;
using CatalogueLibrary.Data.Cohort;
using CohortManagerLibrary.Execution;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.DataAccess;

namespace CohortManagerLibrary
{
    /// <summary>
    /// A cohort identification container (AggregateContainer) or sub query (AggregateConfiguration) that is running in a CohortCompiler and will be 
    /// given the results of the execution (CohortIdentificationTaskExecution).
    /// </summary>
    public interface ICompileable:IOrderable
    {
        IMapsDirectlyToDatabaseTable Child { get; }
        int Timeout { get; set; }

        CancellationToken CancellationToken { get; set; }
        CompilationState State { set; get; }
        
        event EventHandler StateChanged;
        Exception CrashMessage { get; set; }

        int FinalRowCount { set; }
        int? CumulativeRowCount { get; set; }

        IDataAccessPoint[] GetDataAccessPoints();

        Stopwatch Stopwatch { get; set; }
        TimeSpan? ElapsedTime { get; }
        

        string GetCachedQueryUseCount();

        void SetKnownContainer(CohortAggregateContainer parent, bool isFirstInContainer);
    }
}
