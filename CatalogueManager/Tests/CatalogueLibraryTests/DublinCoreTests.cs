using System;
using System.IO;
using CatalogueLibrary.DublinCore;
using NUnit.Framework;

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
    }
}
