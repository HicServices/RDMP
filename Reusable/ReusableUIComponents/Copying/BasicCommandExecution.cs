using System;
using System.Text.RegularExpressions;

namespace ReusableUIComponents.Copying
{
    public abstract class BasicCommandExecution : ICommandExecution
    {
        public bool IsImpossible { get; private set; }
        public string ReasonCommandImpossible { get; private set; }
        
        public virtual void Execute()
        {
            if(IsImpossible)
                throw new NotSupportedException("Command is marked as IsImpossible and should not be Executed.  Reason ReasonCommandImpossible is:" + ReasonCommandImpossible);
        }

        public virtual string GetCommandName()
        {
            var name = GetType().Name;
            var adjusted = name.Replace("ExecuteCommand", "");
            return Regex.Replace(adjusted, @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $1");
        
        }

        public virtual string GetCommandHelp()
        {
            return String.Empty;
        }

        protected void SetImpossible(string reason)
        {
            IsImpossible = true;
            ReasonCommandImpossible = reason;
        }
    }
}