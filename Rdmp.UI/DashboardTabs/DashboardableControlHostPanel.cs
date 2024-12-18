// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.DashboardTabs.Construction;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.DashboardTabs;

/// <summary>
/// TECHNICAL: wrapper class for a hosted IDashboardableControl.  Is responsible for rendering the close box and the border of the control.
/// </summary>
[TechnicalUI]
public partial class DashboardableControlHostPanel : RDMPUserControl
{
    private readonly DashboardControl _databaseRecord;
    private bool _editMode;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IDashboardableControl HostedControl { get; private set; }

    private const float BorderWidth = 5;

    public DashboardableControlHostPanel(IActivateItems activator, DashboardControl databaseRecord,
        IDashboardableControl hostedControl)
    {
        SetItemActivator(activator);

        _databaseRecord = databaseRecord;
        HostedControl = hostedControl;
        InitializeComponent();

        pbDelete.Image = FamFamFamIcons.delete.ImageToBitmap();

        Margin = Padding.Empty;

        pbDelete.Visible = false;

        Controls.Add((Control)HostedControl);

        AdjustControlLocation();
    }

    private void AdjustControlLocation()
    {
        var control = (Control)HostedControl;

        //center it on us with a gap of BorderWidth
        if (_editMode)
        {
            control.Location = new Point((int)BorderWidth, (int)BorderWidth);
            control.Width = (int)(Width - BorderWidth * 2);
            control.Height = (int)(Height - BorderWidth * 2);
        }
        else
        {
            control.Location = new Point(0, 0);
            control.Width = Width;
            control.Height = Height;
        }

        //anchor to all
        control.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (_editMode)
            e.Graphics.FillRectangle(Brushes.Black, 0, 0, Width, Height);
    }

    public void NotifyEditModeChange(bool isEditModeOn)
    {
        _editMode = isEditModeOn;
        AdjustControlLocation();
        HostedControl.NotifyEditModeChange(isEditModeOn);

        pbDelete.Visible = isEditModeOn;

        if (isEditModeOn)
            pbDelete.BringToFront();

        Invalidate();
    }


    private void pbDelete_Click(object sender, EventArgs e)
    {
        if (_editMode)
        {
            var layout = _databaseRecord.ParentLayout;
            Activator.DeleteWithConfirmation(_databaseRecord);
            Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(layout));
        }
    }
}