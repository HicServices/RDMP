using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using NUnit.Framework;
using Rdmp.Core.Reports.DublinCore;

namespace Rdmp.UI.Tests.CommandExecution
{
    class ExecuteCommandImportDublinCoreFormatTests
    {

        /// <summary>
        /// This test also appears in the Rdmp.Tests project since it behaves differently in different runtime. 
        /// </summary>
        [Test]
        public void Test_DublinCore_WriteReadFile_Net461()
        {
            var def1 = new DublinCoreDefinition()
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

            var fi = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "dublin.xml"));

            using(var outStream = fi.OpenWrite())
                def1.WriteXml(outStream);

            using (var inStream = fi.OpenRead())
            {
                var def2 = new DublinCoreDefinition();
                var doc = XDocument.Load(inStream);
                def2.LoadFrom(doc.Root);
            }
        }
    }
}
