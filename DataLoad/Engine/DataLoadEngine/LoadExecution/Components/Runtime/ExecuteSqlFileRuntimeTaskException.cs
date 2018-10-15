using System;

namespace DataLoadEngine.LoadExecution.Components.Runtime
{
    public class ExecuteSqlFileRuntimeTaskException : Exception
    {
        public ExecuteSqlFileRuntimeTaskException(string message):base(message)
        {
            
        }
        public ExecuteSqlFileRuntimeTaskException(string message, Exception innerException): base(message, innerException)
        {
            
        }
    }
}