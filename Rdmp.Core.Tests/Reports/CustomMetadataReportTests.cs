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
using Rdmp.Core.Reports;
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

        [Test]
        public void TestCustomMetadataReport_SingleCatalogueWithNoDQEResults()
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
                @"| Name | Desc| Range |
| $Name | $Description | $StartDate$EndDate$DateRange |");

            var cmd = new ExecuteCommandExtractMetadata(null, new[] {cata}, outDir, template, "$Name.md", false);
            cmd.Execute();

            var outFile = Path.Combine(outDir.FullName, "ffff.md");

            FileAssert.Exists(outFile);
            var resultText = File.ReadAllText(outFile);

            StringAssert.AreEqualIgnoringCase(@"| Name | Desc| Range |
| ffff |  | Unknown |",resultText.TrimEnd());
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

        [TestCase(true)]
        [TestCase(false)]
        public void TestCustomMetadataReport_CatalogueItems(bool oneFile)
        {
            var cata = WhenIHaveA<Catalogue>();
            cata.Name = "ffff";
            cata.Description = "A cool dataset with interesting stuff";
            cata.SaveToDatabase();

            var cataItem1 = new CatalogueItem(RepositoryLocator.CatalogueRepository, cata, "Col1");
            cataItem1.Description = "some info about column 1";
            cataItem1.SaveToDatabase();

            var cataItem2 = new CatalogueItem(RepositoryLocator.CatalogueRepository, cata, "Col2");
            cataItem2.Description = "some info about column 2";
            cataItem2.SaveToDatabase();

            var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
            var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

            if(outDir.Exists)
                outDir.Delete(true);
            
            outDir.Create();

            File.WriteAllText(template.FullName,
                @"## $Name
$Description
| Column | Description |
$foreach CatalogueItem
| $Name | $Description |
$end");

            var cmd = new ExecuteCommandExtractMetadata(null, new[] {cata}, outDir, template, "$Name.md", oneFile);
            cmd.Execute();

            var outFile = Path.Combine(outDir.FullName, "ffff.md");

            FileAssert.Exists(outFile);
            var resultText = File.ReadAllText(outFile);

            StringAssert.AreEqualIgnoringCase(@"## ffff
A cool dataset with interesting stuff
| Column | Description |
| Col1 | some info about column 1 |
| Col2 | some info about column 2 |",resultText.TrimEnd());
        }

        [Test]
        public void TestCustomMetadataReport_TwoCataloguesWithItems()
        {
            var c1 = WhenIHaveA<Catalogue>();
            c1.Name = "ffff";
            c1.Description = "A cool dataset with interesting stuff";
            c1.SaveToDatabase();

            var c1ci1 = new CatalogueItem(RepositoryLocator.CatalogueRepository, c1, "Col1");
            c1ci1.Description = "some info about column 1";
            c1ci1.SaveToDatabase();

            var c1ci2 = new CatalogueItem(RepositoryLocator.CatalogueRepository, c1, "Col2");
            c1ci2.Description = "some info about column 2";
            c1ci2.SaveToDatabase();

            
            var c2 = WhenIHaveA<Catalogue>();
            c2.Name = "Demog";
            c2.Description = "This is expensive dataset: $30 to use";
            c2.SaveToDatabase();

            var c2ci1 = new CatalogueItem(RepositoryLocator.CatalogueRepository, c2, "Name");
            c2ci1.Description = "Name of the patient";
            c2ci1.SaveToDatabase();
            var c2ci2 = new CatalogueItem(RepositoryLocator.CatalogueRepository, c2, "Address");
            c2ci2.Description = "Where they live";
            c2ci2.SaveToDatabase();
            var c2ci3 = new CatalogueItem(RepositoryLocator.CatalogueRepository, c2, "Postcode");
            c2ci3.Description = "Patients postcode";
            c2ci3.SaveToDatabase();


            var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
            var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

            if(outDir.Exists)
                outDir.Delete(true);
            
            outDir.Create();

            File.WriteAllText(template.FullName,
                @"## $Name
$Description
Price: $30
| Column | Description |
$foreach CatalogueItem

| $Name | $Description |
$end");

            var cmd = new ExecuteCommandExtractMetadata(null, new[] {c1,c2}, outDir, template, "Datasets.md", true);
            cmd.Execute();

            var outFile = Path.Combine(outDir.FullName, "Datasets.md");

            FileAssert.Exists(outFile);
            var resultText = File.ReadAllText(outFile);

            StringAssert.AreEqualIgnoringCase(@"## ffff
A cool dataset with interesting stuff
Price: $30
| Column | Description |
| Col1 | some info about column 1 |
| Col2 | some info about column 2 |
## Demog
This is expensive dataset: $30 to use
Price: $30
| Column | Description |
| Name | Name of the patient |
| Address | Where they live |
| Postcode | Patients postcode |",resultText.TrimEnd());
        }

        [Test]
        public void TestCustomMetadataReport_CatalogueItems_NoEndBlock()
        {
            var cata = WhenIHaveA<Catalogue>();
            cata.Name = "ffff";
            cata.Description = "A cool dataset with interesting stuff";
            cata.SaveToDatabase();

            var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
            var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

            if(outDir.Exists)
                outDir.Delete(true);
            
            outDir.Create();

            File.WriteAllText(template.FullName,
                @"## $Name
$Description
| Column | Description |
$foreach CatalogueItem
| $Name | $Description |");

            var cmd = new ExecuteCommandExtractMetadata(null, new[] {cata}, outDir, template, "$Name.md", false);
            var ex = Assert.Throws<CustomMetadataReportException>(cmd.Execute);

            Assert.AreEqual(4,ex.LineNumber);
            Assert.AreEqual("Expected $end to match $foreach which started on line 4",ex.Message);
        }
        
        [Test]
        public void TestCustomMetadataReport_CatalogueItems_TooManyForeachBlocks()
        {
            var cata = WhenIHaveA<Catalogue>();
            cata.Name = "ffff";
            cata.Description = "A cool dataset with interesting stuff";
            cata.SaveToDatabase();

            var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
            var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

            if(outDir.Exists)
                outDir.Delete(true);
            
            outDir.Create();

            File.WriteAllText(template.FullName,
                @"## $Name
$Description
| Column | Description |
$foreach CatalogueItem
| $Name | $Description |
$foreach CatalogueItem
| $Name | $Description |
$end
$end");

            var cmd = new ExecuteCommandExtractMetadata(null, new[] {cata}, outDir, template, "$Name.md", false);
            var ex = Assert.Throws<CustomMetadataReportException>(cmd.Execute);

            Assert.AreEqual(6,ex.LineNumber);
            StringAssert.StartsWith("Error, encountered '$foreach CatalogueItem' on line 6",ex.Message);
        }
    }
}
