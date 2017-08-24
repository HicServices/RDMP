using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ReusableUIComponents
{
    public class PreviewKey
    {
        private readonly Message _message;
        private readonly Keys _modifierKeys;
        //----------------------------------------------
        // Define the PeekMessage API call
        //----------------------------------------------
        
        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x101;
        const int WM_CHAR = 0x102;
        const int WM_SYSCHAR = 0x106;
        const int WM_SYSKEYDOWN = 0x104;
        const int WM_SYSKEYUP = 0x105;
        const int WM_IME_CHAR = 0x286;


        //don't remove this! it is all interopy
        private struct MSG
        {
            public IntPtr hwnd;
            public int message;
            public IntPtr wParam;
            public IntPtr lParam;
            public int time;
            public int pt_x;
            public int pt_y;
        }

        public KeyEventArgs e { get; private set; }
        public bool IsKeyDownMessage { get; private set; }
        public bool IsKeyUpMessage { get; private set; }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool PeekMessage([In, Out] ref MSG msg,
            HandleRef hwnd, int msgMin, int msgMax, int remove);

        //----------------------------------------------

        public PreviewKey(ref Message message, Keys modifierKeys)
        {
            _message = message;
            _modifierKeys = modifierKeys;




            if ((message.Msg != WM_CHAR) && (message.Msg != WM_SYSCHAR) && (message.Msg != WM_IME_CHAR))
            {
                e = new KeyEventArgs(((Keys) ((int) ((long) message.WParam))) | _modifierKeys);
                if ((message.Msg == WM_KEYDOWN) || (message.Msg == WM_SYSKEYDOWN))
                    IsKeyDownMessage = true;

                if ((message.Msg == WM_KEYUP) || (message.Msg == WM_SYSKEYUP))
                    IsKeyUpMessage = true;
            }

        }

        public void Trap(Control owner)
        {
            RemovePendingMessages(WM_CHAR, WM_CHAR,owner);
            RemovePendingMessages(WM_SYSCHAR, WM_SYSCHAR,owner);
            RemovePendingMessages(WM_IME_CHAR, WM_IME_CHAR,owner);
        }


         private void RemovePendingMessages(int msgMin, int msgMax,Control owner)
        {
            if (!owner.IsDisposed)
            {
                MSG msg = new MSG();
                IntPtr handle = owner.Handle;
                while (PeekMessage(ref msg,
                new HandleRef(this, handle), msgMin, msgMax, 1))
                {
                }
            }
        }
    }
}
