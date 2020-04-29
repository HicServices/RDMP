// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.Reports
{
    class CustomMetadataReportTests : UnitTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public void TestCustomMetadataReport_SingleCatalogue(bool oneFile)
        {
            var cata = WhenIHaveA<Catalogue>();
            cata.Name = "ffff";
            cata.Description = null;
            cata.SaveToDatabase();

            var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
            var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

            if(outDir.Exists)
                outDir.Delete(true);
            
            outDir.Create();

            File.WriteAllText(template.FullName,
                @"| Name | Desc|
| $Name | $Description |");

            var cmd = new ExecuteCommandExtractMetadata(null, new[] {cata}, outDir, template, "$Name.md", oneFile);
            cmd.Execute();

            var outFile = Path.Combine(outDir.FullName, "ffff.md");

            FileAssert.Exists(outFile);
            var resultText = File.ReadAllText(outFile);

            StringAssert.AreEqualIgnoringCase(@"| Name | Desc|
| ffff |  |",resultText.TrimEnd());
        }

        
        [TestCase(true)]
        [TestCase(false)]
        public void TestCustomMetadataReport_TwoCatalogues(bool oneFile)
        {
            var cata = WhenIHaveA<Catalogue>();
            cata.Name = "Forest";
            cata.Administrative_contact_email = "me@g.com";
            cata.SaveToDatabase();

            var cata2 = WhenIHaveA<Catalogue>();
            cata2.Name = "Trees";
            cata2.Description = "trollolol";
            cata2.SaveToDatabase();

            var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.xml"));
            var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

            if(outDir.Exists)
                outDir.Delete(true);
            
            outDir.Create();

            File.WriteAllText(template.FullName,
                @"<DataSet>
<Name>$Name</Name>
<Desc>$Description</Desc>
<Email>$Administrative_contact_email</Email>
</DataSet>");


            var cmd = new ExecuteCommandExtractMetadata(null, new[] {cata,cata2}, outDir, template, 
                oneFile ? "results.xml" : "$Name.xml", oneFile);
            cmd.Execute();

            if (oneFile)
            {
                var outFile = Path.Combine(outDir.FullName, "results.xml");

                FileAssert.Exists(outFile);
                var resultText = File.ReadAllText(outFile);

                StringAssert.AreEqualIgnoringCase(
                    @"<DataSet>
<Name>Forest</Name>
<Desc></Desc>
<Email>me@g.com</Email>
</DataSet>
<DataSet>
<Name>Trees</Name>
<Desc>trollolol</Desc>
<Email></Email>
</DataSet>",resultText.TrimEnd());
            }
            else
            {
                var outFile1 = Path.Combine(outDir.FullName, "Forest.xml");
                var outFile2 = Path.Combine(outDir.FullName, "Trees.xml");

                FileAssert.Exists(outFile1);
                FileAssert.Exists(outFile2);

                var resultText1 = File.ReadAllText(outFile1);
                StringAssert.AreEqualIgnoringCase(
                    @"<DataSet>
<Name>Forest</Name>
<Desc></Desc>
<Email>me@g.com</Email>
</DataSet>".Trim(),resultText1.Trim());

                var resultText2 = File.ReadAllText(outFile2);
                
                StringAssert.AreEqualIgnoringCase(
                    @"<DataSet>
<Name>Trees</Name>
<Desc>trollolol</Desc>
<Email></Email>
</DataSet>".Trim(),resultText2.Trim());

            }

        }
    }
}
