// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Xml;

namespace Rdmp.Core.ReusableLibraryCode.Comments;

public static class XmlDocumentExtensions
{
    public static void IterateThroughAllNodes(
        this XmlDocument doc,
        Action<XmlNode> elementVisitor)
    {
        if (doc != null && elementVisitor != null)
            foreach (XmlNode node in doc.ChildNodes)
                doIterateNode(node, elementVisitor);
    }

    public static void IterateThroughAllNodes(
        this XmlNode node,
        Action<XmlNode> elementVisitor)
    {
        foreach (XmlNode c in node.ChildNodes) doIterateNode(c, elementVisitor);
    }

    private static void doIterateNode(
        XmlNode node,
        Action<XmlNode> elementVisitor)
    {
        elementVisitor(node);

        foreach (XmlNode childNode in node.ChildNodes) doIterateNode(childNode, elementVisitor);
    }
}