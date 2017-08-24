using ReusableLibraryCode.Checks;

namespace DataLoadEngine.LoadExecution.Components.Runtime
{
    public interface IMEFRuntimeTask
    {
        ICheckable MEFPluginClassInstance { get; }
    }
}