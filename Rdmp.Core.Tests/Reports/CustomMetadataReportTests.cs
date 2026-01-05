// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Globalization;
using System.IO;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataQualityEngine.Data;
using Rdmp.Core.Reports;
using Tests.Common;

namespace Rdmp.Core.Tests.Reports;

internal class CustomMetadataReportTests : UnitTests
{
    [OneTimeSetUp]
    public void Init()
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-GB");
    }

    private void Setup2Catalogues(out Catalogue c1, out Catalogue c2)
    {
        c1 = WhenIHaveA<Catalogue>();
        c1.Name = "ffff";
        c1.Description = "A cool dataset with interesting stuff";
        c1.SaveToDatabase();

        var c1ci1 = new CatalogueItem(RepositoryLocator.CatalogueRepository, c1, "Col1")
        {
            Description = "some info about column 1"
        };
        c1ci1.SaveToDatabase();

        var c1ci2 = new CatalogueItem(RepositoryLocator.CatalogueRepository, c1, "Col2")
        {
            Description = "some info about column 2"
        };
        c1ci2.SaveToDatabase();

        c2 = WhenIHaveA<Catalogue>();
        c2.Name = "Demog";
        c2.Description = "This is expensive dataset: $30 to use";
        c2.SaveToDatabase();

        var c2ci1 = new CatalogueItem(RepositoryLocator.CatalogueRepository, c2, "Name")
        {
            Description = "Name of the patient"
        };
        c2ci1.SaveToDatabase();
        var c2ci2 = new CatalogueItem(RepositoryLocator.CatalogueRepository, c2, "Address")
        {
            Description = "Where they live"
        };
        c2ci2.SaveToDatabase();
        var c2ci3 = new CatalogueItem(RepositoryLocator.CatalogueRepository, c2, "Postcode")
        {
            Description = "Patients postcode"
        };
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

        if (outDir.Exists)
            outDir.Delete(true);

        outDir.Create();

        File.WriteAllText(template.FullName,
            @"| Name | Desc|
| $Name | $Description |");

        var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] { cata },
            outDir, template, "$Name.md", oneFile, null);
        cmd.Execute();

        var outFile = Path.Combine(outDir.FullName, "ffff.md");

        FileAssert.Exists(outFile);
        var resultText = File.ReadAllText(outFile);

        Assert.That(resultText.TrimEnd(), Is.EqualTo(@"| Name | Desc|
| ffff |  |").IgnoreCase);
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

        if (outDir.Exists)
            outDir.Delete(true);

        outDir.Create();

        File.WriteAllText(template.FullName,
            @"| Name | Desc| Range |
| $Name | $Description | $DQE_StartDate$DQE_EndDate$DQE_DateRange |");

        var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] { cata },
            outDir, template, "$Name.md", false, null);
        cmd.Execute();

        var outFile = Path.Combine(outDir.FullName, "ffff.md");

        FileAssert.Exists(outFile);
        var resultText = File.ReadAllText(outFile);

        Assert.That(resultText.TrimEnd(), Is.EqualTo(@"| Name | Desc| Range |
| ffff |  | Unknown |").IgnoreCase);
    }

    [Test]
    public void TestCustomMetadataReport_SingleCatalogue_DQEMetrics()
    {
        var cata = WhenIHaveA<Catalogue>();
        cata.Name = "ffff";
        cata.Description = null;
        cata.SaveToDatabase();

        var ei = WhenIHaveA<ExtractionInformation>();
        ei.SelectSQL = "[blah]..[mydate]";
        ei.SaveToDatabase();

        cata.TimeCoverage_ExtractionInformation_ID = ei.ID;
        cata.SaveToDatabase();

        var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
        var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

        if (outDir.Exists)
            outDir.Delete(true);

        outDir.Create();

        File.WriteAllText(template.FullName,
            @"| Name | Desc| StartYear | EndYear | StartMonth | EndMonth | StartDay | EndDay | Range | TimeField |
| $Name | $Description | $DQE_StartYear | $DQE_EndYear | $DQE_StartMonth | $DQE_EndMonth | $DQE_StartDay | $DQE_EndDay | $DQE_StartYear-$DQE_EndYear | $TimeCoverage_ExtractionInformation |");


        var reporter = new CustomMetadataReport(RepositoryLocator)
        {
            NewlineSubstitution = null
        };

        var moqDqe = Substitute.For<IDetermineDatasetTimespan>();
        moqDqe.GetMachineReadableTimespanIfKnownOf(cata, true, out _)
            .Returns(new Tuple<DateTime?, DateTime?>(new DateTime(2001, 02, 01), new DateTime(2002, 04, 03)));

        reporter.TimespanCalculator = moqDqe;

        reporter.GenerateReport(new[] { cata }, outDir, template, "$Name.md", false);

        var outFile = Path.Combine(outDir.FullName, "ffff.md");

        FileAssert.Exists(outFile);
        var resultText = File.ReadAllText(outFile);

        Assert.That(
resultText.TrimEnd(), Is.EqualTo(@"| Name | Desc| StartYear | EndYear | StartMonth | EndMonth | StartDay | EndDay | Range | TimeField |
| ffff |  | 2001 | 2002 | 02 | 04 | 01 | 03 | 2001-2002 | mydate |").IgnoreCase);
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

        if (outDir.Exists)
            outDir.Delete(true);

        outDir.Create();

        File.WriteAllText(template.FullName,
            @"<DataSet>
<Name>$Name</Name>
<Desc>$Description</Desc>
<Email>$Administrative_contact_email</Email>
</DataSet>");


        var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator),
            new[] { cata, cata2 }, outDir, template,
            oneFile ? "results.xml" : "$Name.xml", oneFile, null);
        cmd.Execute();

        if (oneFile)
        {
            var outFile = Path.Combine(outDir.FullName, "results.xml");

            FileAssert.Exists(outFile);
            var resultText = File.ReadAllText(outFile);

            Assert.That(
resultText.TrimEnd(), Is.EqualTo(@"<DataSet>
<Name>Forest</Name>
<Desc></Desc>
<Email>me@g.com</Email>
</DataSet>
<DataSet>
<Name>Trees</Name>
<Desc>trollolol</Desc>
<Email></Email>
</DataSet>").IgnoreCase);
        }
        else
        {
            var outFile1 = Path.Combine(outDir.FullName, "Forest.xml");
            var outFile2 = Path.Combine(outDir.FullName, "Trees.xml");

            FileAssert.Exists(outFile1);
            FileAssert.Exists(outFile2);

            var resultText1 = File.ReadAllText(outFile1);
            Assert.That(
resultText1.Trim(), Is.EqualTo(@"<DataSet>
<Name>Forest</Name>
<Desc></Desc>
<Email>me@g.com</Email>
</DataSet>".Trim()).IgnoreCase);

            var resultText2 = File.ReadAllText(outFile2);

            Assert.That(
resultText2.Trim(), Is.EqualTo(@"<DataSet>
<Name>Trees</Name>
<Desc>trollolol</Desc>
<Email></Email>
</DataSet>".Trim()).IgnoreCase);
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

        var cataItem1 = new CatalogueItem(RepositoryLocator.CatalogueRepository, cata, "Col1")
        {
            Description = "some info about column 1"
        };
        cataItem1.SaveToDatabase();

        var cataItem2 = new CatalogueItem(RepositoryLocator.CatalogueRepository, cata, "Col2")
        {
            Description = "some info about column 2"
        };
        cataItem2.SaveToDatabase();

        var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
        var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

        if (outDir.Exists)
            outDir.Delete(true);

        outDir.Create();

        File.WriteAllText(template.FullName,
            @"## $Name
$Description
| Column | Description |
$foreach CatalogueItem
| $Name | $Description |
$end");


        var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] { cata },
            outDir, template, "$Name.md", oneFile, null);
        cmd.Execute();

        var outFile = Path.Combine(outDir.FullName, "ffff.md");

        FileAssert.Exists(outFile);
        var resultText = File.ReadAllText(outFile);

        Assert.That(resultText.TrimEnd(), Is.EqualTo(@"## ffff
A cool dataset with interesting stuff
| Column | Description |
| Col1 | some info about column 1 |
| Col2 | some info about column 2 |").IgnoreCase);
    }

    [Test]
    public void TestCustomMetadataReport_TwoCataloguesWithItems()
    {
        Setup2Catalogues(out var c1, out var c2);

        var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
        var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

        if (outDir.Exists)
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

        var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] { c1, c2 },
            outDir, template, "Datasets.md", true, null);
        cmd.Execute();

        var outFile = Path.Combine(outDir.FullName, "Datasets.md");

        FileAssert.Exists(outFile);
        var resultText = File.ReadAllText(outFile);

        Assert.That(resultText.TrimEnd(), Is.EqualTo(@"## Demog
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
| Col2 | some info about column 2 |").IgnoreCase);
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

        if (outDir.Exists)
            outDir.Delete(true);

        outDir.Create();

        File.WriteAllText(template.FullName,
            @"## $Name
$Description
| Column | Description |
$foreach CatalogueItem
| $Name | $Description |");

        var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] { cata },
            outDir, template, "$Name.md", false, null);
        var ex = Assert.Throws<CustomMetadataReportException>(cmd.Execute);

        Assert.Multiple(() =>
        {
            Assert.That(ex.LineNumber, Is.EqualTo(4));
            Assert.That(ex.Message, Is.EqualTo("Expected $end to match $foreach which started on line 4"));
        });
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

        if (outDir.Exists)
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

        var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] { cata },
            outDir, template, "$Name.md", false, null);
        var ex = Assert.Throws<CustomMetadataReportException>(cmd.Execute);

        Assert.Multiple(() =>
        {
            Assert.That(ex.LineNumber, Is.EqualTo(6));
            Assert.That(ex.Message, Does.StartWith("Error, encountered '$foreach CatalogueItem' on line 6"));
        });
    }

    [Test]
    public void TestNewlineSubstitution()
    {
        var report = new CustomMetadataReport(RepositoryLocator);

        Assert.Multiple(() =>
        {
            //default is no substitution
            Assert.That(report.NewlineSubstitution, Is.Null);

            Assert.That(report.ReplaceNewlines(null), Is.Null);

            Assert.That(report.ReplaceNewlines("aa\r\nbb"), Is.EqualTo("aa\r\nbb"));
            Assert.That(report.ReplaceNewlines("aa\nbb"), Is.EqualTo("aa\nbb"));
        });

        report.NewlineSubstitution = "<br/>";

        Assert.Multiple(() =>
        {
            Assert.That(report.ReplaceNewlines("aa\r\nbb"), Is.EqualTo("aa<br/>bb"));
            Assert.That(report.ReplaceNewlines("aa\nbb"), Is.EqualTo("aa<br/>bb"));
        });
    }

    [Test]
    public void TestNewlineSubstitution_FullTemplate()
    {
        var cata = WhenIHaveA<Catalogue>();
        cata.Name = "ffff";
        cata.Description = @"A cool
dataset with interesting stuff";
        cata.SaveToDatabase();

        var cataItem1 = new CatalogueItem(RepositoryLocator.CatalogueRepository, cata, "Col1")
        {
            Description = "some info about column 1"
        };
        cataItem1.SaveToDatabase();

        var cataItem2 = new CatalogueItem(RepositoryLocator.CatalogueRepository, cata, "Col2")
        {
            Description = @"some info
about column 2"
        };
        cataItem2.SaveToDatabase();

        var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
        var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

        if (outDir.Exists)
            outDir.Delete(true);

        outDir.Create();

        File.WriteAllText(template.FullName,
            @"## $Name
$Description
| Column | Description |
$foreach CatalogueItem
| $Name | $Description |
$end");

        var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] { cata },
            outDir, template, "$Name.md", false, "<br/>");
        cmd.Execute();

        var outFile = Path.Combine(outDir.FullName, "ffff.md");

        FileAssert.Exists(outFile);
        var resultText = File.ReadAllText(outFile);

        Assert.That(resultText.TrimEnd(), Is.EqualTo(@"## ffff
A cool<br/>dataset with interesting stuff
| Column | Description |
| Col1 | some info about column 1 |
| Col2 | some info<br/>about column 2 |").IgnoreCase);
    }


    [Test]
    public void TestTableInfoProperties_NoTableInfo()
    {
        var cata = WhenIHaveA<Catalogue>();
        cata.Name = "ffff";
        cata.Description = "A cool dataset with interesting stuff";
        cata.SaveToDatabase();

        var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
        var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

        if (outDir.Exists)
            outDir.Delete(true);

        outDir.Create();

        File.WriteAllText(template.FullName,
            @"## $Name
Server: $Server
Description: $Description");

        var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] { cata },
            outDir, template, "$Name.md", false, null);
        cmd.Execute();

        var outFile = Path.Combine(outDir.FullName, "ffff.md");

        FileAssert.Exists(outFile);
        var resultText = File.ReadAllText(outFile);

        Assert.That(resultText, Is.EqualTo(@"## ffff
Server:
Description: A cool dataset with interesting stuff").IgnoreCase);
    }


    [Test]
    public void TestTableInfoProperties_OneTableInfo()
    {
        var ei = WhenIHaveA<ExtractionInformation>();
        var cata = ei.CatalogueItem.Catalogue;
        cata.Name = "ffff";
        cata.Description = "A cool dataset with interesting stuff";
        cata.SaveToDatabase();

        ei.ColumnInfo.TableInfo.Server = "myserver";
        ei.ColumnInfo.TableInfo.SaveToDatabase();


        var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
        var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

        if (outDir.Exists)
            outDir.Delete(true);

        outDir.Create();

        File.WriteAllText(template.FullName,
            @"## $Name
Server: $Server
Description: $Description");

        var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] { cata },
            outDir, template, "$Name.md", false, null);
        cmd.Execute();

        var outFile = Path.Combine(outDir.FullName, "ffff.md");

        FileAssert.Exists(outFile);
        var resultText = File.ReadAllText(outFile);

        Assert.That(resultText, Is.EqualTo(@"## ffff
Server: myserver
Description: A cool dataset with interesting stuff").IgnoreCase);
    }

    [Test]
    public void TestCustomMetadataReport_LoopCataloguesPrefix()
    {
        Setup2Catalogues(out var c1, out var c2);

        var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
        var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

        if (outDir.Exists)
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

        var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] { c1, c2 },
            outDir, template, "Datasets.md", true, null);
        cmd.Execute();

        var outFile = Path.Combine(outDir.FullName, "Datasets.md");

        FileAssert.Exists(outFile);
        var resultText = File.ReadAllText(outFile);

        Assert.That(resultText.TrimEnd(), Is.EqualTo(@"# Welcome

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
| Col2 | some info about column 2 |").IgnoreCase);
    }


    [Test]
    public void TestCustomMetadataReport_ColumnInfoDatatype()
    {
        var ei1 = WhenIHaveA<ExtractionInformation>();
        var ei2 = WhenIHaveA<ExtractionInformation>();

        ei1.CatalogueItem.Catalogue.Name = "Cata1";
        ei2.CatalogueItem.Catalogue.Name = "Cata2";

        ei1.CatalogueItem.ColumnInfo.Data_type = "varchar(10)";
        ei2.CatalogueItem.ColumnInfo.Data_type = "datetime2";

        var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
        var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

        if (outDir.Exists)
            outDir.Delete(true);

        outDir.Create();

        File.WriteAllText(template.FullName,
            @"# Welcome

We love data here, see our datasets:

$foreach Catalogue
## Catalogue '$Name'
$Description
Price: $30
| Column | Description | Datatype |
$foreach CatalogueItem
| $Name | $Description | $Data_type |
$end
$end");

        var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator),
            new[] { ei1.CatalogueItem.Catalogue, ei2.CatalogueItem.Catalogue }, outDir, template, "Datasets.md", true,
            null);
        cmd.Execute();

        var outFile = Path.Combine(outDir.FullName, "Datasets.md");

        FileAssert.Exists(outFile);
        var resultText = File.ReadAllText(outFile);

        Assert.That(resultText.TrimEnd(), Is.EqualTo(@"# Welcome

We love data here, see our datasets:

## Catalogue 'Cata1'

Price: $30
| Column | Description | Datatype |
| MyCataItem |  | varchar(10) |
## Catalogue 'Cata2'

Price: $30
| Column | Description | Datatype |
| MyCataItem |  | datetime2 |").IgnoreCase);
    }


    [Test]
    public void TestCustomMetadataReport_Nullability_NoDQERun()
    {
        var ei1 = WhenIHaveA<ExtractionInformation>();
        var ei2 = WhenIHaveA<ExtractionInformation>();

        ei1.CatalogueItem.Catalogue.Name = "Cata1";
        ei2.CatalogueItem.Catalogue.Name = "Cata2";

        ei1.CatalogueItem.Name = "Cata1Col1";
        ei2.CatalogueItem.Name = "Cata2Col1";

        var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
        var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

        if (outDir.Exists)
            outDir.Delete(true);

        outDir.Create();

        File.WriteAllText(template.FullName,
            @"# Welcome

We love data here, see our datasets:

$foreach Catalogue
## Catalogue '$Name'
$Description
Price: $30
Number of Records: $DQE_CountTotal
| Column | Description | Nullability |
$foreach CatalogueItem
| $Name | $Description | $DQE_PercentNull |
$end
$end");

        var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator),
            new[] { ei1.CatalogueItem.Catalogue, ei2.CatalogueItem.Catalogue }, outDir, template, "Datasets.md", true,
            null);
        cmd.Execute();

        var outFile = Path.Combine(outDir.FullName, "Datasets.md");

        FileAssert.Exists(outFile);
        var resultText = File.ReadAllText(outFile);

        Assert.That(resultText.TrimEnd(), Is.EqualTo(@"# Welcome

We love data here, see our datasets:

## Catalogue 'Cata1'

Price: $30
Number of Records:
| Column | Description | Nullability |
| Cata1Col1 |  |  |
## Catalogue 'Cata2'

Price: $30
Number of Records:
| Column | Description | Nullability |
| Cata2Col1 |  |  |").IgnoreCase);
    }


    [Test]
    public void TestCustomMetadataReport_Nullability_WithDQERun()
    {
        var ei1 = WhenIHaveA<ExtractionInformation>();
        var ei2 = WhenIHaveA<ExtractionInformation>();

        ei1.CatalogueItem.Catalogue.Name = "Cata1";
        ei2.CatalogueItem.Catalogue.Name = "Cata2";

        ei1.CatalogueItem.Name = "Cata1Col1";
        ei2.CatalogueItem.Name = "Cata2Col1";

        var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
        var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

        if (outDir.Exists)
            outDir.Delete(true);

        outDir.Create();

        File.WriteAllText(template.FullName,
            @"# Welcome

We love data here, see our datasets:

$foreach Catalogue
## Catalogue '$Name'
$Description
Price: $30
Number of Records: $DQE_CountTotal
| Column | Description | Nullability | Correct | Missing | Wrong | Invalid | Null (inclusive) | Total |
$foreach CatalogueItem
| $Name | $Description | $DQE_PercentNull | $DQE_CountCorrect | $DQE_CountMissing | $DQE_CountWrong | $DQE_CountInvalidatesRow | $DQE_CountDBNull | $DQE_CountTotal |
$end
Accurate as of : $DQE_DateOfEvaluation
$end");
        var cata1 = ei1.CatalogueItem.Catalogue;
        var cata2 = ei2.CatalogueItem.Catalogue;


        var reporter = new CustomMetadataReport(RepositoryLocator);

        var eval1 = Substitute.For<Evaluation>();

        var eval1_col1 = Substitute.For<ColumnState>();
        eval1_col1.TargetProperty = "Cata1Col1";
        eval1_col1.CountCorrect = 9;
        eval1_col1.CountDBNull =
            3; // note that this is separate from the other counts.  A value can be both null and correct.
        eval1.ColumnStates = new ColumnState[] { eval1_col1 };

        var eval2 = Substitute.For<Evaluation>();
        var eval2_col1 = Substitute.For<ColumnState>();
        eval2_col1.TargetProperty = "Cata2Col1";
        eval2_col1.CountCorrect = 1;
        eval2_col1.CountMissing = 2;
        eval2_col1.CountWrong = 3;
        eval2_col1.CountInvalidatesRow = 4;
        eval2_col1.CountDBNull = 5;
        eval2.ColumnStates = new ColumnState[] { eval2_col1 };

        reporter.EvaluationCache.Add(cata1, eval1);
        reporter.EvaluationCache.Add(cata2, eval2);

        reporter.GenerateReport(new[] { cata1, cata2 }, outDir, template, "Datasets.md", true);

        var outFile = Path.Combine(outDir.FullName, "Datasets.md");

        FileAssert.Exists(outFile);
        var resultText = File.ReadAllText(outFile);

        Assert.That(resultText.TrimEnd(), Is.EqualTo(@"# Welcome

We love data here, see our datasets:

## Catalogue 'Cata1'

Price: $30
Number of Records: 9
| Column | Description | Nullability | Correct | Missing | Wrong | Invalid | Null (inclusive) | Total |
| Cata1Col1 |  | 33% | 9 | 0 | 0 | 0 | 3 | 9 |
Accurate as of : 01/01/0001 00:00:00
## Catalogue 'Cata2'

Price: $30
Number of Records: 10
| Column | Description | Nullability | Correct | Missing | Wrong | Invalid | Null (inclusive) | Total |
| Cata2Col1 |  | 50% | 1 | 2 | 3 | 4 | 5 | 10 |
Accurate as of : 01/01/0001 00:00:00").IgnoreCase);
    }


    [Test]
    public void TestCustomMetadataReport_LoopCataloguesSuffix()
    {
        Setup2Catalogues(out var c1, out var c2);

        var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
        var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

        if (outDir.Exists)
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

        var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] { c1, c2 },
            outDir, template, "Datasets.md", true, null);
        cmd.Execute();

        var outFile = Path.Combine(outDir.FullName, "Datasets.md");

        FileAssert.Exists(outFile);
        var resultText = File.ReadAllText(outFile);

        Assert.That(resultText.TrimEnd(), Is.EqualTo(@"## Catalogue 'Demog'
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

Get in touch with us at noreply@nobody.com").IgnoreCase);
    }

    [Test]
    public void TestCustomMetadataReport_LoopCataloguesPrefixAndSuffix()
    {
        Setup2Catalogues(out var c1, out var c2);

        var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
        var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

        if (outDir.Exists)
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

        var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] { c1, c2 },
            outDir, template, "Datasets.md", true, null);
        cmd.Execute();

        var outFile = Path.Combine(outDir.FullName, "Datasets.md");

        FileAssert.Exists(outFile);
        var resultText = File.ReadAllText(outFile);

        Assert.That(resultText.TrimEnd(), Is.EqualTo(@"# Welcome

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

Get in touch with us at noreply@nobody.com").IgnoreCase);
    }


    [Test]
    public void TestCustomMetadataReport_LoopCataloguesTableOfContents()
    {
        Setup2Catalogues(out var c1, out var c2);

        var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
        var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

        if (outDir.Exists)
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

        var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] { c1, c2 },
            outDir, template, "Datasets.md", true, null);
        cmd.Execute();

        var outFile = Path.Combine(outDir.FullName, "Datasets.md");

        FileAssert.Exists(outFile);
        var resultText = File.ReadAllText(outFile);

        Assert.That(resultText.TrimEnd(), Is.EqualTo(@"# Welcome
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

Get in touch with us at noreply@nobody.com").IgnoreCase);
    }

    [Test]
    public void TestCustomMetadataReport_ErrorCondition_ExtraStartBlock()
    {
        Setup2Catalogues(out var c1, out var c2);

        var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
        var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

        if (outDir.Exists)
            outDir.Delete(true);

        outDir.Create();

        File.WriteAllText(template.FullName,
            @"# Welcome
- Datasets
$foreach Catalogue
$foreach Catalogue

some more text
");

        var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] { c1, c2 },
            outDir, template, "Datasets.md", true, null);
        var ex = Assert.Throws<CustomMetadataReportException>(() => cmd.Execute());

        Assert.Multiple(() =>
        {
            Assert.That(ex.Message, Is.EqualTo("Unexpected '$foreach Catalogue' before the end of the last one on line 4"));
            Assert.That(ex.LineNumber, Is.EqualTo(4));
        });
    }

    [Test]
    public void TestCustomMetadataReport_ErrorCondition_UnexpectedEndBlock()
    {
        Setup2Catalogues(out var c1, out var c2);

        var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
        var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

        if (outDir.Exists)
            outDir.Delete(true);

        outDir.Create();

        File.WriteAllText(template.FullName,
            @"# Welcome
- Datasets
$end
$foreach Catalogue

some more text
");

        var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] { c1, c2 },
            outDir, template, "Datasets.md", true, null);
        var ex = Assert.Throws<CustomMetadataReportException>(() => cmd.Execute());

        Assert.Multiple(() =>
        {
            Assert.That(ex.Message, Is.EqualTo("Error, encountered '$end' on line 3 while not in a $foreach Catalogue block"));
            Assert.That(ex.LineNumber, Is.EqualTo(3));
        });
    }


    [Test]
    public void TestCustomMetadataReport_ErrorCondition_TooManyEndBlocks()
    {
        Setup2Catalogues(out var c1, out var c2);

        var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
        var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

        if (outDir.Exists)
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

        var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] { c1, c2 },
            outDir, template, "Datasets.md", true, null);
        var ex = Assert.Throws<CustomMetadataReportException>(() => cmd.Execute());

        Assert.Multiple(() =>
        {
            Assert.That(ex.Message, Is.EqualTo("Error, encountered '$end' on line 5 while not in a $foreach Catalogue block"));
            Assert.That(ex.LineNumber, Is.EqualTo(5));
        });
    }

    [Test]
    public void TestCustomMetadataReport_ErrorCondition_MixingTopLevelBlocks()
    {
        Setup2Catalogues(out var c1, out var c2);

        var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
        var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

        if (outDir.Exists)
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

        var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] { c1, c2 },
            outDir, template, "Datasets.md", true, null);
        var ex = Assert.Throws<CustomMetadataReportException>(() => cmd.Execute());

        Assert.Multiple(() =>
        {
            Assert.That(
                    ex.Message, Is.EqualTo("Error, Unexpected '$foreach CatalogueItem' on line 3.  Current section is plain text, '$foreach CatalogueItem' can only appear within a '$foreach Catalogue' block (you cannot mix and match top level loop elements)"));
            Assert.That(ex.LineNumber, Is.EqualTo(3));
        });
    }

    [Test]
    public void Test_CustomMetadataElementSeperator_ThrowsWhenNotInForEach()
    {
        var templateCode = @"
$Name
$Comma";
        Setup2Catalogues(out var c1, out var c2);

        var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
        var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

        if (outDir.Exists)
            outDir.Delete(true);

        outDir.Create();

        File.WriteAllText(template.FullName, templateCode);

        var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] { c1, c2 },
            outDir, template, "Datasets.md", true, null);
        var ex = Assert.Throws<CustomMetadataReportException>(() => cmd.Execute());

        Assert.That(ex.Message, Is.EqualTo("Unexpected use of $Comma outside of an iteration ($foreach) block"));
    }

    [Test]
    public void Test_CustomMetadataElementSeperator_JsonExample()
    {
        var templateCode = @"[
$foreach Catalogue
  {
    ""Name"": ""$Name"",
    ""Columns"": [
$foreach CatalogueItem
      {
                ""Name"": ""$Name""
      }$Comma
$end
    ]
  }$Comma
$end
]";

        Setup2Catalogues(out var c1, out var c2);

        var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
        var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

        if (outDir.Exists)
            outDir.Delete(true);

        outDir.Create();

        File.WriteAllText(template.FullName, templateCode);

        var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] { c1, c2 },
            outDir, template, "Datasets.md", true, null);
        cmd.Execute();

        var outFile = Path.Combine(outDir.FullName, "Datasets.md");

        FileAssert.Exists(outFile);
        var resultText = File.ReadAllText(outFile);

        Assert.That(resultText.TrimEnd(), Is.EqualTo(@"[
  {
    ""Name"": ""Demog"",
    ""Columns"": [
      {
                ""Name"": ""Name""
      },
      {
                ""Name"": ""Address""
      },
      {
                ""Name"": ""Postcode""
      }
    ]
  },
  {
    ""Name"": ""ffff"",
    ""Columns"": [
      {
                ""Name"": ""Col1""
      },
      {
                ""Name"": ""Col2""
      }
    ]
  }
]").IgnoreCase);
    }

    [Test]
    public void Test_CustomMetadataElementSeperator_JsonExample_SemicolonSub()
    {
        var templateCode = @"[
$foreach Catalogue
  {
    ""Name"": ""$Name"",
    ""Columns"": [
$foreach CatalogueItem
      {
                ""Name"": ""$Name""
      }$Comma
$end
    ]
  }$Comma
$end
]";

        Setup2Catalogues(out var c1, out var c2);

        var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
        var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

        if (outDir.Exists)
            outDir.Delete(true);

        outDir.Create();

        File.WriteAllText(template.FullName, templateCode);

        var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] { c1, c2 },
            outDir, template, "Datasets.md", true, null, ";");
        cmd.Execute();

        var outFile = Path.Combine(outDir.FullName, "Datasets.md");

        FileAssert.Exists(outFile);
        var resultText = File.ReadAllText(outFile);

        Assert.That(resultText.TrimEnd(), Is.EqualTo(@"[
  {
    ""Name"": ""Demog"",
    ""Columns"": [
      {
                ""Name"": ""Name""
      };
      {
                ""Name"": ""Address""
      };
      {
                ""Name"": ""Postcode""
      }
    ]
  };
  {
    ""Name"": ""ffff"",
    ""Columns"": [
      {
                ""Name"": ""Col1""
      };
      {
                ""Name"": ""Col2""
      }
    ]
  }
]").IgnoreCase);
    }

    [Test]
    public void TestAllSubs_Catalogue()
    {
        var templateCode =
            @"$API_access_URL
$Access_options
$Administrative_contact_address
$Administrative_contact_email
$Administrative_contact_name
$Administrative_contact_telephone
$Attribution_citation
$Background_summary
$Browse_URL
$Bulk_Download_URL
$Contact_details
$Country_of_origin
$Data_standards
$DatasetStartDate
$Description
$Detail_Page_URL
$Ethics_approver
$Explicit_consent
$Geographical_coverage
$Granularity
$ID
$IsDeprecated
$IsInternalDataset
$Last_revision_date
$LoggingDataTask
$Name
$Periodicity
$PivotCategory_ExtractionInformation_ID
$Query_tool_URL
$Resource_owner
$Search_keywords
$Source_URL
$Source_of_data_collection
$SubjectNumbers
$Ticket
$Time_coverage
$Type
$Update_freq
$Update_sched
$Database
$DatabaseType
$IsPrimaryExtractionTable
$IsTableValuedFunction
$IsView
$Schema
$Server
$DQE_CountTotal
$DQE_DateOfEvaluation
$DQE_DateRange
$DQE_EndDate
$DQE_EndDay
$DQE_EndMonth
$DQE_EndYear
$DQE_StartDate
$DQE_StartDay
$DQE_StartMonth
$DQE_StartYear";


        Setup2Catalogues(out var c1, out var c2);

        var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
        var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

        if (outDir.Exists)
            outDir.Delete(true);

        outDir.Create();

        File.WriteAllText(template.FullName, templateCode);

        var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] { c1, c2 },
            outDir, template, "Datasets.md", true, null);
        cmd.Execute();

        var outFile = Path.Combine(outDir.FullName, "Datasets.md");

        FileAssert.Exists(outFile);
        var resultText = File.ReadAllText(outFile);

        // this appears in a Catalogue description
        resultText = resultText.Replace("$30", "");

        Assert.That(resultText, Does.Not.Contain('$'), $"Expected all template values to disappear but was {resultText}");
    }


    [Test]
    public void TestAllSubs_CatalogueItem()
    {
        var templateCode =
            @"
$foreach CatalogueItem
$Agg_method
$Comments
$Description
$ID
$Limitations
$Name
$Periodicity
$Research_relevance
$Statistical_cons
$Topic
$Collation
$Data_type
$Digitisation_specs
$Format
$IgnoreInLoads
$IsAutoIncrement
$IsPrimaryKey
$Source
$Status
$DQE_CountCorrect
$DQE_CountDBNull
$DQE_CountInvalidatesRow
$DQE_CountMissing
$DQE_CountTotal
$DQE_CountWrong
$DQE_PercentNull
$end";


        Setup2Catalogues(out var c1, out var c2);

        var template = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "template.md"));
        var outDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "outDir"));

        if (outDir.Exists)
            outDir.Delete(true);

        outDir.Create();

        File.WriteAllText(template.FullName, templateCode);

        var cmd = new ExecuteCommandExtractMetadata(new ThrowImmediatelyActivator(RepositoryLocator), new[] { c1, c2 },
            outDir, template, "Datasets.md", true, null);
        cmd.Execute();

        var outFile = Path.Combine(outDir.FullName, "Datasets.md");

        FileAssert.Exists(outFile);
        var resultText = File.ReadAllText(outFile);

        Assert.That(resultText, Does.Not.Contain('$'), $"Expected all template values to disappear but was {resultText}");
    }
}