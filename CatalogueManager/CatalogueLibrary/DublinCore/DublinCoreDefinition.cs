using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace CatalogueLibrary.DublinCore
{
    class DublinCoreDefinition
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

            doc.Root.Add(new XElement(dcterms + "isPartOf", new XAttribute(xsi + "type", "dcters:URI"), IsPartOf));
            doc.Root.Add(new XElement(dcterms + "identifier", new XAttribute(xsi + "type", "dcters:URI"), Identifier));

            //<dcterms:modified xsi:type="dcterms:W3CDTF">
            if (Modified.HasValue)
                doc.Root.Add(new XElement(dcterms + "modified", new XAttribute(xsi + "type", "dcterms:W3CDTF"), Modified.Value.ToString("yyyy-MM-dd")));

            doc.Root.Add(new XElement(dc + "format", new XAttribute(xsi + "type", "dcterms:IMT"), Format));
            
            using(StreamWriter sw = new StreamWriter(to))
                sw.Write(doc.ToString(SaveOptions.None));
        }
    }
}
