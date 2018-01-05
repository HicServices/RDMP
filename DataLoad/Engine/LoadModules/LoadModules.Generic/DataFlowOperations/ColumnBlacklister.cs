using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.DataFlowOperations
{
    /// <summary>
    /// Pipeline component designed to prevent unwanted data existing within DataTables passing through the pipeline.  The component will crash the entire pipeline
    /// if it sees columns which match the blacklist.  Use cases for this include when the user wants to prevent private identifiers being accidentally released
    /// due to system misconfiguration e.g. you might blacklist all columns containing the strings starting "Patient" on the grounds that they are likely to be
    /// identifiable (PatientName, PatientDob etc).
    /// </summary>
    [Description("Crashes the pipeline if any column matches the regex e.g. '^(mCHI)|(chi)$'")]
    public class ColumnBlacklister : IPluginDataFlowComponent<DataTable>
    {
        [DemandsInitialization("Crashes the load if any column name matches this regex")]
        public Regex CrashIfAnyColumnMatches { get; set; }

        [DemandsInitialization("Alternative to specifying a Regex pattern in CrashIfAnyColumnMatches.  Select an existing StandardRegex.  This has the advantage of centralising the concept.  See StandardRegexUI for configuring StandardRegexes")]
        public StandardRegex StandardRegex { get; set; }
        
        [DemandsInitialization("Crash message (if any) to explain why columns matching the Regex are a problem e.g. 'Patient telephone numbers should never be extracted!'")]
        public string Rationale { get; set; }

        public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener,
            GracefulCancellationToken cancellationToken)
        {

            CrashIfAnyColumnMatches = new Regex(GetPattern(), RegexOptions.IgnoreCase);

            foreach (var c in toProcess.Columns.Cast<DataColumn>().Select(c => c.ColumnName))
                if (CrashIfAnyColumnMatches.IsMatch(c))
                    if(string.IsNullOrWhiteSpace(Rationale))
                        throw new Exception("Column " + c + " matches blacklist regex");
                    else
                        throw new Exception(Rationale + Environment.NewLine + "Exception generated because Column " + c + " matches blacklist regex" );

            return toProcess;
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            
        }

        public void Abort(IDataLoadEventListener listener)
        {
            
        }

        public void Check(ICheckNotifier notifier)
        {
            try
            {
                var p = GetPattern();
                new Regex(p);
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Problem occurred getting Regex pattern for blacklist",CheckResult.Fail, e));
            }
            

        }

        private string GetPattern()
        {
            string pattern = null;

            if (CrashIfAnyColumnMatches != null)
                pattern = CrashIfAnyColumnMatches.ToString();
            else if (StandardRegex != null)
                pattern = StandardRegex.Regex;


            if (string.IsNullOrWhiteSpace(pattern))
                throw new Exception("You must specify either a pattern in CrashIfAnyColumnMatches or pick an existing StandardRegex with a pattern to match on");

            return pattern;
        }
    }
}