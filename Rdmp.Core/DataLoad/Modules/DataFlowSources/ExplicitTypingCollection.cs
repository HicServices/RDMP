// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Rdmp.Core.Curation.Data.DataLoad;

namespace Rdmp.Core.DataLoad.Modules.DataFlowSources;

/// <summary>
///     A collection of column names with explicitly defined column types that the user wants to force where present.  e.g.
///     they loading a CSV and they get values
///     "291","195" but they know that some codes are like "012" and wish to preserve this leading 0s so they can
///     explicitly define the column as being a string.
///     <para>
///         This class can be used by [DemandsInitialization] properties and it will launch its custom UI:
///         ExplicitTypingCollectionUI
///     </para>
/// </summary>
public class ExplicitTypingCollection : ICustomUIDrivenClass
{
    /// <summary>
    ///     A dictionary of names (e.g. column names) which must have specific C# data types
    /// </summary>
    public Dictionary<string, Type> ExplicitTypesCSharp = new();

    /// <inheritdoc />
    public void RestoreStateFrom(string value)
    {
        if (value == null)
            return;

        var doc = new XmlDocument();
        doc.Load(new StringReader(value));


        //get the dictionary tag
        var typesNode = doc.GetElementsByTagName("ExplicitTypesCSharp").Cast<XmlNode>().Single();
        var dictionary = typesNode.InnerText;

        var lines = dictionary.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

        for (var i = 0; i < lines.Length; i += 2)
            ExplicitTypesCSharp.Add(lines[i], Type.GetType(lines[i + 1]));
    }

    /// <inheritdoc />
    public string SaveStateToString()
    {
        var sb = new StringBuilder();

        //Add anything new here

        sb.AppendLine("<ExplicitTypesCSharp>");
        foreach (var kvp in ExplicitTypesCSharp)
        {
            sb.AppendLine(kvp.Key);
            sb.AppendLine(kvp.Value.FullName);
        }

        sb.AppendLine("</ExplicitTypesCSharp>");

        return sb.ToString();
    }
}