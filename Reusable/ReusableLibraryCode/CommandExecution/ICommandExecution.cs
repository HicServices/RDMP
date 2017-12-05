namespace ReusableLibraryCode.CommandExecution
{
    /// <summary>
    /// Describes an executable command that is available to the user (or assembled and executed in code).  ICommandExecution instances are allowed to be in
    /// illegal states (IsImpossible = true) and this should be respected by Execute i.e. it will throw an exception if Executed.
    /// </summary>
    public interface ICommandExecution
    {
        bool IsImpossible { get; }
        string ReasonCommandImpossible { get; }
        void Execute();
        string GetCommandName();
        string GetCommandHelp();
    }
}