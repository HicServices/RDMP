// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleControls;


namespace Rdmp.UI.TestsAndSetup.ServicePropogation;

/// <summary>
/// TECHNICAL: Base class for all Forms in all RDMP applications which require to know where the DataCatalogue Repository and/or DataExportManager Repository databases are stored.
/// IMPORTANT: You MUST set RepositoryLocator = X after calling the constructor on any RDMPForm before showing it (see RDMPFormInitializationTests) this will ensure that OnLoad is
/// able to propagate the locator to all child controls (RDMPUserControl).
/// </summary>
[TechnicalUI]
public class RDMPForm : Form, IRDMPControl
{
    /// <summary>
    /// Whether escape keystrokes should trigger form closing (defaults to true).
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool CloseOnEscape { get; set; }

    protected readonly bool VisualStudioDesignMode;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IActivateItems Activator { get; private set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public RDMPControlCommonFunctionality CommonFunctionality { get; private set; }

    /// <summary>
    /// Constructs the form without initializing the activator.  If you use this method you must call SetItemActivator manually later
    /// </summary>
    public RDMPForm()
    {
        KeyPreview = true;
        CloseOnEscape = true;
        VisualStudioDesignMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        KeyDown += RDMPForm_KeyDown;
        CommonFunctionality = new RDMPControlCommonFunctionality(this);
    }

    /// <summary>
    /// Constructs the form and initializes the activator
    /// </summary>
    /// <param name="activator"></param>
    public RDMPForm(IActivateItems activator) : this()
    {
        SetItemActivator(activator);
    }

    public void SetItemActivator(IActivateItems activator)
    {
        Activator = activator;
    }

    private void RDMPForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (((e.KeyCode == Keys.W && e.Control) || e.KeyCode == Keys.Escape) && CloseOnEscape)
            Close();

        if (e.KeyCode == Keys.S && e.Control)
            if (this is ISaveableUI saveable)
                saveable.GetObjectSaverButton().Save();
    }

    /// <summary>
    /// Returns this since RDMPForm is a Form and therefore a top level control
    /// </summary>
    /// <returns></returns>
    public IRDMPControl GetTopmostRDMPUserControl() => this;

    public event EventHandler<bool> UnSavedChanges;

    public void SetUnSavedChanges(bool b)
    {
        UnSavedChanges?.Invoke(this, b);
    }
}