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
            if (k == Keys.Space)
                return _space;

            //draw it
            var clone = (Bitmap)_key.Clone();

            var graphics = Graphics.FromImage(clone);
            graphics.DrawString(GetTextForKey(k),Font,Brushes.Black, new Rectangle(2, 2, clone.Width, clone.Height));

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
