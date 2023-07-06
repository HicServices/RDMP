// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Dashboarding;
using System.Collections.Generic;

namespace Rdmp.UI.Collections;

/// <summary>
/// Collection of objects grouped into a session.  This is the persistence data object for <see cref="SessionCollectionUI"/> which tracks which objects are included in the session e.g. between RDMP GUI restarts
/// </summary>
public class SessionCollection :PersistableObjectCollection
{
    public string SessionName { get; private set; }

    /// <summary>
    /// for persistence, do not use
    /// </summary>
    public SessionCollection()
    {
    }

    public SessionCollection(string name): this()
    {
        SessionName = name;
    }

    public override string SaveExtraText()
    {
        return PersistStringHelper.SaveDictionaryToString(new Dictionary<string, string> {{nameof(SessionName), SessionName}});
    }

    public override void LoadExtraText(string s)
    {
        SessionName = PersistStringHelper.GetValueIfExistsFromPersistString(nameof(SessionName), s);
    }
}