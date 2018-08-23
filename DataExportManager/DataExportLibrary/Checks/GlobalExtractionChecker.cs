using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode.Checks;

namespace DataExportLibrary.Checks
{
    /// <summary>
    /// Checks that all the globals (<see cref="SupportingDocument"/> / <see cref="SupportingSQLTable"/>) that would be fetched as part of an
    /// <see cref="ExtractionConfiguration"/> are accessible.
    /// </summary>
    public class GlobalExtractionChecker : ICheckable
    {
        private readonly ExtractionConfiguration _configuration;

        public GlobalExtractionChecker(ExtractionConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public void Check(ICheckNotifier notifier)
        {
            foreach (SupportingSQLTable table in _configuration.GetGlobals().OfType<SupportingSQLTable>())
                new SupportingSQLTableChecker(table).Check(notifier);

            foreach (SupportingDocument document in _configuration.GetGlobals().OfType<SupportingDocument>())
                new SupportingDocumentsFetcher(document).Check(notifier);
        }
    }
}