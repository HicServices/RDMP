// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data.Dashboarding;


namespace Rdmp.UI.DashboardTabs;

public class DashboardEditModeFunctionality
{
    private readonly DashboardLayoutUI _layoutUI;
    private bool _editMode;


    public bool EditMode
    {
        get => _editMode;
        set
        {
            _editMode = value; 
            EnableChanged();
        }
    }

    public DashboardEditModeFunctionality(DashboardLayoutUI layoutUI)
    {
        _layoutUI = layoutUI;
    }

    private void EnableChanged()
    {
        if (_editMode)
            foreach (var kvp in _layoutUI.ControlDictionary)
                SubscribeControl(kvp.Value);
        else
            foreach (var kvp in _layoutUI.ControlDictionary)
                UnsubscribeControl(kvp.Value);
    }

    private EditModeAction _plannedAction = EditModeAction.None;
    private UserControl _plannedControl = null;
        
    private EditModeAction _actionUnderway;
    private UserControl _actionUnderwayOnControl;

    private Point lastKnownScreenCoordinate = Point.Empty;
    private const int MinimumControlSize = 50;

    private void SubscribeControl(DashboardableControlHostPanel control)
    {
        control.MouseMove += control_MouseMove;
        control.MouseLeave += control_MouseLeave;
        control.MouseDown += control_MouseDown;
        control.MouseUp += control_MouseUp;

        control.NotifyEditModeChange(true);
    }

    private void UnsubscribeControl(DashboardableControlHostPanel control)
    {
        control.MouseMove -= control_MouseMove;
        control.MouseLeave -= control_MouseLeave;
        control.MouseDown -= control_MouseDown;
        control.MouseUp -= control_MouseUp;

        control.NotifyEditModeChange(false);
    }

    private void control_MouseUp(object sender, MouseEventArgs e)
    {
        //if we are changing a control currently
        if(_actionUnderwayOnControl != null)
        {
                
            _actionUnderwayOnControl.Location = new Point(
                (int)(Math.Round(_actionUnderwayOnControl.Location.X / 5.0) * 5),
                (int)(Math.Round(_actionUnderwayOnControl.Location.Y / 5.0) * 5));
                
            _actionUnderwayOnControl.Size = new Size(
                (int)(Math.Round(_actionUnderwayOnControl.Size.Width / 5.0) * 5),
                (int)(Math.Round(_actionUnderwayOnControl.Size.Height / 5.0) * 5));
                
                
            //save changes
            foreach (var kvp in _layoutUI.ControlDictionary)
            {
                if (kvp.Value == _actionUnderwayOnControl)
                {
                    kvp.Key.Width = _actionUnderwayOnControl.Width;
                    kvp.Key.Height = _actionUnderwayOnControl.Height;
                    kvp.Key.X= _actionUnderwayOnControl.Location.X;
                    kvp.Key.Y= _actionUnderwayOnControl.Location.Y;
                    kvp.Key.SaveToDatabase();
                }
            }
                
        }

        _plannedAction = EditModeAction.Move;
        _plannedControl = null;

        _actionUnderway = EditModeAction.None;
        _actionUnderwayOnControl = null;
    }

    private void control_MouseDown(object sender, MouseEventArgs e)
    {
        if (_plannedAction != EditModeAction.None)
        {
            _actionUnderway = _plannedAction;
            _actionUnderwayOnControl = _plannedControl;
        }
    }

    private void control_MouseLeave(object sender, EventArgs e)
    {
        //if there is no action underway
        if(_actionUnderway == EditModeAction.None)
        {
            //clear the proposed action
            Cursor.Current = Cursors.Default;
            _plannedAction = EditModeAction.Move;
            _plannedControl = null;
        }
    }

    private void control_MouseMove(object sender, MouseEventArgs e)
    {

        var s = (UserControl) sender;

        var currentScreenCoordinate = s.PointToScreen(e.Location);

        if (lastKnownScreenCoordinate == Point.Empty)
        {
            lastKnownScreenCoordinate = currentScreenCoordinate;
            return;
        }


        var layoutUIVisibleArea =  _layoutUI.GetVisibleArea();
            

        if (_actionUnderway != EditModeAction.None)
        {
            var vector = new Point(
                currentScreenCoordinate.X - lastKnownScreenCoordinate.X,
                currentScreenCoordinate.Y - lastKnownScreenCoordinate.Y);

            //move the control
            if(_actionUnderway == EditModeAction.Move)
            {
                _actionUnderwayOnControl.Location
                    = new Point(
                        Math.Max(0, Math.Min(layoutUIVisibleArea.Width - _actionUnderwayOnControl.Width, _actionUnderwayOnControl.Location.X + vector.X)),
                        Math.Max(0, Math.Min(layoutUIVisibleArea.Height - _actionUnderwayOnControl.Height, _actionUnderwayOnControl.Location.Y + vector.Y))
                    );
                _actionUnderwayOnControl.Invalidate();
            }
            else
            if (_actionUnderway == EditModeAction.Resize)
            {

                _actionUnderwayOnControl.Size
                    = new Size(
                            
                        //Do not resize below the minimum size
                        Math.Max(MinimumControlSize,

                            //do not allow resizing beyond the right of the control it is hosted in
                            Math.Min(layoutUIVisibleArea.Width - _actionUnderwayOnControl.Location.X, 

                                //change width by the length of the vector X
                                _actionUnderwayOnControl.Width + (int)vector.X))
                            
                        ,
                               
                        //Do not resize below the minimum size
                        Math.Max(MinimumControlSize,

                            //do not allow resizing beyond the bottom of the control it is hosted in
                            Math.Min(layoutUIVisibleArea.Height - _actionUnderwayOnControl.Location.Y, 

                                _actionUnderwayOnControl.Height + (int)vector.Y))
                    );

                    

                _actionUnderwayOnControl.Invalidate();
            }
        }
            
        lastKnownScreenCoordinate = currentScreenCoordinate;

        if(IsResizeLocation((UserControl)sender,e))
        {
            Cursor.Current = Cursors.SizeNWSE;
            _plannedAction = EditModeAction.Resize;
            _plannedControl = (UserControl) sender;
        }
        else
        {

            Cursor.Current = Cursors.SizeAll;
            _plannedAction = EditModeAction.Move;
            _plannedControl = (UserControl) sender;
        }
    }
        
    /// <summary>
    /// Returns true if the cursor is within the bottom right 5 pixels of a control
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private static bool IsResizeLocation(UserControl sender, MouseEventArgs e)
    {
        return e.X > sender.Width - 20 && e.Y > sender.Height - 20;
    }

    private enum EditModeAction
    {
        None=0,
        Move,
        Resize
    }
}