namespace RDMPAutomationService.Interfaces
{
    public interface IAutomateable
    {
        OnGoingAutomationTask GetTask();
        void RunTask(OnGoingAutomationTask task);
    }
}
