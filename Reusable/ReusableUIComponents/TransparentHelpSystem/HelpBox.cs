using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReusableUIComponents.TransparentHelpSystem
{
    /// <summary>
    /// Describes why a given region is highlighted in TransparentHelpForm.  Describes what the user should do next and optionally can give the user alternate options to change the 
    /// help path being taken.
    /// </summary>
    public partial class HelpBox : UserControl
    {
        private readonly HelpWorkflow _workFlow;
        public const int FixedFormWidth = 400;

        public event Action OptionTaken;
        public HelpBox(HelpWorkflow workFlow, string text, string optionIfAny)
        {
            _workFlow = workFlow;
            InitializeComponent();

            if (workFlow == null && text == null && optionIfAny == null)
            {
                text = "Some useful text which will help guide the user to perform an activity";
                optionIfAny = "Some alternate option button the user can click";
            }
            
            lblHelp.Text = text;

            btnOption1.Text = optionIfAny;
            btnOption1.Visible = !string.IsNullOrWhiteSpace(optionIfAny);

            if (string.IsNullOrWhiteSpace(optionIfAny))
            {
                //make label fill whole form
                lblHelp.Height = panel1.Height;
            }

            btnOption1.Click += (s, e) =>
            {
                if (OptionTaken != null)
                    OptionTaken();
            };
            
            Size = GetSizeOfHelpBoxFor(text,!string.IsNullOrWhiteSpace(optionIfAny));
        }

        private Size GetSizeOfHelpBoxFor(string text, bool hasOptionButtons)
        {
            var basicSize =  TextRenderer.MeasureText(text, SystemFonts.DefaultFont, new Size(FixedFormWidth, 0),TextFormatFlags.WordBreak);

            //if theres an option button and very little text then widen the box
            if (btnOption1.Visible)
                basicSize.Width = Math.Max(basicSize.Width, btnOption1.Width + 3);

            if (hasOptionButtons)
                basicSize.Height += 23;

            //this controls padding
            basicSize.Height += 15;
            basicSize.Width += 30; //padding + width of close button

            return basicSize;
        }

        private void btnCloseHelp_Click(object sender, EventArgs e)
        {
            _workFlow.Abandon();
        }
    }
}
