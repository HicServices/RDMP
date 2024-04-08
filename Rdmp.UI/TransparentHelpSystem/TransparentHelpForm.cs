// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Rdmp.UI.TransparentHelpSystem;

/// <summary>
/// Transparent windows Form which allows a pseudo greyout to occur over all controls in a window except for the location you want the users attention focused.  This includes the
/// addition of a temporary HelpBox which describes what the user is expected to do (See HelpBox).
/// </summary>
[TechnicalUI]
[System.ComponentModel.DesignerCategory("")]
public partial class TransparentHelpForm : Form
{
    private readonly Control _host;
    private Control _highlight;

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const uint SW_SHOWNOACTIVATE = 4;
    private const uint WM_NCHITTEST = 0x0084;
    private const int HTTRANSPARENT = -1;

    private Timer timer = new();
    private SolidBrush _highlightBrush;

    public TransparentHelpForm(Control host)
    {
        _host = host;
        FormBorderStyle = FormBorderStyle.None;
        SizeGripStyle = SizeGripStyle.Hide;
        StartPosition = FormStartPosition.Manual;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        TopMost = true;

        var transparencyColor = Color.Magenta;
        Opacity = 0.5f;

        _highlightBrush = new SolidBrush(transparencyColor);
        BackColor = transparencyColor;
        TransparencyKey = transparencyColor;

        timer.Interval = 100;
        timer.Tick += (s, e) => UpdateLocation();
        timer.Start();
        DoubleBuffered = true;

        //if the host is a Form and it closes we should close too
        if (host is Form form)
            form.FormClosed += (s, e) => Close();
    }

    private void UpdateLocation()
    {
        //move ourself over the hosted control
        if (_host is Form)
        {
            Location = _host.PointToScreen(new Point(0, 0));
            Width = _host.ClientRectangle.Width;
            Height = _host.ClientRectangle.Height;
        }
        else
        {
            Location = _host.PointToScreen(_host
                .Location); //host is not a top level control but an embedded control so get the screen coordinate of the control;
            Width = _host.Width;
            Height = _host.Height;
        }


        if (_host.ContainsFocus)
            ShowWithoutActivate();
        else
            Visible = false;

        Invalidate(true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.FillRectangle(Brushes.Black, 0, 0, Width, Height);

        if (_highlight != null)
        {
            var screenLocation = _highlight.PointToScreen(new Point(0, 0));
            var clientLocation = PointToClient(screenLocation);

            e.Graphics.FillRectangle(_highlightBrush, clientLocation.X, clientLocation.Y, _highlight.Width,
                _highlight.Height);
        }

        if (_currentHelpBox != null)
        {
            var screenLocation = _currentHelpBox.PointToScreen(new Point(0, 0));
            var clientLocation = PointToClient(screenLocation);

            e.Graphics.FillRectangle(_highlightBrush, clientLocation.X, clientLocation.Y, _currentHelpBox.Width,
                _currentHelpBox.Height);
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        timer.Stop();
        timer.Dispose();

        if (_host != null && _currentHelpBox != null)
            _host.Controls.Remove(_currentHelpBox);

        base.OnFormClosed(e);
    }

    public void ShowWithoutActivate()
    {
        // Show the window without activating it (i.e. do not take focus)
        ShowWindow(Handle, (short)SW_SHOWNOACTIVATE);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == (int)WM_NCHITTEST)
            m.Result = (IntPtr)HTTRANSPARENT;
        else
            base.WndProc(ref m);
    }

    private HelpBox _currentHelpBox;

    public HelpBox ShowStage(HelpWorkflow workflow, HelpStage stage)
    {
        if (_currentHelpBox != null)
            _host.Controls.Remove(_currentHelpBox);

        _highlight = stage.HighlightControl;
        _currentHelpBox = new HelpBox(workflow, stage.HelpText, stage.OptionButtonText);

        _currentHelpBox.Location = stage.UseDefaultPosition
            ? GetGoodLocationForHelpBox(_currentHelpBox)
            : stage.HostLocationForStageBox;

        _host.Controls.Add(_currentHelpBox);
        _currentHelpBox.BringToFront();
        _host.Invalidate();
        return _currentHelpBox;
    }

    private Point GetGoodLocationForHelpBox(HelpBox currentHelpBox)
    {
        var screenCoordinates = _highlight.PointToScreen(new Point(0, 0));
        var highlightTopLeft = _host.PointToClient(screenCoordinates);

        var highlightBottomLeft = highlightTopLeft with { Y = highlightTopLeft.Y + _highlight.ClientRectangle.Height };


        //First let's try to place it like this
        /**************HOST CONTROL BOUNDS*************
         *
         *       HIGHLIGHT
         *       HIGHLIGHT
         *       MSG
         *
         *********************************************/


        /**************HOST CONTROL BOUNDS*************
         *
         *       HIGHLIGHT
         *     _ HIGHLIGHT
         *     ^  MSG<------availableSpaceHorizontally->
         *     |
         *     V availableSpaceBelowHighlight
         *********************************************/

        var availableSpaceBelowHighlight = _host.ClientRectangle.Height - highlightBottomLeft.Y;
        var availableSpaceHorizontally = _host.ClientRectangle.Width - highlightBottomLeft.X;

        //fallback
        var availableSpaceAboveHighlight = highlightTopLeft.Y;

        //there is enough space below
        if (currentHelpBox.Height < availableSpaceBelowHighlight)
        {
            if (_currentHelpBox.Width < availableSpaceHorizontally)
                return highlightBottomLeft;

            //not enough space horizontally so try to move MSG to left till there is enough space
            /**************HOST CONTROL BOUNDS***********
             *
             *       HIGHLIGHT
             *  <---|HIGHLIGHT
             *  MSG_MSG_MSGMSG_MSG_MSGMSG_MSG_MSGMSG_MSG_MSG
             *
             *********************************************/
            return highlightBottomLeft with { X = Math.Max(0, _host.ClientRectangle.Width - currentHelpBox.Width) };
        }

        if (currentHelpBox.Height < availableSpaceAboveHighlight)
            //No space below so go above it
            return _currentHelpBox.Width < availableSpaceHorizontally
                ? highlightTopLeft with { Y = highlightTopLeft.Y - currentHelpBox.Height }
                :
                //consider moving X back because message box is so wide (See diagram above)
                new Point(Math.Max(0, _host.ClientRectangle.Width - currentHelpBox.Width),
                    highlightTopLeft.Y - currentHelpBox.Height);

        var screenCoordinatesTopRight = _highlight.PointToScreen(new Point(_highlight.ClientRectangle.Width, 0));
        var highlightTopRight = _host.PointToClient(screenCoordinatesTopRight);

        var spaceToLeft = highlightTopLeft.X;
        var spaceToRight = _host.ClientRectangle.Width - highlightTopRight.X;

        //there is no space at all for this help box (so just overlap it on the bottom left of the screen space available)
        if (spaceToRight < _currentHelpBox.Width && spaceToLeft < _currentHelpBox.Width)
            return new Point(0, _host.ClientRectangle.Height - _currentHelpBox.Height);

        //there is space to the right or left so put it in whichever is greater
        return spaceToRight > spaceToLeft
            ? highlightTopRight
            : new Point(highlightTopLeft.X - _currentHelpBox.Width, 0);
    }
}