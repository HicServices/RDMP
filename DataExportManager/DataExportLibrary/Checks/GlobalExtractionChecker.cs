using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode.Checks;

namespace DataExportLibrary.Checks
{
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
            {
                SupportingDocumentsFetcher fetcher = new SupportingDocumentsFetcher(document);
                fetcher.CheckSingle(notifier); 
            }
        }
    }
}