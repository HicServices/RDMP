using System.Threading;
using CatalogueLibrary.Data;

using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataQualityEngine.Reports
{
    public abstract class DataQualityReport : IDataQualityReport
    {
        protected Catalogue _catalogue;
        public abstract void Check(ICheckNotifier notifier);

        public virtual bool CatalogueSupportsReport(Catalogue c)
        {
            _catalogue = c;

            ToMemoryCheckNotifier checkNotifier = new ToMemoryCheckNotifier();
            Check(checkNotifier);

            return checkNotifier.GetWorst() <= CheckResult.Warning;

        }
        public abstract void GenerateReport(Catalogue c, IDataLoadEventListener listener, CancellationToken cancellationToken);
    }
}