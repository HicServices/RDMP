namespace ReusableLibraryCode.CommandExecution
{
    /// <summary>
    /// A command that can never be executed because of the given reason, this is a corner case where you are unable even to construct the 
    /// BasicCommandExecution you really want but need to return an ICommandExecution anyway.
    /// </summary>
    public class ImpossibleCommand : BasicCommandExecution
    {
        public ImpossibleCommand(string reasonImpossible)
        {
            SetImpossible(reasonImpossible);   
        }
    }
}