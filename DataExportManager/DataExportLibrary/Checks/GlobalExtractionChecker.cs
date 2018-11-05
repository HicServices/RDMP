using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Pipelines;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.ExtractionTime.ExtractionPipeline;
using HIC.Logging;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.Checks
{
    /// <summary>
    /// Checks that all the globals (<see cref="SupportingDocument"/> / <see cref="SupportingSQLTable"/>) that would be fetched as part of an
    /// <see cref="ExtractionConfiguration"/> are accessible.
    /// </summary>
    public class GlobalExtractionChecker : ICheckable
    {
        private readonly ExtractionConfiguration _configuration;
        private readonly ExtractGlobalsCommand _command;
        private readonly IPipeline _alsoCheckPipeline;

        public GlobalExtractionChecker(ExtractionConfiguration configuration) : this (configuration, null, null)
        { }
        
        public GlobalExtractionChecker(ExtractionConfiguration configuration, ExtractGlobalsCommand command, IPipeline alsoCheckPipeline)
        {
            this._configuration = configuration;
            this._command = command;
            this._alsoCheckPipeline = alsoCheckPipeline;
        }

        public void Check(ICheckNotifier notifier)
        {
            foreach (SupportingSQLTable table in _configuration.GetGlobals().OfType<SupportingSQLTable>())
                new SupportingSQLTableChecker(table).Check(notifier);

            foreach (SupportingDocument document in _configuration.GetGlobals().OfType<SupportingDocument>())
                new SupportingDocumentsFetcher(document).Check(notifier);

            if (_alsoCheckPipeline != null && _command != null)
            {
                var engine = new ExtractionPipelineUseCase(_configuration.Project, _command, _alsoCheckPipeline, DataLoadInfo.Empty)
                                    .GetEngine(_alsoCheckPipeline, new FromCheckNotifierToDataLoadEventListener(notifier));
                engine.Check(notifier);
            }
        }
    }
}