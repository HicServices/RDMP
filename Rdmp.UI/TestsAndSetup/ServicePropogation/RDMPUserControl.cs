// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.UI.ItemActivation;


namespace Rdmp.UI.TestsAndSetup.ServicePropogation;

/// <summary>
/// TECHNICAL: Base class for all UserControls in all RDMP applications which require to know where the DataCatalogue Repository and/or DataExportManager Repository databases are stored.
/// The class handles propagation of the RepositoryLocator to all Child Controls at OnLoad time.  IMPORTANT: Do not use RepositoryLocator until OnLoad or later (i.e. don't use it
/// in the constructor of your class).  Also make sure your RDMPUserControl is hosted on an RDMPForm.
/// </summary>
[TechnicalUI]
[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<RDMPUserControl, UserControl>))]
public abstract class RDMPUserControl : UserControl, IRDMPControl
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public RDMPControlCommonFunctionality CommonFunctionality { get; private set; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IActivateItems Activator { get; private set; }

    protected readonly bool VisualStudioDesignMode;


    //constructor
    protected RDMPUserControl()
    {
        VisualStudioDesignMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        CommonFunctionality = new RDMPControlCommonFunctionality(this);
    }

    public virtual void SetItemActivator(IActivateItems activator)
    {
        Activator = activator;
        CommonFunctionality.SetItemActivator(activator);
    }


    /// <summary>
    /// Called immediately before checking the object set up by the last call to <see cref="RDMPControlCommonFunctionality.AddChecks(ICheckable)"/>
    /// </summary>
    protected virtual void OnBeforeChecking()
    {
    }

    /// <summary>
    /// Returns the topmost control which implements <see cref="RDMPUserControl"/>
    /// </summary>
    public IRDMPControl GetTopmostRDMPUserControl() => GetTopmostRDMPUserControl(this, this);

    public event EventHandler<bool> UnSavedChanges;

    public void SetUnSavedChanges(bool b)
    {
        UnSavedChanges?.Invoke(this, b);
    }

    private static IRDMPControl GetTopmostRDMPUserControl(Control c, RDMPUserControl found)
    {
        while (true)
        {
            if (c.Parent == null) return found;
            var c1 = c;
            c = c.Parent;
            found = c1.Parent as RDMPUserControl ?? found;
        }
    }
}