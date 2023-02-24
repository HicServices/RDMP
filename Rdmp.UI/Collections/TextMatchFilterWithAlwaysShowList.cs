// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using BrightIdeasSoftware;

namespace Rdmp.UI.Collections;

/// <summary>
/// <see cref="TextMatchFilter"/> which always shows a given list of objects (the alwaysShowList).  This class is an <see cref="IModelFilter"/>
/// for use with ObjectListView
/// </summary>
public class TextMatchFilterWithAlwaysShowList : TextMatchFilter
{
    public HashSet<object>  AlwaysShow = new();
    private string[] _tokens;
    private CompositeAllFilter _compositeFilter;

    public TextMatchFilterWithAlwaysShowList(IEnumerable<object> alwaysShow ,ObjectListView olv, string text, StringComparison comparison): base(olv, text, comparison)
    {
        if(!string.IsNullOrWhiteSpace(text) && text.Contains(" "))
        {
            var filters = new List<IModelFilter>();
                
            _tokens = text.Split(' ');
            foreach (var token in _tokens)
                filters.Add(new TextMatchFilter(olv,token,comparison));

            _compositeFilter = new CompositeAllFilter(filters);
        }

        foreach (var o in alwaysShow)
            AlwaysShow.Add(o);
    }

    /// <summary>
    /// Returns true if the object should be included in the list
    /// </summary>
    /// <param name="modelObject"></param>
    /// <returns></returns>
    public override bool Filter(object modelObject)
    {
        //gets us the highlight and composite match if the user put in spaces
        var showing = _compositeFilter != null ? _compositeFilter.Filter(modelObject) : base.Filter(modelObject);

        //if its in the always show it
        if (AlwaysShow.Contains(modelObject))
            return true;

        return showing;
    }
}