using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Repositories;
using CatalogueManager.SimpleDialogs.Reports;
using RDMPStartup;
using ReusableUIComponents;


namespace CatalogueManager.TestsAndSetup.ServicePropogation
{
    /// <summary>
    /// TECHNICAL: Base class for all Forms in all RDMP applications which require to know where the DataCatalogue Repository and/or DataExportManager Repository databases are stored.
    /// IMPORTANT: You MUST set RepositoryLocator = X after calling the constructor on any RDMPForm before showing it (see RDMPFormInitializationTests) this will ensure that OnLoad is 
    /// able to propagate the locator to all child controls (RDMPUserControl).  
    /// </summary>
    [TechnicalUI]
    public class RDMPForm : Form, IRepositoryUser, IKnowIfImHostedByVisualStudio
    {
        private IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
        public bool CloseOnEscape { get; set; } 

        public RDMPForm()
        {
            KeyPreview = true;
            CloseOnEscape = true;
            VisualStudioDesignMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);
            this.KeyPress += RDMPForm_KeyPress;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator
        {
            get { return _repositoryLocator; }
            set
            {
                _repositoryLocator = value;
                new ServiceLocatorPropagatorToChildControls(this).PropagateRecursively(new[] { this }, VisualStudioDesignMode);
                
                if (value != null)
                    OnRepositoryLocatorAvailable();
            }
        }

        protected virtual void OnRepositoryLocatorAvailable()
        {
            if (RepositoryLocator != null)
                new ServiceLocatorPropagatorToChildControls(this).PropagateRecursively(new[] { this }, VisualStudioDesignMode);
        }

        protected override void OnLoad(EventArgs e)
        {
            if (VisualStudioDesignMode)
                return;

            new ServiceLocatorPropagatorToChildControls(this).PropagateRecursively(new[] { this }, VisualStudioDesignMode);

            base.OnLoad(e);
            if(RepositoryLocator == null)
                throw new NullReferenceException("Repository Locator not set!");
        }
        
        public bool VisualStudioDesignMode { get; private set; }
        public void SetVisualStudioDesignMode(bool visualStudioDesignMode)
        {
            VisualStudioDesignMode = visualStudioDesignMode;
        }

        private void RDMPForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char) Keys.Escape && CloseOnEscape)
                Close();
        }
    }

    
}