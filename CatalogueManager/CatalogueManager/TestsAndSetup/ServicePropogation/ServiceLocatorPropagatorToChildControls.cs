using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Repositories;
using RDMPStartup;
using ReusableUIComponents;

namespace CatalogueManager.TestsAndSetup.ServicePropogation
{
    public class ServiceLocatorPropagatorToChildControls
    {
        private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
        public ServiceLocatorPropagatorToChildControls(IRepositoryUser root)
        {
            _repositoryLocator = root.RepositoryLocator;
        }
        public ServiceLocatorPropagatorToChildControls(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            _repositoryLocator = repositoryLocator;
        }

        public void PropagateRecursively(IEnumerable<Control> controls ,bool visualStudioDesignMode)
        {
            foreach (Control control in controls)
            {
                var propagateLocator = control as RDMPUserControl;
                var propagateVisualStudioDesignerMode = control as IKnowIfImHostedByVisualStudio;

                if (propagateLocator != null)
                    propagateLocator.RepositoryLocator = _repositoryLocator;

                if(propagateVisualStudioDesignerMode != null)
                    propagateVisualStudioDesignerMode.SetVisualStudioDesignMode(visualStudioDesignMode);
                
                PropagateRecursively(control.Controls.Cast<Control>(),visualStudioDesignMode);

            }

            
        }
    }
}