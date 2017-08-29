using System;
using System.Linq;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data;
using DataQualityEngine.Data;
using DataQualityEngine.Database;
using Diagnostics.TestData;
using HIC.Common.Validation;
using HIC.Common.Validation.Constraints.Secondary;
using HIC.Common.Validation.Dependency;
using HIC.Common.Validation.Dependency.Exceptions;
using MapsDirectlyToDatabaseTable.Versioning;
using NUnit.Framework;
using RDMPStartup;
using ReusableLibraryCode.Checks;
using Tests.Common;

namespace CatalogueLibraryTests.Integration.ObscureDependencyTests
{
    public class ValidationXMLObscureDependencyFinderTests: DatabaseTests
    {
        [Test]
        public void TestGettingTheUsualSuspects()
        {
            ValidationXMLObscureDependencyFinder finder = new ValidationXMLObscureDependencyFinder( RepositoryLocator);
            
            //forces call to initialize
            finder.ThrowIfDeleteDisallowed(null);

            //this guy should be a usual suspect!
            Assert.IsTrue(finder.TheUsualSuspects.Any(s => s.Type == typeof(ReferentialIntegrityConstraint)));

            var testXML = 
            @"<?xml version=""1.0"" encoding=""utf-16""?>
<Validator xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <ItemValidators>
    <ItemValidator>
      <TargetProperty>previous_address_L1</TargetProperty>
      <SecondaryConstraints>
        <SecondaryConstraint xsi:type=""ReferentialIntegrityConstraint"">
          <Consequence xsi:nil=""true"" />
          <InvertLogic>true</InvertLogic>
          <OtherColumnInfoID>10029</OtherColumnInfoID>
        </SecondaryConstraint>
      </SecondaryConstraints>
    </ItemValidator>
  </ItemValidators>
</Validator>";

            bool kaizerSoze = false;
            foreach (Suspect suspect in finder.TheUsualSuspects)
            {
                string pattern = string.Format(suspect.Pattern, 10029);

                kaizerSoze = Regex.IsMatch(testXML, pattern,RegexOptions.Singleline);

                if (kaizerSoze)
                    break;
            }
            
            Assert.IsTrue(kaizerSoze);
        }

        public Startup CreateStartup()
        {
            // Ignore the test patcher classes that are in the test assembly (two of them are intentionally invalid and will cause setup to fail)
            var pluginBootstrapper = new PluginBootstrapper(CatalogueRepository)
            {
                PatcherTypesToIgnore =
                    new[]
                    {
                        "TestPluginPatcher", 
                        "TestPluginPatcherWithNoConstructor",
                        "TestPluginPatcherWithNoValidConstructor"
                    }
            };

            return new Startup(RepositoryLocator);
        }

        [Test]
        public void DeleteAReferencedValidationXML()
        {
            ColumnInfo l2ColumnInfo;
            BulkTestsData testData = SetupTestData(out l2ColumnInfo);
            try
            {
                Validator.LocatorForXMLDeserialization = RepositoryLocator;

                var worked = Validator.LoadFromXml(testData.catalogue.ValidatorXML);

                //notice that it is the ID of the referenced column that is maintained not the name of it! that is because we need to use a data access portal to get the contents of the column which might be in a different table (and normally would be)
                Assert.IsFalse(testData.catalogue.ValidatorXML.Contains("previous_address_L2"));
                Assert.IsTrue(testData.catalogue.ValidatorXML.Contains(l2ColumnInfo.ID.ToString()));

                Assert.IsTrue(testData.catalogue.ValidatorXML.Contains("previous_address_L1"));

                //we expect the validation XML to find the reference
                ValidationXMLObscureDependencyFinder finder = new ValidationXMLObscureDependencyFinder(RepositoryLocator);
                
                //and explode
                Assert.Throws<ValidationXmlDependencyException>(() => finder.ThrowIfDeleteDisallowed(l2ColumnInfo));
                
                Assert.AreEqual(0,finder.BlackList.Count);

                //now clear the validation XML
                testData.catalogue.ValidatorXML = testData.catalogue.ValidatorXML.Insert(100,"I've got a lovely bunch of coconuts!");
                testData.catalogue.SaveToDatabase();

                //column info should be deleteable but only because we got ourselves onto the blacklist
                Assert.DoesNotThrow(() => finder.ThrowIfDeleteDisallowed(l2ColumnInfo));
                Assert.AreEqual(1, finder.BlackList.Count);

                testData.catalogue.ValidatorXML = "";
                testData.catalogue.SaveToDatabase();

                //column info should be deleteable now that we cleared the XML
                Assert.DoesNotThrow(() => finder.ThrowIfDeleteDisallowed(l2ColumnInfo));
            }
            finally
            {
                testData.DeleteCatalogue();
            }
        }
        
        [Test]
        public void TheIronTest_DeleteAColumnInfoThatIsReferenced()
        {
            var startup = new Startup(RepositoryLocator);
            startup.DoStartup(new IgnoreAllErrorsCheckNotifier());

            ColumnInfo l2ColumnInfo;
            var testData = SetupTestData(out l2ColumnInfo);

            try
            {
                //should fail because of the validation constraint being dependent on it
                Assert.Throws<ValidationXmlDependencyException>(()=>l2ColumnInfo.DeleteInDatabase());
            }
            finally
            {
                testData.catalogue.ValidatorXML = null;
                testData.catalogue.SaveToDatabase();

                testData.DeleteCatalogue();
            }
        }

        [Test]
        public void TestRunningSetupMultipleTimes()
        {

            var startup = new Startup(RepositoryLocator);
            try
            {
                startup.DoStartup(new IgnoreAllErrorsCheckNotifier());
            }
            catch (InvalidPatchException patchException)
            {
                throw new Exception("Problem in patch " + patchException.ScriptName ,patchException);
            }
            //there should be all the obscure dependencies we need done with only the first call to this function
            int numberAfterFirstRun =
                ((CatalogueObscureDependencyFinder) CatalogueRepository.ObscureDependencyFinder)
                    .OtherDependencyFinders.Count;

            startup.DoStartup(new IgnoreAllErrorsCheckNotifier());
            startup.DoStartup(new IgnoreAllErrorsCheckNotifier());
            startup.DoStartup(new IgnoreAllErrorsCheckNotifier());

            //there should not be any replication! and doubling up!
            Assert.AreEqual(numberAfterFirstRun,
                ((CatalogueObscureDependencyFinder) CatalogueRepository.ObscureDependencyFinder)
                    .OtherDependencyFinders.Count);
            
            
        }

        #region setup test data with some validation rule

        private BulkTestsData SetupTestData(out ColumnInfo l2ColumnInfo)
        {
            //Setup test data
            var testData = new BulkTestsData(CatalogueRepository, DiscoveredDatabaseICanCreateRandomTablesIn);
            testData.SetupTestData();
            testData.ImportAsCatalogue();

            //Setup some validation rules
            Validator v = new Validator();

            //rule is that previous address line 1 cannot be the same as previous address line 2
            var iv = new ItemValidator("previous_address_L1");
            l2ColumnInfo = testData.columnInfos.Single(c => c.GetRuntimeName().Equals("previous_address_L2"));

            //define the secondary constraint
            var referentialConstraint = new ReferentialIntegrityConstraint(CatalogueRepository);
            referentialConstraint.InvertLogic = true;
            referentialConstraint.OtherColumnInfo = l2ColumnInfo;

            //add it to the item validator for previous_address_L1
            iv.SecondaryConstraints.Add(referentialConstraint);

            //add the completed item validator to the validator (normally there would be 1 item validator per column with validation but in this test we only have 1)
            v.ItemValidators.Add(iv);

            testData.catalogue.ValidatorXML = v.SaveToXml();
            testData.catalogue.SaveToDatabase();

            return testData;
        }
        #endregion
    }
}
