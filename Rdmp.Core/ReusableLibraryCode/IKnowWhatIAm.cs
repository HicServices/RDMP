// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.ReusableLibraryCode;

/// <summary>
/// Interface for classes who do not want to rely on their own xmldoc comments to tell the user what they are (see ExecuteCommandShowKeywordHelp)
/// </summary>
public interface IKnowWhatIAm
{
    /// <summary>
    /// Return an alternative description of yourself (e.g. dependent on your state) that serves to describe the general purpose of your object.
    /// This will be provided to consumers as an alternative to your class xmldoc (summary comments).
    /// </summary>
    /// <returns></returns>
    string WhatIsThis();
}