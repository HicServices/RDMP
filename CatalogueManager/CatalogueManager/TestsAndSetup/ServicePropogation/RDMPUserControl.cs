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

        /// <summary>
        /// This is the strip of buttons and labels for all controls commonly used for interacting with the content of the tab.  The 
        /// bar should start with <see cref="_menuDropDown"/>.
        /// </summary>
        private ToolStrip _toolStrip;

        /// <summary>
        /// This is the button with 3 horizontal lines which exposes all menu options which are seldom pressed or navigate you somewhere
        /// </summary>
        private ToolStripMenuItem _menuDropDown;

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
            RepositoryLocator = _activator.RepositoryLocator;
        }


        /// <summary>
        /// Adds the given <paramref name="cmd"/> to the top bar at the top of the control.  This will be always
        /// visible at the top of the form
        /// </summary>
        /// <param name="cmd"></param>
        protected void Add(IAtomicCommand cmd, string overrideCommandName = null, Image overrideImage = null)
        {
            var p = Parent as RDMPUserControl;
            if (p != null)
            {
                p.Add(cmd,overrideCommandName,overrideImage);
                return;
            }

            InitializeToolStrip();

            var button = atomicCommandUIFactory.CreateToolStripItem(cmd);
            if (!string.IsNullOrWhiteSpace(overrideCommandName))
                button.Text = overrideCommandName;

            if (overrideImage != null)
                button.Image = overrideImage;

            Add(button);
        }

        /// <summary>
        /// Adds the given <paramref name="item"/> to the top bar at the top of the control.  This will be always
        /// visible at the top of the form
        /// </summary>
        /// <param name="cmd"></param>
        protected void Add(ToolStripItem item)
        {
            var p = Parent as RDMPUserControl;
            if (p != null)
            {
                p.Add(item);
                return;
            }

            InitializeToolStrip();

            _toolStrip.Items.Add(item);
        }

        protected void ClearToolStrip()
        {
            var p = Parent as RDMPUserControl;
            if (p != null)
            {
                p.ClearToolStrip();
                return;
            }

            if (_toolStrip != null)
                _toolStrip.Items.Clear();

            if (_menuDropDown != null)
            {
                _menuDropDown.DropDownItems.Clear();
                _menuDropDown.Enabled = false;

                if(_toolStrip != null)
                    _toolStrip.Items.Add(_menuDropDown);
            }
        }

        /// <summary>
        /// Adds the given <paramref name="cmd"/> to the drop down menu button bar at the top of the control.  This
        /// will be visible only when you click on the menu button.
        /// </summary>
        /// <param name="cmd"></param>
        protected void AddToMenu(IAtomicCommand cmd, string overrideCommandName = null, Image overrideImage = null)
        {
            var p = Parent as RDMPUserControl;
            if (p != null)
            {
                p.AddToMenu(cmd,overrideCommandName,overrideImage);
                return;
            }

            InitializeToolStrip();

            var menuItem = atomicCommandUIFactory.CreateMenuItem(cmd);
            if (!string.IsNullOrWhiteSpace(overrideCommandName))
                menuItem.Text = overrideCommandName;

            if (overrideImage != null)
                menuItem.Image = overrideImage;

            AddToMenu(menuItem);
        }

        /// <summary>
        /// Adds the given <paramref name="menuItem"/> to the drop down menu button bar at the top of the control.  This
        /// will be visible only when you click on the menu button.
        /// </summary>
        protected void AddToMenu(ToolStripItem menuItem)
        {
            var p = Parent as RDMPUserControl;
            if (p != null)
            {
                p.AddToMenu(menuItem);
                return;
            }

            InitializeToolStrip();

            _menuDropDown.DropDownItems.Add(menuItem);
            _menuDropDown.Enabled = true;
        }

        /// <summary>
        /// Adds a new ToolStripLabel with the supplied <paramref name="label"/> text to the menu bar at the top of the control
        /// </summary>
        /// <param name="label"></param>
        /// <param name="showIcon">True to add the text icon next to the text</param>
        protected void Add(string label, bool showIcon = true)
        {
            Add(new ToolStripLabel(label, showIcon ? FamFamFamIcons.text_align_left : null));
        }

        protected virtual void InitializeToolStrip()
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
                
                //Add the three lines dropdown for seldom used options (See AddToMenu). This starts disabled.
                _menuDropDown = new ToolStripMenuItem();
                _menuDropDown.Image = CatalogueIcons.Menu;
                _menuDropDown.Enabled = false;
                _toolStrip.Items.Add(_menuDropDown);
            }

            
            _activator.Theme.ApplyTo(_toolStrip);
        }

    }
}