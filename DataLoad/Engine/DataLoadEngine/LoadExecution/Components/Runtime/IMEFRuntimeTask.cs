using ReusableLibraryCode.Checks;

namespace DataLoadEngine.LoadExecution.Components.Runtime
{
    /// <summary>
    /// RuntimeTask for all 'class based' ProcessTaskTypes (IAttacher, IDataProvier etc).  See RuntimeTask for full Description.
    /// </summary>
    public interface IMEFRuntimeTask : IRuntimeTask
    {
        ICheckable MEFPluginClassInstance { get; }
    }
}