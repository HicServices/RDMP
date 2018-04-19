using System;

namespace Sharing.Refactoring.Exceptions
{
    /// <summary>
    /// Thrown when there is a problem performing renaming refactoring on an SQL string (e.g. SelectSQL / WhereSQL etc)
    /// </summary>
    public class RefactoringException:Exception
    {
        public RefactoringException(string msg):base(msg)
        {
            
        }
        public RefactoringException(string msg, Exception ex):base(msg,ex)
        {
            
        }
    }
}
