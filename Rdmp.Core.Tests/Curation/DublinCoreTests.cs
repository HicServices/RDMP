// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Xml.Linq;
using NUnit.Framework;
using Rdmp.Core.Reports.DublinCore;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation;

[Category("Unit")]
internal class DublinCoreTests
{
    [Test]
    public void TestWritingDocument()
    {
        var def = new DublinCoreDefinition
        {
            Title = "ssssshh",
            Alternative = "O'Rly",
            Description = "Description of stuff",
            Format = "text/html",
            Identifier = new Uri("http://foo.com"),
            Publisher = "University of Dundee",
            IsPartOf = new Uri("http://foo2.com"),
            Modified = new DateTime(2001, 1, 1),
            Subject = "Interesting, PayAttention, HighPriority, Omg"
        };

        var f = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "dublinTest.xml"));

        using (var fw = f.OpenWrite())
        {
            def.WriteXml(fw);
        }

        var contents = File.ReadAllText(f.FullName);
        Assert.That(contents, Does.Contain(def.Title));
        Assert.That(contents, Does.Contain(def.Alternative));
        Assert.That(contents, Does.Contain(def.Description));
        Assert.That(contents, Does.Contain(def.Format));
        Assert.That(contents, Does.Contain(def.Publisher));
        Assert.That(contents, Does.Contain(def.Subject));

        Assert.That(contents, Does.Contain("2001-01-01"));
        Assert.That(contents, Does.Contain("http://foo.com"));
        Assert.That(contents, Does.Contain("http://foo2.com"));

        var def2 = new DublinCoreDefinition();
        def2.LoadFrom(XDocument.Load(f.FullName).Root);

        Assert.Multiple(() =>
        {
            Assert.That(def2.Title, Is.EqualTo(def.Title));
            Assert.That(def2.Alternative, Is.EqualTo(def.Alternative));
            Assert.That(def2.Description, Is.EqualTo(def.Description));
            Assert.That(def2.Format, Is.EqualTo(def.Format));
            Assert.That(def2.Publisher, Is.EqualTo(def.Publisher));
            Assert.That(def2.Subject, Is.EqualTo(def.Subject));

            Assert.That(def2.Modified, Is.EqualTo(def.Modified));
            Assert.That(def2.IsPartOf.ToString(), Is.EqualTo(def.IsPartOf.ToString()));
            Assert.That(def2.Identifier.ToString(), Is.EqualTo(def.Identifier.ToString()));
        });
    }

    [Test]
    public void TestReadingDocument()
    {
        const string xml = """
                           <?xml version="1.0"?>

                           <metadata
                             xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                             xsi:schemaLocation="http://example.org/myapp/ http://example.org/myapp/schema.xsd"
                             xmlns:dc="http://purl.org/dc/elements/1.1/"
                             xmlns:dcterms="http://purl.org/dc/terms/">
                           
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
                             <dc:subject xsi:type="dcterms:DDC">
                               062
                             </dc:subject>
                             <dc:subject xsi:type="dcterms:UDC">
                               061(410)
                             </dc:subject>
                             <dc:description>
                               UKOLN is a national focus of expertise in digital information
                               management. It provides policy, research and awareness services
                               to the UK library, information and cultural heritage communities.
                               UKOLN is based at the University of Bath.
                             </dc:description>
                             <dc:description xml:lang="fr">
                               UKOLN est un centre national d'expertise dans la gestion de l'information
                               digitale.
                             </dc:description>
                             <dc:publisher>
                               UKOLN, University of Bath
                             </dc:publisher>
                             <dcterms:isPartOf xsi:type="dcterms:URI">
                               http://www.bath.ac.uk/
                             </dcterms:isPartOf>
                             <dc:identifier xsi:type="dcterms:URI">
                               http://www.ukoln.ac.uk/
                             </dc:identifier>
                             <dcterms:modified xsi:type="dcterms:W3CDTF">
                               2001-07-18
                             </dcterms:modified>
                             <dc:format xsi:type="dcterms:IMT">
                               text/html
                             </dc:format>
                           </metadata>
                           """;

        var fi = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "dublinTestReading.xml"));
        File.WriteAllText(fi.FullName, xml);

        var doc = XDocument.Load(fi.FullName);

        var def = new DublinCoreDefinition();

        def.LoadFrom(doc.Root);

        Assert.Multiple(() =>
        {
            Assert.That(DatabaseTests.AreBasicallyEquals("UKOLN", def.Title));
            Assert.That(DatabaseTests.AreBasicallyEquals("UK Office for Library and Information Networking",
                def.Alternative));
            Assert.That(DatabaseTests.AreBasicallyEquals(@"national centre, network information support, library
    community, awareness, research, information services,public
    library networking, bibliographic management, distributed
    library systems, metadata, resource discovery,
    conferences,lectures, workshops", def.Subject));

            Assert.That(DatabaseTests.AreBasicallyEquals(@"UKOLN is a national focus of expertise in digital information
    management. It provides policy, research and awareness services
    to the UK library, information and cultural heritage communities.
    UKOLN is based at the University of Bath.", def.Description));

            Assert.That(DatabaseTests.AreBasicallyEquals("UKOLN, University of Bath", def.Publisher));
            Assert.That(def.IsPartOf.AbsoluteUri, Is.EqualTo("http://www.bath.ac.uk/").IgnoreCase);
            Assert.That(def.Identifier.AbsoluteUri, Is.EqualTo("http://www.ukoln.ac.uk/").IgnoreCase);
            Assert.That(DatabaseTests.AreBasicallyEquals(new DateTime(2001, 07, 18), def.Modified));
        });
    }

    /// <summary>
    /// This test also appears in the Rdmp.UI.Tests project since it behaves differently in different runtime.
    /// </summary>
    [Test]
    public void Test_DublinCore_WriteReadFile_NetCore()
    {
        var def1 = new DublinCoreDefinition
        {
            Title = "ssssshh",
            Alternative = "O'Rly",
            Description = "Description of stuff",
            Format = "text/html",
            Identifier = new Uri("http://foo.com"),
            Publisher = "University of Dundee",
            IsPartOf = new Uri("http://foo2.com"),
            Modified = new DateTime(2001, 1, 1),
            Subject = "Interesting, PayAttention, HighPriority, Omg"
        };

        var fi = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "dublin.xml"));

        using (var outStream = fi.OpenWrite())
        {
            def1.WriteXml(outStream);
        }

        using var inStream = fi.OpenRead();
        var def2 = new DublinCoreDefinition();
        var doc = XDocument.Load(inStream);
        def2.LoadFrom(doc.Root);
    }
}