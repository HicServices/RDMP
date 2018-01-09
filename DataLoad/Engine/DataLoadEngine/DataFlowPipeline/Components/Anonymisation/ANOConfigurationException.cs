using System;

namespace DataLoadEngine.DataFlowPipeline.Components.Anonymisation
{
    /// <summary>
    /// Thrown when there is a problem with the configuration of an ANOTable / Identifier Dump.  This can include datatype mismatches, dump not having correct
    /// columns / backup trigger etc.
    /// </summary>
    public class ANOConfigurationException : Exception
    {
        public ANOConfigurationException(string message) : base(message)
        {
            
        }

        public ANOConfigurationException(string message, Exception innerException) : base(message, innerException)
        { 
            
        }
    }
}