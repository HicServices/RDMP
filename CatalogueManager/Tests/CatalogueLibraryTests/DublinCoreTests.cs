using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using CatalogueLibrary.DublinCore;
using NUnit.Framework;
using Tests.Common;

namespace CatalogueLibraryTests
{
    class DublinCoreTests
    {
        [Test]
        public void TestWrittingDocument()
        {
            var def = new DublinCoreDefinition()
            {
                Title =  "ssssshh",
                Alternative =  "O'Rly",
                Description = "Description of stuff",
                Format = "text/html",
                Identifier = new Uri("http://foo.com"),
                Publisher = "University of Dundee",
                IsPartOf = new Uri("http://foo2.com"),
                Modified = new DateTime(2001,1,1),
                Subject = "Interesting, PayAttention, HighPriority, Omg"
            };

            var f = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "dublinTest.xml"));

            using(var fw = f.OpenWrite())
                def.WriteXml(fw);

            var contents = File.ReadAllText(f.FullName);
            StringAssert.Contains(def.Title, contents);
            StringAssert.Contains(def.Alternative, contents);
            StringAssert.Contains(def.Description, contents);
            StringAssert.Contains(def.Format, contents);
            StringAssert.Contains(def.Publisher, contents);
            StringAssert.Contains(def.Subject, contents);

            StringAssert.Contains("2001-01-01", contents);
            StringAssert.Contains("http://foo.com", contents);
            StringAssert.Contains("http://foo2.com", contents);
        }

        [Test]
        public void TestReadingDocument()
        {
            string xml =
                @"<?xml version=""1.0""?>

<metadata
  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
  xsi:schemaLocation=""http://example.org/myapp/ http://example.org/myapp/schema.xsd""
  xmlns:dc=""http://purl.org/dc/elements/1.1/""
  xmlns:dcterms=""http://purl.org/dc/terms/"">

  <dc:title>
    UKOLN
  </dc:title>
  <dcterms:alternative>
    UK Office for Library and Information Networking
  </dcterms:alternative>
  <dc:subject>
    national centre, network information support, library
    community, awareness, research, information services,public
    library networking, bibliographic management, distributed
    library systems, metadata, resource discovery,
    conferences,lectures, workshops
  </dc:subject>
  <dc:subject xsi:type=""dcterms:DDC"">
    062
  </dc:subject>
  <dc:subject xsi:type=""dcterms:UDC"">
    061(410)
  </dc:subject>
  <dc:description>
    UKOLN is a national focus of expertise in digital information
    management. It provides policy, research and awareness services
    to the UK library, information and cultural heritage communities.
    UKOLN is based at the University of Bath.
  </dc:description>
  <dc:description xml:lang=""fr"">
    UKOLN est un centre national d'expertise dans la gestion de l'information
    digitale.
  </dc:description>
  <dc:publisher>
    UKOLN, University of Bath
  </dc:publisher>
  <dcterms:isPartOf xsi:type=""dcterms:URI"">
    http://www.bath.ac.uk/
  </dcterms:isPartOf>
  <dc:identifier xsi:type=""dcterms:URI"">
    http://www.ukoln.ac.uk/
  </dc:identifier>
  <dcterms:modified xsi:type=""dcterms:W3CDTF"">
    2001-07-18
  </dcterms:modified>
  <dc:format xsi:type=""dcterms:IMT"">
    text/html
  </dc:format>
</metadata>";

            var fi = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "dublinTestReading.xml"));
            File.WriteAllText(fi.FullName,xml);

            var doc = XDocument.Load(fi.FullName);

            var def = new DublinCoreDefinition();

            def.LoadFrom(doc.Root);

            Assert.IsTrue(DatabaseTests.AreBasicallyEquals("UKOLN",def.Title));
            Assert.IsTrue(DatabaseTests.AreBasicallyEquals("UK Office for Library and Information Networking", def.Alternative));
            Assert.IsTrue(DatabaseTests.AreBasicallyEquals(@"national centre, network information support, library
    community, awareness, research, information services,public
    library networking, bibliographic management, distributed
    library systems, metadata, resource discovery,
    conferences,lectures, workshops", def.Subject));

            Assert.IsTrue(DatabaseTests.AreBasicallyEquals(@"UKOLN is a national focus of expertise in digital information
    management. It provides policy, research and awareness services
    to the UK library, information and cultural heritage communities.
    UKOLN is based at the University of Bath.", def.Description));

            Assert.IsTrue(DatabaseTests.AreBasicallyEquals("UKOLN, University of Bath", def.Publisher));
            StringAssert.AreEqualIgnoringCase("http://www.bath.ac.uk/", def.IsPartOf.AbsoluteUri);
            StringAssert.AreEqualIgnoringCase("http://www.ukoln.ac.uk/", def.Identifier.AbsoluteUri);
            Assert.IsTrue(DatabaseTests.AreBasicallyEquals(new DateTime(2001,07,18),def.Modified));
        }
    }
}
