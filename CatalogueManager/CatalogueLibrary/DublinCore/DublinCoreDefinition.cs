using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CatalogueLibrary.DublinCore
{
    /// <summary>
    /// Class describing the RDMP exposed attributes defined in Dublin Core metadata format.
    /// </summary>
    public class DublinCoreDefinition
    {
        public string Title { get; set; }
        public string Alternative { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Publisher { get; set; }
        public Uri IsPartOf { get; set; }
        public Uri Identifier { get; set; }
        public DateTime? Modified { get; set; }
        public string Format { get; set; }

        /// <summary>
        /// Writes the defintion in the format listed in http://dublincore.org/documents/dc-xml-guidelines/
        /// </summary>
        /// <param name="to"></param>
        public void WriteXml(Stream to)
        {

            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
            XNamespace dc = "http://purl.org/dc/elements/1.1/";
            XNamespace dcterms = "http://purl.org/dc/terms/";


            var xsiAttr = new XAttribute(XNamespace.Xmlns + "xsi", xsi);
            var dcAttr = new XAttribute(XNamespace.Xmlns + "dc", dc);
            var dctermsAttr = new XAttribute(XNamespace.Xmlns + "dcterms",dcterms);
            
            XDocument doc = new XDocument(new XElement("metadata",xsiAttr,dcAttr,dctermsAttr));
            doc.Root.Add(new XElement(dc + "title", Title));
            doc.Root.Add(new XElement(dcterms + "alternative", Alternative));
            doc.Root.Add(new XElement(dc + "subject", Subject));
            doc.Root.Add(new XElement(dc + "description", Description));
            doc.Root.Add(new XElement(dc + "publisher", Publisher));

            doc.Root.Add(new XElement(dcterms + "isPartOf", new XAttribute(xsi + "type", "dcterms:URI"), IsPartOf));
            doc.Root.Add(new XElement(dcterms + "identifier", new XAttribute(xsi + "type", "dcterms:URI"), Identifier));

            //<dcterms:modified xsi:type="dcterms:W3CDTF">
            if (Modified.HasValue)
                doc.Root.Add(new XElement(dcterms + "modified", new XAttribute(xsi + "type", "dcterms:W3CDTF"), Modified.Value.ToString("yyyy-MM-dd")));

            doc.Root.Add(new XElement(dc + "format", new XAttribute(xsi + "type", "dcterms:IMT"), Format));
            
            using(StreamWriter sw = new StreamWriter(to))
                sw.Write(doc.ToString(SaveOptions.None));
        }

        public void LoadFrom(XElement element)
        {
            if(element.Name != "metadata")
                throw new XmlSyntaxException("Expected metadata element but got " + element);

            var descendants = element.Descendants().ToArray();
            Title = GetElement(descendants, "title",true);
            Alternative = GetElement(descendants, "alternative",false);
            Subject = GetElement(descendants, "subject", false);
            Description = GetElement(descendants, "description", false);
            Publisher = GetElement(descendants, "publisher", false);
            IsPartOf = GetElementUri(descendants, "ispartof",false);
            Identifier = GetElementUri(descendants, "identifier",false);
            Modified = GetElementDateTime(descendants, "modified",false);
            Format = GetElement(descendants, "format",false);
        }

        private DateTime? GetElementDateTime(XElement[] descendants, string tagLocalName, bool mandatory)
        {
            var stringValue = GetElement(descendants, tagLocalName, mandatory);
            if (string.IsNullOrWhiteSpace(stringValue))
                return null;

            return DateTime.Parse(stringValue);
        }

        private Uri GetElementUri(XElement[] descendants, string tagLocalName, bool mandatory)
        {
            var stringValue = GetElement(descendants, tagLocalName, mandatory);
            if (string.IsNullOrWhiteSpace(stringValue))
                return null;

            return new Uri(stringValue);
        }

        private string GetElement(XElement[] descendants, string tagLocalName, bool mandatory)
        {
            var match = descendants.FirstOrDefault(e => e.Name.LocalName.Equals(tagLocalName,StringComparison.CurrentCultureIgnoreCase));

            if (match == null)
                if(mandatory)
                    throw new XmlSyntaxException("Failed to find mandatory tag " + tagLocalName);
                else
                    return null;

            return match.Value.Trim();
        }
    }
}
