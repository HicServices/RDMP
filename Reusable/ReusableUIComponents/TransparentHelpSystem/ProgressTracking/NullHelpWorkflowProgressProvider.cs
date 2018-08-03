namespace ReusableUIComponents.TransparentHelpSystem.ProgressTracking
{
    public class NullHelpWorkflowProgressProvider : IHelpWorkflowProgressProvider
    {
        public bool ShouldShowUserWorkflow(HelpWorkflow workflow)
        {
            return true;
        }

        public void Completed(HelpWorkflow helpWorkflow)
        {
        }
    }
}