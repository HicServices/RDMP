using System;

namespace CatalogueLibrary.DataFlowPipeline.Requirements.Exceptions
{
    /// <summary>
    /// Occurs when there are 2 or more interfaces which both are compatible with a single input object during initialization of a pipeline component
    /// </summary>
    public class OverlappingImplementationsException : Exception
    {
        public OverlappingImplementationsException(string s) : base (s)
        {
        }
    }
}