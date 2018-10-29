using System;

namespace CatalogueLibrary.DataFlowPipeline.Requirements.Exceptions
{
    /// <summary>
    /// Occurs when multiple input objects in a data flow pipeline match a given IPipelineRequirement e.g. IPipelineRequirement of object (never do this!) will match every single input object which will throw this exception
    /// </summary>
    public class MultipleMatchingImplmentationException : Exception
    {
        /// <summary>
        /// Creates a new Exception with the provided message
        /// </summary>
        /// <param name="s"></param>
        public MultipleMatchingImplmentationException(string s) : base (s)
        {
        }
    }
}