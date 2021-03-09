﻿// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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
