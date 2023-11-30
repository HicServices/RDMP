// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Reports;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration;

internal class DitaExtractorTests : DatabaseTests
{
    private Exception _setupException;

    private TestDirectoryHelper _directoryHelper;

    [OneTimeSetUp]
    protected override void OneTimeSetUp()
    {
        base.OneTimeSetUp();

        try
        {
            _directoryHelper = new TestDirectoryHelper(GetType());

            _directoryHelper.SetUp();

            var random = new Random();

            //delete all catalogues with duplicate names
            var catalogues = CatalogueRepository.GetAllObjects<Catalogue>().ToArray();

            foreach (var cata in catalogues.GroupBy(c => c.Name).Where(g => g.Count() > 1).SelectMany(y => y))
                cata.DeleteInDatabase();

            //make sure all Catalogues have acronyms, if they don't then assign them a super random one
            foreach (var cata in CatalogueRepository.GetAllObjects<Catalogue>()
                         .Where(c => string.IsNullOrWhiteSpace(c.Acronym)))
            {
                cata.Acronym = $"RANDOMACRONYM_{random.Next(10000)}";
                cata.SaveToDatabase();
            }
        }
        catch (Exception e)
        {
            _setupException = e;
        }
    }

    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();
        if (_setupException != null)
        {
            Console.WriteLine("TestFixtureSetUp failed in {0} - {1}", GetType(), _setupException.Message);
            throw _setupException;
        }

        _directoryHelper.DeleteAllEntriesInDir();
    }

    [Test]
    public void DitaExtractorConstructor_ExtractTestCatalogue_FilesExist()
    {
        var testDir = _directoryHelper.Directory;

        //get rid of any old copies lying around
        var oldCatalogueVersion = CatalogueRepository.GetAllObjects<Catalogue>()
            .SingleOrDefault(c => c.Name.Equals("DitaExtractorConstructor_ExtractTestCatalogue_FilesExist"));
        oldCatalogueVersion?.DeleteInDatabase();

        var ditaTestCatalogue =
            new Catalogue(CatalogueRepository, "DitaExtractorConstructor_ExtractTestCatalogue_FilesExist")
            {
                Acronym = "DITA_TEST",
                Description =
                    $"Test catalogue for the unit test DitaExtractorConstructor_ExtractTestCatalogue_FilesExist in file {typeof(DitaExtractorTests).FullName}.cs"
            }; //name of Catalogue

        ditaTestCatalogue.SaveToDatabase();


        try
        {
            var extractor = new DitaCatalogueExtractor(CatalogueRepository, testDir);

            extractor.Extract(ThrowImmediatelyDataLoadEventListener.Quiet);

            Assert.Multiple(() =>
            {
                //make sure the root mapping files exist for navigating around
                Assert.That(File.Exists(Path.Combine(testDir.FullName, "hic_data_catalogue.ditamap")));
                Assert.That(File.Exists(Path.Combine(testDir.FullName, "introduction.dita")));
                Assert.That(File.Exists(Path.Combine(testDir.FullName, "dataset.dita")));
            });

            //make sure the catalogue we created is there
            var ditaCatalogueAsDotDitaFile = new FileInfo(Path.Combine(testDir.FullName,
                "ditaextractorconstructor_extracttestcatalogue_filesexist.dita")); //name of Dita file (for the Catalogue we just created)
            Assert.Multiple(() =>
            {
                Assert.That(ditaCatalogueAsDotDitaFile.Exists);
                Assert.That(File.ReadAllText(ditaCatalogueAsDotDitaFile.FullName)
    , Does.Contain(ditaTestCatalogue.Description));
            });
        }
        finally
        {
            ditaTestCatalogue.DeleteInDatabase();
            foreach (var file in testDir.GetFiles())
                file.Delete();
        }
    }

    [Test]
    public void CreateCatalogueWithNoAcronym_CrashesDITAExtractor()
    {
        var testDir = _directoryHelper.Directory;

        try
        {
            //create a new Catalogue in the test datbaase that doesnt have a acronym (should crash Dita Extractor)
            var myNewCatalogue = new Catalogue(CatalogueRepository, "UnitTestCatalogue")
            {
                Acronym = ""
            };
            myNewCatalogue.SaveToDatabase();

            try
            {
                var extractor = new DitaCatalogueExtractor(CatalogueRepository, testDir);
                var ex = Assert.Throws<Exception>(() => extractor.Extract(ThrowImmediatelyDataLoadEventListener.Quiet));
                Assert.That(
                    ex.Message, Is.EqualTo("Dita Extraction requires that each catalogue have a unique Acronym, the catalogue UnitTestCatalogue is missing an Acronym"));
            }
            finally
            {
                myNewCatalogue.DeleteInDatabase();
            }
        }
        finally
        {
            foreach (var file in testDir.GetFiles())
                file.Delete();
        }
    }
}