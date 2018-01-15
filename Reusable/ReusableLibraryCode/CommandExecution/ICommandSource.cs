namespace ReusableLibraryCode.CommandExecution
{
    /// <summary>
    /// Object which can be converted into an ICommand e.g. by starting a dragg operation on it.
    /// </summary>
    public interface ICommandSource
    {
        ICommand GetCommand();
    }
}