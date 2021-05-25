// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Moq;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Reports;
using Tests.Common;

namespace Rdmp.Core.Tests.Reports
{
    class CustomMetadataReportTests : UnitTests
    {
        private void Setup2Catalogues(out Catalogue c1, out Catalogue c2)
        {
            c1 = WhenIHaveA<Catalogue>();
            c1.Name = "ffff";
            c1.Description = "A cool dataset with interesting stuff";
            c1.SaveToDatabase();

            var c1ci1 = new CatalogueItem(RepositoryLocator.CatalogueRepository, c1, "Col1");
            c1ci1.Description = "some info about column 1";
            c1ci1.SaveToDatabase();

            var c1ci2 = new CatalogueItem(RepositoryLocator.CatalogueRepository, c1, "Col2");
            c1ci2.Description = "some info about column 2";
            c1ci2.SaveToDatabase();
            
            c2 = WhenIHaveA<Catalogue>();
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
        }

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

            var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] {cata}, outDir, template, "$Name.md", oneFile,null);
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
| $Name | $Description | $DQE_StartDate$DQE_EndDate$DQE_DateRange |");

            var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] {cata}, outDir, template, "$Name.md", false,null);
            cmd.Execute();

            var outFile = Path.Combine(outDir.FullName, "ffff.md");

            FileAssert.Exists(outFile);
            var resultText = File.ReadAllText(outFile);

            StringAssert.AreEqualIgnoringCase(@"| Name | Desc| Range |
| ffff |  | Unknown |",resultText.TrimEnd());
        }

        [Test]
        public void TestCustomMetadataReport_SingleCatalogue_DQEMetrics()
        {
            var cata = WhenIHaveA<Catalogue>();
            cata.Name = "ffff";
            cata.Description = null;
            cata.SaveToDatabase();

            var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
            var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

            if (outDir.Exists)
                outDir.Delete(true);

            outDir.Create();

            File.WriteAllText(template.FullName,
                @"| Name | Desc| StartYear | EndYear | StartMonth | EndMonth | StartDay | EndDay | Range
| $Name | $Description | $DQE_StartYear | $DQE_EndYear | $DQE_StartMonth | $DQE_EndMonth | $DQE_StartDay | $DQE_EndDay | $DQE_StartYear-$DQE_EndYear |");


            var reporter = new CustomMetadataReport(RepositoryLocator)
            {
                NewlineSubstitution = null
            };
            
            DateTime? ignore;

            var moqDqe = new Mock<IDetermineDatasetTimespan>();
            moqDqe.Setup((f) => f.GetMachineReadableTimepsanIfKnownOf(cata, true, out ignore))
                .Returns(new Tuple<DateTime?, DateTime?>(new DateTime(2001,02,01 ), new DateTime(2002, 04,03)));

            reporter.TimespanCalculator = moqDqe.Object;

            reporter.GenerateReport(new[] { cata }, outDir, template, "$Name.md", false);

            var outFile = Path.Combine(outDir.FullName, "ffff.md");

            FileAssert.Exists(outFile);
            var resultText = File.ReadAllText(outFile);

            StringAssert.AreEqualIgnoringCase(@"| Name | Desc| StartYear | EndYear | StartMonth | EndMonth | StartDay | EndDay | Range
| ffff |  | 2001 | 2002 | 02 | 04 | 01 | 03 | 2001-2002 |", resultText.TrimEnd());
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


            var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] {cata,cata2}, outDir, template, 
                oneFile ? "results.xml" : "$Name.xml", oneFile,null);
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

            

            var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] {cata}, outDir, template, "$Name.md", oneFile,null);
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
            Setup2Catalogues(out Catalogue c1,out Catalogue c2);

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

            var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] {c1,c2}, outDir, template, "Datasets.md", true,null);
            cmd.Execute();

            var outFile = Path.Combine(outDir.FullName, "Datasets.md");

            FileAssert.Exists(outFile);
            var resultText = File.ReadAllText(outFile);

            StringAssert.AreEqualIgnoringCase(@"## Demog
This is expensive dataset: $30 to use
Price: $30
| Column | Description |
| Name | Name of the patient |
| Address | Where they live |
| Postcode | Patients postcode |
## ffff
A cool dataset with interesting stuff
Price: $30
| Column | Description |
| Col1 | some info about column 1 |
| Col2 | some info about column 2 |",resultText.TrimEnd());
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

            var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] {cata}, outDir, template, "$Name.md", false,null);
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

            var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] {cata}, outDir, template, "$Name.md", false,null);
            var ex = Assert.Throws<CustomMetadataReportException>(cmd.Execute);

            Assert.AreEqual(6,ex.LineNumber);
            StringAssert.StartsWith("Error, encountered '$foreach CatalogueItem' on line 6",ex.Message);
        }

        [Test]
        public void TestNewlineSubstitution()
        {
            var report = new CustomMetadataReport(RepositoryLocator);

            //default is no substitution
            Assert.IsNull(report.NewlineSubstitution);

            Assert.IsNull(report.ReplaceNewlines(null));

            Assert.AreEqual("aa\r\nbb",report.ReplaceNewlines("aa\r\nbb"));
            Assert.AreEqual("aa\nbb",report.ReplaceNewlines("aa\nbb"));

            report.NewlineSubstitution = "<br/>";

            Assert.AreEqual("aa<br/>bb",report.ReplaceNewlines("aa\r\nbb"));
            Assert.AreEqual("aa<br/>bb",report.ReplaceNewlines("aa\nbb"));

        }

        [Test]
        public void TestNewlineSubstitution_FullTemplate()
        {
            var cata = WhenIHaveA<Catalogue>();
            cata.Name = "ffff";
            cata.Description = @"A cool
dataset with interesting stuff";
            cata.SaveToDatabase();

            var cataItem1 = new CatalogueItem(RepositoryLocator.CatalogueRepository, cata, "Col1");
            cataItem1.Description = "some info about column 1";
            cataItem1.SaveToDatabase();

            var cataItem2 = new CatalogueItem(RepositoryLocator.CatalogueRepository, cata, "Col2");
            cataItem2.Description = @"some info 
about column 2";
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

            var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] {cata}, outDir, template, "$Name.md", false,"<br/>");
            cmd.Execute();

            var outFile = Path.Combine(outDir.FullName, "ffff.md");

            FileAssert.Exists(outFile);
            var resultText = File.ReadAllText(outFile);

            StringAssert.AreEqualIgnoringCase(@"## ffff
A cool<br/>dataset with interesting stuff
| Column | Description |
| Col1 | some info about column 1 |
| Col2 | some info <br/>about column 2 |",resultText.TrimEnd());
        }
    #region Loop Catalogues tests
        
        [Test]
        public void TestCustomMetadataReport_LoopCataloguesPrefix()
        {
            Setup2Catalogues(out Catalogue c1,out Catalogue c2);

            var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
            var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

            if(outDir.Exists)
                outDir.Delete(true);
            
            outDir.Create();

            File.WriteAllText(template.FullName,
                @"# Welcome

We love data here, see our datasets:

$foreach Catalogue
## Catalogue '$Name'
$Description
Price: $30
| Column | Description |
$foreach CatalogueItem
| $Name | $Description |
$end
$end");

            var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] {c1,c2}, outDir, template, "Datasets.md", true,null);
            cmd.Execute();

            var outFile = Path.Combine(outDir.FullName, "Datasets.md");

            FileAssert.Exists(outFile);
            var resultText = File.ReadAllText(outFile);

            StringAssert.AreEqualIgnoringCase(@"# Welcome

We love data here, see our datasets:

## Catalogue 'Demog'
This is expensive dataset: $30 to use
Price: $30
| Column | Description |
| Name | Name of the patient |
| Address | Where they live |
| Postcode | Patients postcode |
## Catalogue 'ffff'
A cool dataset with interesting stuff
Price: $30
| Column | Description |
| Col1 | some info about column 1 |
| Col2 | some info about column 2 |",resultText.TrimEnd());
        }
        
        [Test]
        public void TestCustomMetadataReport_LoopCataloguesSuffix()
        {
            Setup2Catalogues(out Catalogue c1,out Catalogue c2);

            var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
            var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

            if(outDir.Exists)
                outDir.Delete(true);
            
            outDir.Create();

            File.WriteAllText(template.FullName,
                @"$foreach Catalogue
## Catalogue '$Name'
$Description
Price: $30
| Column | Description |
$foreach CatalogueItem
| $Name | $Description |
$end
$end

Get in touch with us at noreply@nobody.com");

            var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] {c1,c2}, outDir, template, "Datasets.md", true,null);
            cmd.Execute();

            var outFile = Path.Combine(outDir.FullName, "Datasets.md");

            FileAssert.Exists(outFile);
            var resultText = File.ReadAllText(outFile);

            StringAssert.AreEqualIgnoringCase(@"## Catalogue 'Demog'
This is expensive dataset: $30 to use
Price: $30
| Column | Description |
| Name | Name of the patient |
| Address | Where they live |
| Postcode | Patients postcode |
## Catalogue 'ffff'
A cool dataset with interesting stuff
Price: $30
| Column | Description |
| Col1 | some info about column 1 |
| Col2 | some info about column 2 |

Get in touch with us at noreply@nobody.com",resultText.TrimEnd());
        }
        [Test]
        public void TestCustomMetadataReport_LoopCataloguesPrefixAndSuffix()
        {
            Setup2Catalogues(out Catalogue c1,out Catalogue c2);

            var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
            var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

            if(outDir.Exists)
                outDir.Delete(true);
            
            outDir.Create();

            File.WriteAllText(template.FullName,
                @"# Welcome

We love data here, see our datasets:

$foreach Catalogue
## Catalogue '$Name'
$Description
Price: $30
| Column | Description |
$foreach CatalogueItem
| $Name | $Description |
$end
$end

Get in touch with us at noreply@nobody.com");

            var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] {c1,c2}, outDir, template, "Datasets.md", true,null);
            cmd.Execute();

            var outFile = Path.Combine(outDir.FullName, "Datasets.md");

            FileAssert.Exists(outFile);
            var resultText = File.ReadAllText(outFile);

            StringAssert.AreEqualIgnoringCase(@"# Welcome

We love data here, see our datasets:

## Catalogue 'Demog'
This is expensive dataset: $30 to use
Price: $30
| Column | Description |
| Name | Name of the patient |
| Address | Where they live |
| Postcode | Patients postcode |
## Catalogue 'ffff'
A cool dataset with interesting stuff
Price: $30
| Column | Description |
| Col1 | some info about column 1 |
| Col2 | some info about column 2 |

Get in touch with us at noreply@nobody.com",resultText.TrimEnd());
        }


        [Test]
        public void TestCustomMetadataReport_LoopCataloguesTableOfContents()
        {
            Setup2Catalogues(out Catalogue c1,out Catalogue c2);

            var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
            var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

            if(outDir.Exists)
                outDir.Delete(true);
            
            outDir.Create();

            File.WriteAllText(template.FullName,
                @"# Welcome
- [Background](#background)
- [Datasets](#datasets)
$foreach Catalogue
   - [$Name](#$Name)
$end

# Background

Boy do we love our datasets here!

# Datasets

$foreach Catalogue
## $Name
$Description
Price: $30
| Column | Description |
$foreach CatalogueItem
| $Name | $Description |
$end
$end

Get in touch with us at noreply@nobody.com");

            var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] {c1,c2}, outDir, template, "Datasets.md", true,null);
            cmd.Execute();

            var outFile = Path.Combine(outDir.FullName, "Datasets.md");

            FileAssert.Exists(outFile);
            var resultText = File.ReadAllText(outFile);

            StringAssert.AreEqualIgnoringCase(@"# Welcome
- [Background](#background)
- [Datasets](#datasets)
   - [Demog](#Demog)
   - [ffff](#ffff)

# Background

Boy do we love our datasets here!

# Datasets

## Demog
This is expensive dataset: $30 to use
Price: $30
| Column | Description |
| Name | Name of the patient |
| Address | Where they live |
| Postcode | Patients postcode |
## ffff
A cool dataset with interesting stuff
Price: $30
| Column | Description |
| Col1 | some info about column 1 |
| Col2 | some info about column 2 |

Get in touch with us at noreply@nobody.com",resultText.TrimEnd());
        }
                
        [Test]
        public void TestCustomMetadataReport_ErrorCondition_ExtraStartBlock()
        {
            Setup2Catalogues(out Catalogue c1,out Catalogue c2);

            var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
            var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

            if(outDir.Exists)
                outDir.Delete(true);
            
            outDir.Create();

            File.WriteAllText(template.FullName,
                @"# Welcome
- Datasets
$foreach Catalogue
$foreach Catalogue

some more text
");

            var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] {c1,c2}, outDir, template, "Datasets.md", true,null);
            var ex = Assert.Throws<CustomMetadataReportException>(()=>cmd.Execute());

            Assert.AreEqual("Unexpected '$foreach Catalogue' before the end of the last one on line 4",ex.Message);
            Assert.AreEqual(4,ex.LineNumber);
        } 
        [Test]
        public void TestCustomMetadataReport_ErrorCondition_UnexpectedEndBlock()
        {
            Setup2Catalogues(out Catalogue c1,out Catalogue c2);

            var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
            var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

            if(outDir.Exists)
                outDir.Delete(true);
            
            outDir.Create();

            File.WriteAllText(template.FullName,
                @"# Welcome
- Datasets
$end
$foreach Catalogue

some more text
");

            var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] {c1,c2}, outDir, template, "Datasets.md", true,null);
            var ex = Assert.Throws<CustomMetadataReportException>(()=>cmd.Execute());

            Assert.AreEqual("Error, encountered '$end' on line 3 while not in a $foreach Catalogue block",ex.Message);
            Assert.AreEqual(3,ex.LineNumber);
        }

        
        [Test]
        public void TestCustomMetadataReport_ErrorCondition_TooManyEndBlocks()
        {
            Setup2Catalogues(out Catalogue c1,out Catalogue c2);

            var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
            var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

            if(outDir.Exists)
                outDir.Delete(true);
            
            outDir.Create();

            File.WriteAllText(template.FullName,
                @"# Welcome
- Datasets
$foreach Catalogue
$end
$end

some more text
");

            var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] {c1,c2}, outDir, template, "Datasets.md", true,null);
            var ex = Assert.Throws<CustomMetadataReportException>(()=>cmd.Execute());

            Assert.AreEqual("Error, encountered '$end' on line 5 while not in a $foreach Catalogue block",ex.Message);
            Assert.AreEqual(5,ex.LineNumber);
        }
        [Test]
        public void TestCustomMetadataReport_ErrorCondition_MixingTopLevelBlocks()
        {
            Setup2Catalogues(out Catalogue c1,out Catalogue c2);

            var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
            var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

            if(outDir.Exists)
                outDir.Delete(true);
            
            outDir.Create();

            File.WriteAllText(template.FullName,
                @"# Welcome
- Datasets
$foreach CatalogueItem
$end
$foreach Catalogue
$end

some more text
");

            var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] {c1,c2}, outDir, template, "Datasets.md", true,null);
            var ex = Assert.Throws<CustomMetadataReportException>(()=>cmd.Execute());
            
            Assert.AreEqual("Error, Unexpected '$foreach CatalogueItem' on line 3.  Current section is plain text, '$foreach CatalogueItem' can only appear within a '$foreach Catalogue' block (you cannot mix and match top level loop elements)",ex.Message);
            Assert.AreEqual(3,ex.LineNumber);
        }
        #endregion
    }

}
