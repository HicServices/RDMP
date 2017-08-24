using System;

namespace DataLoadEngine.DataFlowPipeline.Components.Anonymisation
{
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