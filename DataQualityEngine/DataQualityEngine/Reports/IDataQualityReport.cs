using System.Threading;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataQualityEngine.Reports
{
    public interface IDataQualityReport: ICheckable
    {
        bool CatalogueSupportsReport(Catalogue c);
        void GenerateReport(Catalogue c, IDataLoadEventListener listener,CancellationToken cancellationToken, AutomationJob job=null);
    }
}