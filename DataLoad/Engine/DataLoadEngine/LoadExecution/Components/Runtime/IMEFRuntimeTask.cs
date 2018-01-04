using ReusableLibraryCode.Checks;

namespace DataLoadEngine.LoadExecution.Components.Runtime
{
    public interface IMEFRuntimeTask : IRuntimeTask
    {
        ICheckable MEFPluginClassInstance { get; }
    }
}