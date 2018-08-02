using System;
using System.Collections.Generic;

namespace ReusableUIComponents.TransparentHelpSystem.ProgressTracking
{
    public interface IHelpWorkflowProgressProvider
    {
        bool ShouldShowUserWorkflow(HelpWorkflow workflow);
        void Completed(HelpWorkflow helpWorkflow);
    }
}
