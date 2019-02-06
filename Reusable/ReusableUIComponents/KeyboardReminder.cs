// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace ReusableUIComponents
{
    /// <summary>
    /// Displays a visual of a keyboard shortcut to the user by rendering the supplied key combination onto bitmaps of keys.
    /// </summary>
    [TechnicalUI]
    public partial class KeyboardReminder : UserControl
    {
        private Bitmap _key;
        private Bitmap _space;

        public KeyboardReminder()
        {
            InitializeComponent();
            _key = Images.KeyboardKey;
            _space = Images.KeyboardKeySpace;
        }

        public void Setup(string label, params Keys[] keys)
        {
            int x = 0;

            for (int index = 0; index < keys.Length; index++)
            {
                Keys key = keys[index];
                Bitmap b = GetBitmap(key);
                PictureBox pb = new PictureBox();
                pb.Location = new Point(x, 0);
                pb.Image = b;
                pb.Width = b.Width;
                pb.Height = b.Height;

                Controls.Add(pb);
                x += pb.Width + 2;

                //if not last add a '+'
                if(index +1 < keys.Length)
                {
                    Label plus = new Label();
                    plus.Text = "+";
                    plus.AutoSize = true;
                    plus.Location = new Point(x, 3);
                    Controls.Add(plus);
                    x += plus.Width + 2;
                }
            }

            Label l = new Label();
            l.AutoSize = true;
            l.Text = label;
            l.Location = new Point(x,3);
            Controls.Add(l);
            x += l.Width;
            
            this.Width = x;
        }

        private Bitmap GetBitmap(Keys k)
        {
            //draw it
            var clone = (Bitmap)(k == Keys.Space?_space.Clone():_key.Clone());

            var graphics = Graphics.FromImage(clone);
            graphics.DrawString(GetTextForKey(k),Font,Brushes.Black, new Rectangle( k==Keys.Space?10:2, 2, clone.Width, clone.Height));

            return clone;
        }

        private string GetTextForKey(Keys keys)
        {
            switch (keys)
            {
                case Keys.Control :
                    return "Ctrl";
                case Keys.ControlKey:
                    return "Ctrl";
                case Keys.LControlKey:
                    return "Ctrl";

                default:
                    return keys.ToString();
            }
        }
    }
}
