// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using System;
using System.Collections.Generic;

namespace Rdmp.Core.Providers;

public static class RdmpEnumerableExtensions
{
    public static Dictionary<TKey, TElement> ToDictionaryEx<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) where TKey : notnull
    {
        var d = new Dictionary<TKey, TElement>();
        foreach (var element in source)
        {
            try
            {
                d.Add(keySelector(element), elementSelector(element));
            }
            catch (Exception ex)
            {
                if(element is IMapsDirectlyToDatabaseTable m)
                {
                    throw new Exception($"Failed to add {element} ({m.GetType().Name}, ID={m.ID}) to Dictionary.  Repository was {m.Repository}", ex);
                }
                else
                {
                    throw new Exception($"Failed to add {element} to Dictionary", ex);
                }

                    
            }
        }

        return d;
    }
}