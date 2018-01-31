using System;

namespace RDMPAutomationService.Pipeline
{
    internal class PipelineRunStatus
    {
        public int NumErrors { get; set; }
        public DateTime LastError { get; set; }
    }
}