using System;
using System.Runtime.InteropServices;

namespace ReusableUIComponents
{
    public class NativeMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetScrollPos(IntPtr hWnd, System.Windows.Forms.Orientation nBar);
    }
}
