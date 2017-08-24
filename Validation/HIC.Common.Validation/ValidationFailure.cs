using System;
using System.Collections.Generic;
using HIC.Common.Validation.Constraints;

namespace HIC.Common.Validation
{
    /// <summary>
    /// A custom Validation exception, thrown when user-specified validation has failed in some way.
    /// </summary>
    public class ValidationFailure
    {
        
        public ItemValidator SourceItemValidator { get; set; }
        public IConstraint SourceConstraint { get; set; }

        public string Message { get; set; }

        private List<ValidationFailure> eList;

        private ValidationFailure(string message)
        {
            Message = message;
        }

        public ValidationFailure(string message, IConstraint sender) :this(message)
        {
            SourceConstraint = sender;
        }

        public ValidationFailure(string message, ItemValidator sender): this(message)
        {
            
            SourceItemValidator = sender;
        }

        public ValidationFailure(string message, List<ValidationFailure> e): this(message)
        {
            eList = e;
        }

        public List<ValidationFailure> GetExceptionList()
        {
            return eList;
        } 
    }
}
