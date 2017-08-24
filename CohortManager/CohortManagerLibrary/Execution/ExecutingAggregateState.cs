namespace CohortManagerLibrary.Execution
{
    public enum CompilationState
    {
        NotScheduled,
        Scheduled,
        Executing,
        Finished,
        Crashed
    }
}