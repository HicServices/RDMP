// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.Reports.DublinCore;

/// <summary>
///     Handles updating / extracting data from RDMP objects using the interchange object DublinCoreDefinition
/// </summary>
public class DublinCoreTranslater
{
    /// <summary>
    ///     Populates the given <paramref name="toFill" /> with the descriptions stored in <paramref name="fillWith" />.   This
    ///     will overwrite previous values.
    ///     <para>Not all object types T are supported</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="toFill"></param>
    /// <param name="fillWith"></param>
    public static void Fill<T>(T toFill, DublinCoreDefinition fillWith)
    {
        if (toFill is Catalogue c)
        {
            //only overwritte name if Catalogue has default blank name
            if (c.Name != null && c.Name.StartsWith("New Catalogue ", StringComparison.CurrentCultureIgnoreCase))
                c.Name = fillWith.Title;

            c.Description = fillWith.Description;
            c.Search_keywords = fillWith.Subject;

            //only change Acronym if it was null before
            if (string.IsNullOrWhiteSpace(c.Acronym))
                c.Acronym = fillWith.Alternative;
        }
        else
        {
            throw new NotSupportedException(
                $"Did not know how to hydrate the Type {typeof(T)} from a DublinCoreDefinition");
        }
    }

    /// <summary>
    ///     Generates a <see cref="DublinCoreDefinition" /> for the provided <paramref name="generateFrom" /> by reading
    ///     specific fields out of the object
    ///     and translating them to dublin core metadata fields.
    ///     <para>Not all object types T are supported</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="generateFrom"></param>
    /// <returns></returns>
    public static DublinCoreDefinition GenerateFrom<T>(T generateFrom)
    {
        var toReturn = new DublinCoreDefinition();

        if (generateFrom is Catalogue c)
        {
            toReturn.Title = c.Name;
            toReturn.Description = c.Description;
            toReturn.Subject = c.Search_keywords;
            toReturn.Alternative = c.Acronym;
        }
        else
        {
            throw new NotSupportedException(
                $"Did not know how to extracta a DublinCoreDefinition from the Type {typeof(T)}");
        }

        return toReturn;
    }
}