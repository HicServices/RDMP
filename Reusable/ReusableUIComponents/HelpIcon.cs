using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReusableUIComponents
{
    /// <summary>
    /// Hovering over this control displays helpful information that relates to a nearby control.
    /// </summary>
    public partial class HelpIcon : UserControl
    {
        private string _hoverText;
        private string _title;

        public HelpIcon()
        {
            InitializeComponent();
        }
        
        public void SetHelpText(string title, string hoverText)
        {
            _title = title;
            _hoverText = hoverText;
            Visible = !String.IsNullOrWhiteSpace(_hoverText);

            Regex rgx = new Regex("(.{150}\\s)");
            _hoverText = rgx.Replace(hoverText, "$1\n");
        }

        private void HelpIcon_MouseHover(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(_hoverText))
            {
                var tt = new ToolTip
                {
                    AutoPopDelay = 15000,  // Warning! MSDN states this is Int32, but anything over 32767 will fail.
                    ShowAlways = true,
                    ToolTipTitle = _title,
                    InitialDelay = 200,
                    ReshowDelay = 200,
                    UseAnimation = true
                };
                tt.SetToolTip(this, _hoverText);
            }

        }
    }

  
}
