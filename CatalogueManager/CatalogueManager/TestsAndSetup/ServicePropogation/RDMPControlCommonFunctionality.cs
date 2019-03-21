using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;
using ReusableUIComponents.ChecksUI;

namespace CatalogueManager.TestsAndSetup.ServicePropogation
{
    public class RDMPControlCommonFunctionality
    {
        /// <summary>
        /// This is the strip of buttons and labels for all controls commonly used for interacting with the content of the tab.  The 
        /// bar should start with <see cref="_menuDropDown"/>.
        /// </summary>
        public ToolStrip ToolStrip { get; private set; }
        
        /// <summary>
        /// Occurs before checking the <see cref="ICheckable"/> (see  <see cref="StartChecking"/>
        /// </summary>
        public event EventHandler BeforeChecking;

        /// <summary>
        /// Event occurs when the <see cref="ToolStrip"/> is added to the hosting Control
        /// </summary>
        public event EventHandler ToolStripAddedToHost;

        private readonly IRDMPControl _hostControl;

        /// <summary>
        /// This is the button with 3 horizontal lines which exposes all menu options which are seldom pressed or navigate you somewhere
        /// </summary>
        private ToolStripMenuItem _menuDropDown;

        private AtomicCommandUIFactory atomicCommandUIFactory;
        
        private RAGSmileyToolStrip _ragSmileyToolStrip;
        private readonly ToolStripButton _runChecksToolStripButton = new ToolStripButton("Run Checks", FamFamFamIcons.arrow_refresh);
        private ICheckable _checkable;
        private IActivateItems _activator;
        
        
        /// <summary>
        /// All keywords added via <see cref="AddHelp"/>
        /// </summary>
        private readonly HashSet<string> _helpAdded = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

        public RDMPControlCommonFunctionality(IRDMPControl hostControl)
        {
            _hostControl = hostControl;
            ToolStrip = new ToolStrip();
            
            ToolStrip.Location = new Point(0, 0);
            ToolStrip.TabIndex = 1;

            //Add the three lines dropdown for seldom used options (See AddToMenu). This starts disabled.
            _menuDropDown = new ToolStripMenuItem();
            _menuDropDown.Image = CatalogueIcons.Menu;
            _menuDropDown.Enabled = false;
            ToolStrip.Items.Add(_menuDropDown);

            _runChecksToolStripButton.Click += (s, e) => StartChecking();
        }

        public void SetItemActivator(IActivateItems activator)
        {
            _activator = activator;
            atomicCommandUIFactory = new AtomicCommandUIFactory(_activator);
            _activator.Theme.ApplyTo(ToolStrip);
        }


        /// <summary>
        /// Adds the given <paramref name="cmd"/> to the top bar at the top of the control.  This will be always
        /// visible at the top of the form
        /// </summary>
        /// <param name="cmd"></param>
        public void Add(IAtomicCommand cmd, string overrideCommandName = null, Image overrideImage = null)
        {
            var p = _hostControl.GetTopmostRDMPUserControl();
            if (p != _hostControl)
            {
                p.CommonFunctionality.Add(cmd, overrideCommandName, overrideImage);
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
        /// <param name="item"></param>
        public void Add(ToolStripItem item)
        {
            var p = _hostControl.GetTopmostRDMPUserControl();
            if (p != _hostControl)
            {
                p.CommonFunctionality.Add(item);
                return;
            }

            InitializeToolStrip();

            ToolStrip.Items.Add(item);
        }

        /// <summary>
        /// Adds check buttons to the tool strip and sets up <see cref="StartChecking"/> to target <paramref name="checkable"/>.
        /// </summary>
        /// <param name="checkable"></param>
        public void AddChecks(ICheckable checkable)
        {
            if (_ragSmileyToolStrip == null)
                _ragSmileyToolStrip = new RAGSmileyToolStrip((Control) _hostControl);

            Add(_ragSmileyToolStrip);
            Add(_runChecksToolStripButton);
            _checkable = checkable;
        }

        /// <summary>
        /// Adds check buttons to the tool strip and sets up <see cref="StartChecking"/> to target the return value of <paramref name="checkableFunc"/>.  If the method throws the
        /// Exception will be exposed in the checking system. 
        /// 
        /// <para>Only use this method if there is a reasonable chance the <paramref name="checkableFunc"/> will crash otherwise use the normal overload</para>
        /// </summary>
        /// <param name="checkableFunc"></param>
        public void AddChecks(Func<ICheckable> checkableFunc)
        {
            if (_ragSmileyToolStrip == null)
                _ragSmileyToolStrip = new RAGSmileyToolStrip((Control)_hostControl);

            Add(_ragSmileyToolStrip);
            Add(_runChecksToolStripButton);

            try
            {
                _checkable = checkableFunc();
            }
            catch (Exception e)
            {
                //covers the case where you have previously passed checks then fail but _checks points to the old passing one
                _checkable = null;
                _ragSmileyToolStrip.Fatal(e);
            }
        }

        /// <summary>
        /// Runs checks on the last variable passed in <see cref="AddChecks"/>.  Do not call this method unless you have first
        /// called <see cref="AddChecks"/>.
        /// </summary>
        public void StartChecking()
        {
            if (_checkable == null)
                return;

            if(BeforeChecking != null)
                BeforeChecking(this,new EventArgs());

            _ragSmileyToolStrip.StartChecking(_checkable);
        }
        /// <summary>
        /// Reports the supplied exception in the RAG checks smiley on the top toolbar.  This will result in rag checks becomming 
        /// visible if it was not visible before.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="exception"></param>
        public void Fatal(string s, Exception exception)
        {
            if (_ragSmileyToolStrip == null)
                _ragSmileyToolStrip = new RAGSmileyToolStrip((Control) _hostControl);

            if (_ragSmileyToolStrip.GetCurrentParent() == null)
                Add(_ragSmileyToolStrip);

            _ragSmileyToolStrip.OnCheckPerformed(new CheckEventArgs(s, CheckResult.Fail, exception));
        }

        /// <summary>
        /// Adds a <see cref="HelpIcon"/> on the right of the control with documentation for the listed property
        /// </summary>
        /// <param name="c">The control you want the help to appear beside</param>
        /// <param name="propertyName">The xml-doc property you want e.g. "ICatalogue.Name"</param>
        /// <param name="anchor">Explicit anchor style to apply to help icon.  If you pass None (default) then anchor will
        ///  be chosen based on the control <paramref name="c"/></param>
        public void AddHelp(Control c, string propertyName, string title = null, AnchorStyles anchor = AnchorStyles.None)
        {
            if (_activator == null)
                throw new Exception("Control not initialized yet, call SetItemActivator before trying to add items to the ToolStrip");

            if (c.Parent == null)
                throw new NotSupportedException("Control is not in a container.  HelpIcon cannot be added to top level controls");

            _helpAdded.Add(propertyName);

            title = title ?? propertyName;
            string body = _activator.CommentStore.GetDocumentationIfExists(propertyName, false, true);

            if (body == null)
                return;

            var help = new HelpIcon();
            help.SetHelpText(title, body);

            help.Location = new Point(c.Right + 3, c.Top);

            if (anchor == AnchorStyles.None)
            {
                if (c.Anchor.HasFlag(AnchorStyles.Right) && c.Anchor.HasFlag(AnchorStyles.Top))
                    help.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                else if (c.Anchor.HasFlag(AnchorStyles.Right) && c.Anchor.HasFlag(AnchorStyles.Bottom))
                    help.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            }
            else
                help.Anchor = anchor;

            c.Parent.Controls.Add(help);
        }


        public void ClearToolStrip()
        {
            var p = _hostControl.GetTopmostRDMPUserControl();

            //never clear the toolbar if you are not the topmost RDMPUserControl otherwise the parent will setup some menu items and you will clear it
            if (p != _hostControl)
                return;

            if (ToolStrip != null)
                ToolStrip.Items.Clear();

            if (_menuDropDown != null)
            {
                _menuDropDown.DropDownItems.Clear();
                _menuDropDown.Enabled = false;

                if (ToolStrip != null)
                    ToolStrip.Items.Add(_menuDropDown);
            }
        }

        /// <summary>
        /// Adds the given <paramref name="cmd"/> to the drop down menu button bar at the top of the control.  This
        /// will be visible only when you click on the menu button.
        /// </summary>
        /// <param name="cmd"></param>
        public void AddToMenu(IAtomicCommand cmd, string overrideCommandName = null, Image overrideImage = null)
        {
            var p = _hostControl.GetTopmostRDMPUserControl();
            if (p != _hostControl)
            {
                p.CommonFunctionality.AddToMenu(cmd, overrideCommandName, overrideImage);
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
        public void AddToMenu(ToolStripItem menuItem)
        {
            var p = _hostControl.GetTopmostRDMPUserControl();
            if (p != _hostControl)
            {
                p.CommonFunctionality.AddToMenu(menuItem);
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
        public void Add(string label, bool showIcon = true)
        {
            Add(new ToolStripLabel(label, showIcon ? FamFamFamIcons.text_align_left : null));
        }

        /// <summary>
        /// Adds the given <paramref name="cmd"/> to the menu bar at the top of the control
        /// </summary>
        /// <param name="cmd"></param>
        protected void Add(IAtomicCommand cmd, string overrideCommandName, RDMPConcept overrideImage, OverlayKind overlayKind = OverlayKind.None)
        {
            Add(cmd, overrideCommandName, _activator.CoreIconProvider.GetImage(overrideImage, overlayKind));
        }

        protected virtual void InitializeToolStrip()
        {
            if (_activator == null)
                throw new Exception("Control not initialized yet, call SetItemActivator before trying to add items to the ToolStrip");

            ((Control)_hostControl).Controls.Add(ToolStrip);

            if(ToolStripAddedToHost != null)
                ToolStripAddedToHost(this,new EventArgs());
        }
    }
}