// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Rdmp.Core.Icons.IconProvision;

namespace Rdmp.UI.DataLoadUIs.LoadMetadataUIs.LoadDiagram.StateDiscovery;

/// <summary>
/// Tells you what state the LoadDiagram is in.  This starts at 'Unknown' which means no database requests have been sent and the visible tables are the 'Anticipated' state of the tables
/// during a load.  Checking the state when RAW/STAGING do not exist indicates that no load is underway and that the last load was succesful (or RAW/STAGING were cleaned up after a problem
/// was resolved).  The final state is 'Load Underway/Crashed' this indicates that RAW and/or STAGING exist which means that either a data load is in progress (not nesessarily started by you)
/// or one has completed with an error and has therefore left RAW/STAGING for debugging (See LoadDiagram).
/// </summary>
public partial class LoadStateUI : UserControl
{
    private Bitmap _unknown;
    private Bitmap _noLoadUnderway;
    private Bitmap _executingOrCrashed;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public LoadState State { get; private set; }

    public LoadStateUI()
    {
        InitializeComponent();

        _unknown = CatalogueIcons.OrangeIssue.ImageToBitmap();
        _noLoadUnderway = CatalogueIcons.Tick.ImageToBitmap();
        _executingOrCrashed = CatalogueIcons.ExecuteArrow.ImageToBitmap();

        BackColor = Color.Wheat;
        SetStatus(LoadState.Unknown);
    }


    public void SetStatus(LoadState state)
    {
        State = state;
        switch (state)
        {
            case LoadState.Unknown:
                pictureBox1.Image = _unknown;
                lblStatus.Text = "State Unknown";
                break;
            case LoadState.NotStarted:
                lblStatus.Text = "No Loads Underway";
                pictureBox1.Image = _noLoadUnderway;
                break;
            case LoadState.StartedOrCrashed:
                lblStatus.Text = "Load Underway/Crashed";
                pictureBox1.Image = _executingOrCrashed;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state));
        }
    }

    public enum LoadState
    {
        Unknown,
        NotStarted,
        StartedOrCrashed
    }
}