using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;

namespace CatalogueLibraryTests.SourceCodeEvaluation
{
    public class InterfaceDeclarationsCorrect
    {
        public void FindProblems(MEF mef)
        {
            List<string> excusables = new List<string>()
            {
                "IPlugin",
                "IDataAccessCredentials"
            };
            List<string> problems = new List<string>();

            List<Exception> whoCares;
            foreach (var dbEntities in mef.GetAllTypesFromAllKnownAssemblies(out whoCares).Where(t => typeof(DatabaseEntity).IsAssignableFrom(t)))
            {
                var matchingInterface = mef.GetTypeByNameFromAnyLoadedAssembly("I" + dbEntities.Name);

                if (matchingInterface != null)
                {
                    if (excusables.Contains(matchingInterface.Name))
                        continue;

                    if (!typeof (IMapsDirectlyToDatabaseTable).IsAssignableFrom(matchingInterface))
                    {
                        problems.Add("FAIL: Interface '" + matchingInterface.Name + "' does not inherit IMapsDirectlyToDatabaseTable");
                    }
                }

            }

            foreach (string problem in problems)
                Console.WriteLine(problem);

                Assert.AreEqual(0,problems.Count);
        }
    }
}