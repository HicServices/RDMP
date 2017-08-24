using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueManager.DashboardTabs.Construction;
using ReusableUIComponents;
using Cursor = System.Windows.Forms.Cursor;
using Cursors = System.Windows.Forms.Cursors;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace CatalogueManager.DashboardTabs
{
    public class DashboardEditModeFunctionality
    {
        private readonly DashboardLayoutUI _layoutUI;
        private bool _editMode;


        public bool EditMode
        {
            get { return _editMode; }
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
                //if the user is not holding shift down, snap to grid
                if (!(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
                {
                    _actionUnderwayOnControl.Location = new Point(
                        (int)(Math.Round(_actionUnderwayOnControl.Location.X / 5.0) * 5),
                        (int)(Math.Round(_actionUnderwayOnControl.Location.Y / 5.0) * 5));


                    _actionUnderwayOnControl.Size = new Size(
                        (int)(Math.Round(_actionUnderwayOnControl.Size.Width / 5.0) * 5),
                        (int)(Math.Round(_actionUnderwayOnControl.Size.Height / 5.0) * 5));
                }
                
                //save changes
                foreach (KeyValuePair<DashboardControl, DashboardableControlHostPanel> kvp in _layoutUI.ControlDictionary)
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

        void control_MouseDown(object sender, MouseEventArgs e)
        {
            if (_plannedAction != EditModeAction.None)
            {
                _actionUnderway = _plannedAction;
                _actionUnderwayOnControl = _plannedControl;
            }
        }

        void control_MouseLeave(object sender, EventArgs e)
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
        
        void control_MouseMove(object sender, MouseEventArgs e)
        {

            var s = (UserControl) sender;

            var currentScreenCoordinate = s.PointToScreen(e.Location);

            if (lastKnownScreenCoordinate == Point.Empty)
            {
                lastKnownScreenCoordinate = currentScreenCoordinate;
                return;
            }


            var layoutUIVisibleArea =  FormsHelper.GetVisibleArea(_layoutUI);
            

            if (_actionUnderway != EditModeAction.None)
            {
                var vector = new Vector(
                        (currentScreenCoordinate.X - lastKnownScreenCoordinate.X ),
                        (currentScreenCoordinate.Y - lastKnownScreenCoordinate.Y));

                //move the control
                if(_actionUnderway == EditModeAction.Move)
                {
                    _actionUnderwayOnControl.Location
                        = new Point(
                            Math.Max(0, Math.Min(layoutUIVisibleArea.Width - _actionUnderwayOnControl.Width, _actionUnderwayOnControl.Location.X + (int)vector.X)),
                            Math.Max(0, Math.Min(layoutUIVisibleArea.Height - _actionUnderwayOnControl.Height, _actionUnderwayOnControl.Location.Y + (int)vector.Y))
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
        private bool IsResizeLocation(UserControl sender, MouseEventArgs e)
        {
            return e.X > sender.Width - 20 && e.Y > sender.Height - 20;
        }
        
        enum EditModeAction
        {
            None=0,
            Move,
            Resize
        }
    }
}
