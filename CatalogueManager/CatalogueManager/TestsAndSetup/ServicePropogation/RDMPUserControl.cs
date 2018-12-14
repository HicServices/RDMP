using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CatalogueLibrary.Repositories;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
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

        private ToolStrip _toolStrip;
        private AtomicCommandUIFactory atomicCommandUIFactory;
        private IActivateItems _activator;

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


        protected void SetItemActivator(IActivateItems activator)
        {
            _activator = activator;
        }

        protected void ClearToolStrip()
        {
            //Clear the tool strip 
            if (_toolStrip != null)
                _toolStrip.Items.Clear();
        }

        /// <summary>
        /// Adds the given <paramref name="cmd"/> to the menu bar at the top of the control
        /// </summary>
        /// <param name="cmd"></param>
        protected void Add(IAtomicCommand cmd, string overrideCommandName = null, Image overrideImage = null)
        {
            InitializeToolStrip();

            var button = atomicCommandUIFactory.CreateToolStripItem(cmd);
            if (!string.IsNullOrWhiteSpace(overrideCommandName))
                button.Text = overrideCommandName;

            if (overrideImage != null)
                button.Image = overrideImage;

            _toolStrip.Items.Add(button);
        }


        /// <summary>
        /// Adds a new ToolStripLabel with the supplied <paramref name="label"/> text to the menu bar at the top of the control
        /// </summary>
        /// <param name="label"></param>
        /// <param name="showIcon">True to add the text icon next to the text</param>
        protected void Add(string label, bool showIcon = true)
        {
            InitializeToolStrip();

            _toolStrip.Items.Add(new ToolStripLabel(label, showIcon ? FamFamFamIcons.text_align_left : null));
        }

        private void InitializeToolStrip()
        {
            if(_activator == null)
                throw new Exception("Control not initialized yet, call SetItemActivator before trying to add items to the ToolStrip");

            if (atomicCommandUIFactory == null)
                atomicCommandUIFactory = new AtomicCommandUIFactory(_activator);

            if (_toolStrip == null)
            {
                _toolStrip = new ToolStrip();
                _toolStrip.Location = new Point(0, 0);
                _toolStrip.TabIndex = 1;
                this.Controls.Add(this._toolStrip);
            }
        }

    }
}