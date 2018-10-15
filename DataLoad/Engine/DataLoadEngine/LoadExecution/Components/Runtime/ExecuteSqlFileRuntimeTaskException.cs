using System;

namespace DataLoadEngine.LoadExecution.Components.Runtime
{
    /// <summary>
    /// Exception thrown when there is an error in assembling/running an <see cref="ExecuteSqlFileRuntimeTask"/>.  This does not include SqlExceptions thrown as a result
    /// of running the final script on the database.
    /// </summary>
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