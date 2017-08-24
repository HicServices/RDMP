using System;
using CatalogueLibrary;

namespace DataLoadEngine.LoadProcess
{
    interface IDataLoadOperation
    {
        ExitCodeType? ExitCode { get; }
        Exception Exception { get; }
    }
}
