using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using RDMPAutomationService.Options.Abstracts;

namespace RDMPAutomationService.Options
{
    /// <summary>
    /// Command line options for the Cohort Creation Pipelines
    /// </summary>
    [Verb("cohort", HelpText = "Runs the Cohort Creation")]
    public class CohortCreationOptions : RDMPCommandLineOptions
    {
        // Used for refreshes:
        [Option('e', "ExtractionConfiguration", HelpText = "The ExtractionConfiguration ID to extract", Required = true)]
        public int ExtractionConfiguration { get; set; }

        // Other Options:
        /*
         * External Cohort Table
         * 
         * Project (only existing?)
         * 
         * New Cohort / Revision?
         * 
         * Name (or existing cohort name/id?)
         * 
         * Pipeline
         * 
         * Description
         * 
         */

    }
}
