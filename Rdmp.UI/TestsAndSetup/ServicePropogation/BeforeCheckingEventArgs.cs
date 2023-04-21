// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.UI.ChecksUI;


namespace Rdmp.UI.TestsAndSetup.ServicePropogation;

public class BeforeCheckingEventArgs : EventArgs
{
    public ICheckable Checkable { get; private set; }
    public ICheckNotifier CheckNotifier{ get; private set; }

    /// <summary>
    /// True to cancel the checking process
    /// </summary>
    public bool Cancel { get; set; }

    public BeforeCheckingEventArgs(RAGSmileyToolStrip checkNotifier, ICheckable checkable)
    {
        Checkable = checkable;
        CheckNotifier = checkNotifier;
    }
}