// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Rdmp.Core.Reports.DublinCore;

/// <summary>
///     Class describing the RDMP exposed attributes defined in Dublin Core metadata format.
/// </summary>
public class DublinCoreDefinition
{
    /// <summary>
    ///     A name given to the resource. See http://www.dublincore.org/documents/dces/
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///     Dublin Core property see http://www.dublincore.org/documents/dces/
    /// </summary>
    public string Alternative { get; set; }

    /// <summary>
    ///     Dublin Core property see http://www.dublincore.org/documents/dces/
    /// </summary>
    public string Subject { get; set; }

    /// <summary>
    ///     Dublin Core property see http://www.dublincore.org/documents/dces/
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///     Dublin Core property see http://www.dublincore.org/documents/dces/
    /// </summary>
    public string Publisher { get; set; }

    /// <summary>
    ///     Dublin Core property see http://www.dublincore.org/documents/dces/
    /// </summary>
    public Uri IsPartOf { get; set; }

    /// <summary>
    ///     Dublin Core property see http://www.dublincore.org/documents/dces/
    /// </summary>
    public Uri Identifier { get; set; }

    /// <summary>
    ///     Dublin Core property see http://www.dublincore.org/documents/dces/
    /// </summary>
    public DateTime? Modified { get; set; }

    /// <summary>
    ///     Dublin Core property see http://www.dublincore.org/documents/dces/
    /// </summary>
    public string Format { get; set; }

    /// <summary>
    ///     Writes the defintion in the format listed in http://dublincore.org/documents/dc-xml-guidelines/
    /// </summary>
    /// <param name="to"></param>
    public void WriteXml(Stream to)
    {
        XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
        XNamespace dc = "http://purl.org/dc/elements/1.1/";
        XNamespace dcterms = "http://purl.org/dc/terms/";


        var xsiAttr = new XAttribute(XNamespace.Xmlns + "xsi", xsi);
        var dcAttr = new XAttribute(XNamespace.Xmlns + "dc", dc);
        var dctermsAttr = new XAttribute(XNamespace.Xmlns + "dcterms", dcterms);

        var doc = new XDocument(new XElement("metadata", xsiAttr, dcAttr, dctermsAttr));
        doc.Root.Add(new XElement(dc + "title", Title));
        doc.Root.Add(new XElement(dcterms + "alternative", Alternative));
        doc.Root.Add(new XElement(dc + "subject", Subject));
        doc.Root.Add(new XElement(dc + "description", Description));
        doc.Root.Add(new XElement(dc + "publisher", Publisher));

        doc.Root.Add(new XElement(dcterms + "isPartOf", new XAttribute(xsi + "type", "dcterms:URI"), IsPartOf));
        doc.Root.Add(new XElement(dcterms + "identifier", new XAttribute(xsi + "type", "dcterms:URI"), Identifier));

        //<dcterms:modified xsi:type="dcterms:W3CDTF">
        if (Modified.HasValue)
            doc.Root.Add(new XElement(dcterms + "modified", new XAttribute(xsi + "type", "dcterms:W3CDTF"),
                Modified.Value.ToString("yyyy-MM-dd")));

        doc.Root.Add(new XElement(dc + "format", new XAttribute(xsi + "type", "dcterms:IMT"), Format));

        using var sw = new StreamWriter(to);
        sw.Write(doc.ToString(SaveOptions.None));
    }

    /// <summary>
    ///     Parses elements such as title, subject, description etc out of a metadata tag expected to follow the Dublin Core
    ///     Xml guidlines ( http://dublincore.org/documents/dc-xml-guidelines/)
    /// </summary>
    /// <param name="element"></param>
    public void LoadFrom(XElement element)
    {
        if (element.Name != "metadata")
            throw new XmlException($"Expected metadata element but got {element}");

        var descendants = element.Descendants().ToArray();
        Title = GetElement(descendants, "title", true);
        Alternative = GetElement(descendants, "alternative", false);
        Subject = GetElement(descendants, "subject", false);
        Description = GetElement(descendants, "description", false);
        Publisher = GetElement(descendants, "publisher", false);
        IsPartOf = GetElementUri(descendants, "ispartof", false);
        Identifier = GetElementUri(descendants, "identifier", false);
        Modified = GetElementDateTime(descendants, "modified", false);
        Format = GetElement(descendants, "format", false);
    }

    private static DateTime? GetElementDateTime(XElement[] descendants, string tagLocalName, bool mandatory)
    {
        var stringValue = GetElement(descendants, tagLocalName, mandatory);
        return string.IsNullOrWhiteSpace(stringValue) ? null : DateTime.Parse(stringValue);
    }

    private static Uri GetElementUri(XElement[] descendants, string tagLocalName, bool mandatory)
    {
        var stringValue = GetElement(descendants, tagLocalName, mandatory);
        return string.IsNullOrWhiteSpace(stringValue) ? null : new Uri(stringValue);
    }

    private static string GetElement(XElement[] descendants, string tagLocalName, bool mandatory)
    {
        var match = descendants.FirstOrDefault(e =>
            e.Name.LocalName.Equals(tagLocalName, StringComparison.CurrentCultureIgnoreCase));

        if (match == null)
            return mandatory ? throw new XmlException($"Failed to find mandatory tag {tagLocalName}") : null;

        return match.Value.Trim();
    }
}