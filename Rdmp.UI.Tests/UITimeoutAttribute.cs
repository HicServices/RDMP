// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace Rdmp.UI.Tests;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
internal partial class UITimeoutAttribute : NUnitAttribute, IWrapTestMethod
{
    private readonly int _timeout;

    /// <summary>
    /// Allows <paramref name="timeout"/> for the test to complete before calling <see cref="Process.CloseMainWindow"/> and failing the test
    /// </summary>
    /// <param name="timeout">timeout in milliseconds</param>
    public UITimeoutAttribute(int timeout)
    {
        _timeout = timeout;
    }

    /// <inheritdoc/>
    public TestCommand Wrap(TestCommand command) => new TimeoutCommand(command, _timeout);

    private partial class TimeoutCommand : DelegatingTestCommand
    {
        private int _timeout;

        public TimeoutCommand(TestCommand innerCommand, int timeout) : base(innerCommand)
        {
            _timeout = timeout;
        }

        [LibraryImport("user32.dll", SetLastError = true, EntryPoint = "SendMessageW", StringMarshalling = StringMarshalling.Utf16)]
        private static partial IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [LibraryImport("user32.dll", SetLastError = true, EntryPoint = "GetDlgItemW", StringMarshalling = StringMarshalling.Utf16)]
        private static partial IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);

        private string YesNoDialog = "#32770";

        private const uint WM_CLOSE = 0x0010;
        private const uint WM_COMMAND = 0x0111;
        private int IDNO = 7;


        public override TestResult Execute(TestExecutionContext context)
        {
            TestResult result = null;
            Exception threadException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    result = innerCommand.Execute(context);
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
            });
            if (RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            try
            {
                while (thread.IsAlive && (_timeout > 0 || Debugger.IsAttached))
                    if (!thread.Join(_timeout) && !Debugger.IsAttached)
                        _timeout = 0;

                var closeAttempts = 10;

                if (_timeout <= 0)
                {
                    //Sends WM_Close which closes any form except a YES/NO dialog box because yay
                    Process.GetCurrentProcess().CloseMainWindow();

                    //if it still has a window handle then presumably needs further treatment
                    IntPtr handle;

                    while ((handle = Process.GetCurrentProcess().MainWindowHandle) != IntPtr.Zero)
                    {
                        if (closeAttempts-- <= 0)
                            throw new Exception("Failed to close all windows even after multiple attempts");

                        var sbClass = new StringBuilder(100);

                        GetClassName(handle, sbClass, 100);

                        //Is it a yes/no dialog
                        if (sbClass.ToString() == YesNoDialog && GetDlgItem(handle, IDNO) != IntPtr.Zero)
                            //with a no button
                            SendMessage(handle, WM_COMMAND, IDNO, IntPtr.Zero); //click NO!
                        else
                            SendMessage(handle, WM_CLOSE, 0, IntPtr.Zero); //click close
                    }

                    throw new Exception("UI test did not complete after timeout");
                }


                if (threadException != null)
                    throw threadException;

                return result ?? throw new Exception("UI test did not produce a result");
            }
            catch (AggregateException ae)
            {
                throw ae.InnerException;
            }
        }
    }
}