using System;

namespace CatalogueManager.PluginManagement.CodeGeneration
{
    public class CodeGenerationException : Exception
    {
        public CodeGenerationException(string message):base(message)
        {
            
        }
    }
}