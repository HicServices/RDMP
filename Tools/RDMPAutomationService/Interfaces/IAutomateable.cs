namespace RDMPAutomationService.Interfaces
{
    /// <summary>
    /// A discrete job that is to be performed within the Automation framework of RDMP.  For example running a Data Quality Engine run on a single dataset.  The 
    /// constructor arguments should setup the IAutomateable ready to go (including accepting an AutomationServiceSlot), RunTask should contain the implementation
    /// logic for completing the task when it is run.
    /// 
    /// <para>This class is designed to be created by an IAutomationSource when it identifies a descrete work package that can be executed.</para>
    /// </summary>
    public interface IAutomateable
    {
        /// <summary>
        /// Packages your RunTask method with appropriate information (which AutomationJob represents it in the AutomationServiceSlot, what AutomationJobType it is
        /// etc.). 
        /// </summary>
        /// <returns></returns>
        OnGoingAutomationTask GetTask();

        void RunTask(OnGoingAutomationTask task);
    }
}
