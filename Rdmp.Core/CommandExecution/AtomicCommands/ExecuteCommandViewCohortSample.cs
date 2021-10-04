using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction;
using Rdmp.Core.DataViewing;
using System.IO;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    /// <summary>
    /// Fetches the private and release identifiers for a given <see cref="ExtractableCohort"/> optionally to file
    /// </summary>
    class ExecuteCommandViewCohortSample : BasicCommandExecution
    {
        public ExtractableCohort Cohort { get; }
        public int Sample { get; }
        public FileInfo ToFile { get; }

        public ExecuteCommandViewCohortSample(IBasicActivateItems activator,
            [DemandsInitialization("The cohort that you want to fetch records for")]
            ExtractableCohort cohort,
            [DemandsInitialization("Optional. The maximum number of records to retrieve")]
            int sample = 100,
            [DemandsInitialization("Optional. A file to write the records to instead of the console")]
            FileInfo toFile = null):base(activator)
        {
            Cohort = cohort;
            Sample = sample;
            ToFile = toFile;
        }
        public override void Execute()
        {
            base.Execute();

            var collection = new ViewCohortExtractionUICollection(Cohort) {
                Top = Sample
            };

            if(ToFile == null)
            {
                BasicActivator.ShowData(collection);
            }
            else
            {
                ExtractTableVerbatim.ExtractDataToFile(collection, ToFile);
            }
            
        }
    }
}
