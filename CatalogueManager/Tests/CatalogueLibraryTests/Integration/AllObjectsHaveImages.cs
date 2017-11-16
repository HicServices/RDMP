using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueManager.Icons.IconProvision;
using NUnit.Framework;
using ReusableLibraryCode;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    public class AllObjectsHaveImages:DatabaseTests
    {
        [Test]
        public void AllIHasDependenciesHaveIcons()
        {
            List<string> missingConcepts = new List<string>();


            string[] ExceptionsAllowed = new[]
            {

                "IHasDependencies", //base interface of which nobody is who isn't otherwise on this list
                "ITableInfo", //handled because they are all secretly TableInfos
                "AggregateConfiguration", //handled by AggregateConfigurationStateBasedIconProvider
                "AnyTableSqlParameter", //handled by CatalogueIconProvider looking for ISqlParameter classes
                "ICatalogue", //handled because they are all secretly Catalogues
                "ExtractionFilter", //handled by CatalogueIconProvider looking for IFilter
                "ExtractionFilterParameter",
                //handled by CatalogueIconProvider just like AnyTableSqlParameter by looking for the ISqlParameter classes

                "StackFrame"
                //not required , it's only ever dependent on itself and it doesnt have any visualisation on Catalogue / Export

            };

            List<Exception> whoCares;
            foreach (Type type in RepositoryLocator.CatalogueRepository.MEF.GetAllTypesFromAllKnownAssemblies(out whoCares).Where(t => typeof (IHasDependencies).IsAssignableFrom(t) && !t.IsInterface))
            {
                var typeName = type.Name;
                if (ExceptionsAllowed.Any(s=>s.Equals(typeName)))
                    continue;

                try
                {
                    var c = Enum.Parse(typeof (RDMPConcept), typeName);
                }
                catch (Exception e)
                {
                    missingConcepts.Add(typeName);
                }
            }

            Console.WriteLine("The following Database Object Types are missing concepts (and therefore images) in CatalogueManager.exe" + Environment.NewLine + string.Join("," + Environment.NewLine , missingConcepts));

            Assert.AreEqual(0,missingConcepts.Count);
        }
    }
}