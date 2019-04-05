using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Spontaneous;
using CatalogueManager.CommandExecution.AtomicCommands;
using MapsDirectlyToDatabaseTable.Revertable;
using NUnit.Framework;
using Tests.Common;

namespace CatalogueLibraryTests.UserInterfaceTests
{
    internal class AllFormsLaunchableTest : UITests
    {

        //These types do not have to be supported by the method WhenIHaveA
        private HashSet<string> _skipTheseTypes = new HashSet<string>(new string[]
        {
            "TestColumn",
            "ExtractableCohort",
            "DQEGraphAnnotation",
            "WindowLayout"
        });

        [Test,UITimeout(50000)]
        public void TestAllSupported()
        {
            SetupMEF();
            
            List<Exception> ex;
            var types = Repository.CatalogueRepository.MEF.GetAllTypesFromAllKnownAssemblies(out ex)
                .Where(t => t != null && typeof (DatabaseEntity).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface).ToArray();

            var methods = typeof (UnitTests).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
            var method = methods.Single(m => m.Name.Equals("WhenIHaveA") && !m.GetParameters().Any());

            List<Type> notSupported = new List<Type>();

            foreach (Type t in types)
            {
                //ignore these types too
                if (_skipTheseTypes.Contains(t.Name) || t.Name.StartsWith("Spontaneous") ||
                    typeof (SpontaneousObject).IsAssignableFrom(t))
                    continue;

                //ensure that the method supports the Type
                var generic = method.MakeGenericMethod(t);
                var instance = (DatabaseEntity) generic.Invoke(this, null);

                var cmd = new ExecuteCommandActivate(ItemActivator, instance);
                
                if(!cmd.IsImpossible)
                {
                    try
                    {
                        cmd.Execute();
                        AssertNoErrors(ExpectedErrorType.KilledForm);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Could not Activate Type:" + t.Name,e);
                    }
                }

            }

            Assert.IsEmpty(notSupported,
                "The following Types were not supported by WhenIHaveA<T>:" + Environment.NewLine +
                string.Join(Environment.NewLine, notSupported.Select(t => t.Name)));
        }
    }
}