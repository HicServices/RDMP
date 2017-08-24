using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using CatalogueLibrary.Repositories;
using CatalogueManager.SimpleDialogs.Reports;
using RDMPStartup;
using ReusableUIComponents;

namespace CatalogueManager.TestsAndSetup.ServicePropogation
{
    
    /// <summary>
    /// TECHNICAL: Base class for all UserControls in all RDMP applications which require to know where the DataCatalogue Repository and/or DataExportManager Repository databases are stored.
    /// The class handles propagation of the RepositoryLocator to all Child Controls at OnLoad time.  IMPORTANT: Do not use RepositoryLocator until OnLoad or later (i.e. don't use it
    /// in the constructor of your class).  Also make sure your RDMPUserControl is hosted on an RDMPForm.
    /// </summary>
    [TechnicalUI]
    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<RDMPUserControl, UserControl>))]
    public abstract class RDMPUserControl : UserControl, IRepositoryUser, IKnowIfImHostedByVisualStudio
    {
        private IRDMPPlatformRepositoryServiceLocator _repositoryLocator;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator
        {
            get { return _repositoryLocator; }
            set
            {
                //no change so don't bother firing events or even changing it
                if(value == _repositoryLocator)
                    return;

                _repositoryLocator = value;
                if (value != null)
                    OnRepositoryLocatorAvailable();
            }
        }

        protected virtual void OnRepositoryLocatorAvailable()
        {
            if (RepositoryLocator != null)
                new ServiceLocatorPropagatorToChildControls(this).PropagateRecursively(new[] { this }, VisualStudioDesignMode);
        }

        public bool VisualStudioDesignMode { get;protected set; }
        public void SetVisualStudioDesignMode(bool visualStudioDesignMode)
        {
            VisualStudioDesignMode = visualStudioDesignMode;
        }

        //constructor
        public RDMPUserControl()
        {
            VisualStudioDesignMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);
        }
        
        protected override void OnLoad(EventArgs e)
        {
            if (VisualStudioDesignMode)
                return;
            
            if (RepositoryLocator != null)
                new ServiceLocatorPropagatorToChildControls(this).PropagateRecursively(new[] {this},VisualStudioDesignMode);

            base.OnLoad(e);
        }
    }
}