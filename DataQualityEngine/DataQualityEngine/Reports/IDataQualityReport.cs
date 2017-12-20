using System.Threading;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataQualityEngine.Reports
{
    /// <summary>
    /// Shared interface for any DQE run implementation (currently only CatalogueConstraintReport).  Supports confirming that the report can be run on a given
    /// Catalogue and running it.
    /// </summary>
    public interface IDataQualityReport: ICheckable
    {
        bool CatalogueSupportsReport(Catalogue c);
        void GenerateReport(Catalogue c, IDataLoadEventListener listener,CancellationToken cancellationToken, AutomationJob job=null);
    }
}