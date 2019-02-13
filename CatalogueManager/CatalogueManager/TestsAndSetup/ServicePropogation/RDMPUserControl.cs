// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CatalogueLibrary.Repositories;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableUIComponents;
using ReusableUIComponents.ChecksUI;

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

        private RAGSmileyToolStrip ragSmileyToolStrip;
        private ToolStripButton runChecksToolStripButton = new ToolStripButton("Run Checks", FamFamFamIcons.arrow_refresh);
        private ICheckable _checkable;

        /// <summary>
        /// All keywords added via <see cref="AddHelp"/>
        /// </summary>
        private readonly HashSet<string> _helpAdded = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

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

            runChecksToolStripButton.Click += (s,e)=>StartChecking();
        }

        protected override void OnLoad(EventArgs e)
        {
            if (VisualStudioDesignMode)
                return;
            
            if (RepositoryLocator != null)
                new ServiceLocatorPropagatorToChildControls(this).PropagateRecursively(new[] {this},VisualStudioDesignMode);

            base.OnLoad(e);
        }


        public virtual void SetItemActivator(IActivateItems activator)
        {
            _activator = activator;
            RepositoryLocator = _activator.RepositoryLocator;
        }


        /// <summary>
        /// Adds the given <paramref name="cmd"/> to the top bar at the top of the control.  This will be always
        /// visible at the top of the form
        /// </summary>
        /// <param name="cmd"></param>
        public void Add(IAtomicCommand cmd, string overrideCommandName = null, Image overrideImage = null)
        {
            var p = GetTopmostRDMPUserControl(this, null);
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
        /// <param name="item"></param>
        public void Add(ToolStripItem item)
        {
            var p = GetTopmostRDMPUserControl(this, null);
            if (p != null)
            {
                p.Add(item);
                return;
            }

            InitializeToolStrip();

            _toolStrip.Items.Add(item);
        }

        /// <summary>
        /// Adds check buttons to the tool strip and sets up <see cref="StartChecking"/> to target <paramref name="checkable"/>.
        /// </summary>
        /// <param name="checkable"></param>
        public void AddChecks(ICheckable checkable)
        {
            if (ragSmileyToolStrip == null)
                ragSmileyToolStrip = new RAGSmileyToolStrip(this);

            Add(ragSmileyToolStrip);
            Add(runChecksToolStripButton);
            _checkable = checkable;
        }

        /// <summary>
        /// Adds check buttons to the tool strip and sets up <see cref="StartChecking"/> to target the return value of <paramref name="checkableFunc"/>.  If the method throws the
        /// Exception will be exposed in the checking system. 
        /// 
        /// <para>Only use this method if there is a reasonable chance the <paramref name="checkableFunc"/> will crash otherwise use the normal overload</para>
        /// </summary>
        /// <param name="checkableFunc"></param>
        protected void AddChecks(Func<ICheckable> checkableFunc)
        {
            if (ragSmileyToolStrip == null)
                ragSmileyToolStrip = new RAGSmileyToolStrip(this);

            Add(ragSmileyToolStrip);
            Add(runChecksToolStripButton);

            try
            {
                _checkable = checkableFunc();
            }
            catch (Exception e)
            {
                //covers the case where you have previously passed checks then fail but _checks points to the old passing one
                _checkable = null;
                ragSmileyToolStrip.Fatal(e);
            }
        }

        /// <summary>
        /// Runs checks on the last variable passed in <see cref="AddChecks"/>.  Do not call this method unless you have first
        /// called <see cref="AddChecks"/>.
        /// </summary>
        protected void StartChecking()
        {
            if(_checkable == null)
                return;

            OnBeforeChecking();

            ragSmileyToolStrip.StartChecking(_checkable);
        }

        /// <summary>
        /// Called immediately before checking the object set up by the last call to <see cref="AddChecks"/>
        /// </summary>
        protected virtual void OnBeforeChecking()
        {
            
        }

        /// <summary>
        /// Reports the supplied exception in the RAG checks smiley on the top toolbar.  This will result in rag checks becomming 
        /// visible if it was not visible before.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="exception"></param>
        protected void Fatal(string s, Exception exception)
        {
            if (ragSmileyToolStrip == null)
                ragSmileyToolStrip = new RAGSmileyToolStrip(this);

            if (ragSmileyToolStrip.GetCurrentParent() == null)
                Add(ragSmileyToolStrip);

            ragSmileyToolStrip.OnCheckPerformed(new CheckEventArgs(s,CheckResult.Fail,exception));
        }

        /// <summary>
        /// Adds a <see cref="HelpIcon"/> on the right of the control with documentation for the listed property
        /// </summary>
        /// <param name="c">The control you want the help to appear beside</param>
        /// <param name="propertyName">The xml-doc property you want e.g. "ICatalogue.Name"</param>
        /// <param name="anchor">Explicit anchor style to apply to help icon.  If you pass None (default) then anchor will
        ///  be chosen based on the control <paramref name="c"/></param>
        protected void AddHelp(Control c, string propertyName,string title = null, AnchorStyles anchor = AnchorStyles.None)
        {
            if(_activator == null)
                throw new Exception("Control not initialized yet, call SetItemActivator before trying to add items to the ToolStrip");

            if(c.Parent == null)
                throw new NotSupportedException("Control is not in a container.  HelpIcon cannot be added to top level controls");

            _helpAdded.Add(propertyName);

            title = title?? propertyName;
            string body = _activator.CommentStore.GetDocumentationIfExists(propertyName,false,true);

            if(body == null)
                return;
            
            var help = new HelpIcon();
            help.SetHelpText(title,body);
            
            help.Location = new Point(c.Right+3,c.Top);

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


        protected void ClearToolStrip()
        {
            var p = GetTopmostRDMPUserControl(this, null);
            
            //never clear the toolbar if you are not the topmost RDMPUserControl otherwise the parent will setup some menu items and you will clear it
            if (p != null)
                return;

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
        public void AddToMenu(IAtomicCommand cmd, string overrideCommandName = null, Image overrideImage = null)
        {
            var p = GetTopmostRDMPUserControl(this, null);
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
        public void AddToMenu(ToolStripItem menuItem)
        {
            var p = GetTopmostRDMPUserControl(this,null);
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
        /// Returns the topmost control which implements <see cref="RDMPUserControl"/>
        /// </summary>
        /// <param name="c"></param>
        /// <param name="found">pass null when calling this</param>
        /// <returns></returns>
        private RDMPUserControl GetTopmostRDMPUserControl(Control c, RDMPUserControl found)
        {
            if (c.Parent == null)
                return found;

            return GetTopmostRDMPUserControl(c.Parent, c.Parent as RDMPUserControl ?? found);
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