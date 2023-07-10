// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.UI.SimpleDialogs;
using System;
using System.Threading;

namespace Rdmp.UI.TestsAndSetup;

/// <summary>
/// Global singleton for registering/changing how global application errors are handled.
/// </summary>
public class GlobalExceptionHandler
{
    public static GlobalExceptionHandler Instance {get;} = new();

    /// <summary>
    /// What to do when errors occur, changing this discards the old action and sets a new one.  Defaults to launching a non modal <see cref="ExceptionViewer"/>
    /// </summary>
    public Action<Exception> Handler {get;set;}

    public GlobalExceptionHandler()
    {
        Handler = e=>ExceptionViewer.Show(e,false);
    }
    internal void Handle(object sender, UnhandledExceptionEventArgs  args)
    {
        Handler((Exception)args.ExceptionObject);

    }
    internal void Handle(object sender, ThreadExceptionEventArgs args)
    {
        Handler(args.Exception);
    }


}