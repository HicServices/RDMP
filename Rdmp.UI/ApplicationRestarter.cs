using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Rdmp.UI
{
    public static class ApplicationRestarter
    {

        public static void Restart()
        {
            try
            {
                Application.Restart();
            }
            catch (Exception)
            {
                RestartImpl();
            }
        }

        private static void RestartImpl()
        {
            string[] arguments = Environment.GetCommandLineArgs();

            StringBuilder sb = new StringBuilder((arguments.Length - 1) * 16);
            for (int argumentIndex = 1; argumentIndex < arguments.Length - 1; argumentIndex++)
            {
                sb.Append('"');
                sb.Append(arguments[argumentIndex]);
                sb.Append("\" ");
            }
            if (arguments.Length > 1)
            {
                sb.Append('"');
                sb.Append(arguments[arguments.Length - 1]);
                sb.Append('"');
            }
            ProcessStartInfo currentStartInfo = new ProcessStartInfo();
            currentStartInfo.FileName = Path.ChangeExtension(Application.ExecutablePath, "exe");
            if (sb.Length > 0)
            {
                currentStartInfo.Arguments = sb.ToString();
            }
            Application.Exit();
            Process.Start(currentStartInfo);
        }
    }
}
