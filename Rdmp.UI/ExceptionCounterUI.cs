// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.Icons.IconProvision;
using ReusableLibraryCode.Checks;

using PopupChecksUI = Rdmp.UI.ChecksUI.PopupChecksUI;

namespace Rdmp.UI
{
    /// <summary>
    /// Small UI control for capturing and displaying Exceptions that should not be directly brought directly to the users attention
    /// but which should none the less be visible.
    /// </summary>
    [System.ComponentModel.DesignerCategory("")]
    public class ExceptionCounterUI : ToolStripButton,ICheckNotifier
    {
        private const float EmSize = 8f;

        private ToMemoryCheckNotifier _events = new ToMemoryCheckNotifier();
        
        private const float NotifyWidth = 15;

        public ExceptionCounterUI()
        {
            Image = Images.exclamation;
            Enabled = false;
            ToolTipText = "Application Errors";
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            base.OnPaint(e);
            
            int exceptionCount = Math.Min(_events.Messages.Count, 10);

            if(exceptionCount > 0)
            {
                string msg = exceptionCount == 10?"!":exceptionCount.ToString();

                var f = new Font(FontFamily.GenericMonospace, EmSize,FontStyle.Bold);

                var xStart = (Width - NotifyWidth)/2;
                var yStart = (Height - NotifyWidth) / 2;

                GraphicsPath gp = new GraphicsPath();
                gp.AddEllipse(xStart,yStart,Width,Height);

                PathGradientBrush pgb = new PathGradientBrush(gp);

                pgb.CenterPoint = new PointF(Width / 2f,Height / 2f);
                pgb.CenterColor = Color.FromArgb(255,218,188);
                pgb.SurroundColors = new Color[] { Color.FromArgb(255, 55, 0) };


                e.Graphics.FillEllipse(pgb, xStart, yStart, NotifyWidth, NotifyWidth);
                e.Graphics.DrawString(msg,f,Brushes.White,new RectangleF(xStart + 3,yStart,NotifyWidth,NotifyWidth));
                e.Graphics.DrawEllipse(new Pen(Brushes.White,2f), xStart, yStart, NotifyWidth, NotifyWidth);
            }
        }

        protected override void OnClick(EventArgs e)
        {
            if (_events.Messages.Any())
            {
                var popup = new PopupChecksUI("Exceptions", false);
                popup.Check(new ReplayCheckable(_events));

                popup.FormClosed += (s, ea) =>
                {
                    _events = new ToMemoryCheckNotifier();
                    Enabled = false;
                    Invalidate();
                };
            }
        }
        
        public bool OnCheckPerformed(CheckEventArgs args)
        {
            //handle cross thread invocations
            var p = GetCurrentParent();

            if(p!= null && p.InvokeRequired)
            {
                p.BeginInvoke(new MethodInvoker(()=>{OnCheckPerformed(args);}));
                return false;
            }

            _events.OnCheckPerformed(args);

            try
            {
                Enabled = true;
                Invalidate();
            }
            catch (Exception)
            {
                //thrown if cross thread
            }
            return false;

        }
    }
}
