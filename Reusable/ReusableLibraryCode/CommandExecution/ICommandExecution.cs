namespace ReusableLibraryCode.CommandExecution
{
    public interface ICommandExecution
    {
        bool IsImpossible { get; }
        string ReasonCommandImpossible { get; }
        void Execute();
        string GetCommandName();
        string GetCommandHelp();
    }
}