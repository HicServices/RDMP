using System;

namespace ReusableLibraryCode.CommandExecution
{
    /// <summary>
    /// Basic implementation of ICommandExecution ensures that if a command is marked IsImpossible then it cannot be run.  Call SetImpossible to render your command 
    /// un runnable with the given arguments.  You cannot make an IsImpossible command Possible again (therefore you should probably make this discision in your 
    /// constructor).  Override Execute to provide the implementation logic of your command but make sure to leave the base.Execute() call in first to ensure 
    /// IsImpossible is respected in the unlikely event that some code or user attempts to execute an impossible command.
    /// 
    /// <para>Override GetCommandHelp and GetCommandName to change the persentation layer of the command (if applicable).</para>
    /// </summary>
    public abstract class BasicCommandExecution : ICommandExecution
    {
        public bool IsImpossible { get; private set; }
        public string ReasonCommandImpossible { get; private set; }
        public string OverrideCommandName { get; set; }

        public virtual void Execute()
        {
            if(IsImpossible)
                throw new NotSupportedException("Command is marked as IsImpossible and should not be Executed.  Reason ReasonCommandImpossible is:" + ReasonCommandImpossible);
        }

        public virtual string GetCommandName()
        {
            if (!string.IsNullOrWhiteSpace(OverrideCommandName))
                return OverrideCommandName;

            var name = GetType().Name;
            var adjusted = name.Replace("ExecuteCommand", "");
            return UsefulStuff.PascalCaseStringToHumanReadable(adjusted);
        
        }

        public virtual string GetCommandHelp()
        {
            return String.Empty;
        }

        /// <summary>
        /// makes the command unrunnable because of the given reason.  This will result in greyed out menu items, crashes when executed programatically etc.
        /// </summary>
        /// <param name="reason"></param>
        protected void SetImpossible(string reason)
        {
            IsImpossible = true;
            ReasonCommandImpossible = reason;
        }

        /// <summary>
        /// Resets the IsImpossible status of the command
        /// </summary>
        protected void ResetImpossibleness()
        {
            IsImpossible = false;
            ReasonCommandImpossible = null;
        }
    }
}
