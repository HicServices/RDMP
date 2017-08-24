using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReusableUIComponents
{
    public class DoTransparencyProperly
    {
        public static void ThisHoversOver(Control controlThatHovers, Control whatItHoversOver)
        {
            controlThatHovers.BackColor = Color.Transparent;

            var pos = controlThatHovers.Parent.PointToScreen(controlThatHovers.Location);
            controlThatHovers.Parent = whatItHoversOver;
            controlThatHovers.Location = whatItHoversOver.PointToClient(pos);
        }
    }
}
