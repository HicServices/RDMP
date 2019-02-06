// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ReusableUIComponents.Dialogs;

namespace ReusableUIComponents
{
    public class FormsHelper
    {
        public static IEnumerable<Control> GetAllChildControlsRecursively(Control control)
        {
            var controls = control.Controls.Cast<Control>().ToArray();

            return controls.SelectMany(GetAllChildControlsRecursively).Concat(controls);
        }


        public static Size GetPreferredSizeOfTextControl(Control c)
        {
            Graphics graphics = c.CreateGraphics();
            SizeF measureString = graphics.MeasureString(c.Text, c.Font);

            int minimumWidth = 400;
            int minimumHeight = 150;

            Rectangle maxSize = Screen.GetBounds(c);
            return new Size(
            (int)Math.Min(maxSize.Width, Math.Max(measureString.Width + 50,minimumWidth)),
            (int)Math.Min(maxSize.Height,Math.Max(measureString.Height + 100,minimumHeight)));

        }

        public static Rectangle GetVisibleArea(Control c)
        {
            Control originalControl = c;
            var rect = c.RectangleToScreen(c.ClientRectangle);
            while (c != null)
            {
                rect = Rectangle.Intersect(rect, c.RectangleToScreen(c.ClientRectangle));
                c = c.Parent;
            }
            rect = originalControl.RectangleToClient(rect);
            return rect;
        }

        public static List<Exception> SetupAutoCompleteForTypes(ComboBox comboBox, IEnumerable<Type> typeList)
        {
            comboBox.Items.Clear();

            List<Exception> problemsDuringLoad = new List<Exception>();

            try
            {
                // Find all types anywhere in the entire program which can be assigned to a type the user wants (and aren't abstract or interfaces)
                var classes = typeList.ToArray();

                foreach (string className in classes.Select(t => t.FullName))
                    comboBox.Items.Add(className);
            }
            catch (Exception e)
            {
                ExceptionViewer.Show(e);

                //see if it is a missing assembly
                FileNotFoundException fileNotFoundException = e as FileNotFoundException;

                if (fileNotFoundException != null && !String.IsNullOrWhiteSpace(fileNotFoundException.FusionLog))
                    MessageBox.Show("FusionLog was:" + fileNotFoundException.FusionLog);


                return problemsDuringLoad;
            }

            comboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
            return problemsDuringLoad;
        }

        public static void DoActionAndRedIfThrows(TextBox tb, Action action)
        {
            tb.ForeColor = Color.Black;
            try
            {
                action();
            }
            catch (Exception)
            {
                tb.ForeColor = Color.Red;
            }
        }
    }
}
