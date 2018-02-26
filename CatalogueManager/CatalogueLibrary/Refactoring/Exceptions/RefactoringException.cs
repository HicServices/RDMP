using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueLibrary.Refactoring.Exceptions
{
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
