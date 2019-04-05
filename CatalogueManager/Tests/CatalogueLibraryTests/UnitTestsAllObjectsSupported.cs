using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Spontaneous;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable.Revertable;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using Rhino.Mocks;
using Tests.Common;

namespace CatalogueLibraryTests
{
    class UnitTestsAllObjectsSupported:UnitTests
    {
        //These types do not have to be supported by the method WhenIHaveA
        private HashSet<string> _skipTheseTypes = new HashSet<string>(new string[]
        {
            "TestColumn",
            "ExtractableCohort",
            "DQEGraphAnnotation"
        });
            
        [Test]
        public void TestAllSupported()
        {
            //load all DatabaseEntity types
            MEF mef = new MEF();
            mef.Setup(new SafeDirectoryCatalog(TestContext.CurrentContext.TestDirectory));

            List<Exception> ex;
            var types = mef.GetAllTypesFromAllKnownAssemblies(out ex)
                .Where(t => typeof (DatabaseEntity).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface).ToArray();

            var methods = typeof(UnitTests).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
            var method = methods.Single(m => m.Name.Equals("WhenIHaveA") && !m.GetParameters().Any());
             
            List<Type> notSupported = new List<Type>();
            
            foreach (Type t in types)
            {
                //ignore these types too
                if (_skipTheseTypes.Contains(t.Name) || t.Name.StartsWith("Spontaneous") || typeof(SpontaneousObject).IsAssignableFrom(t))
                    continue;

                DatabaseEntity instance = null;

                try
                {
                    //ensure that the method supports the Type
                    var generic = method.MakeGenericMethod(t);
                    instance = (DatabaseEntity)generic.Invoke(this, null);
                }
                catch (TargetInvocationException exception)
                {
                    if (exception.InnerException is TestCaseNotWrittenYetException)
                        notSupported.Add(t);
                    else
                        throw;
                }

                //if the instance returned by MakeGenericMethod does not pass checks that's a dealbreaker!
                if (instance != null)
                {
                    try
                    {
                        //and that it returns an instance
                        Assert.IsNotNull(instance);
                        Assert.IsTrue(instance.Exists());
                        Assert.AreEqual(ChangeDescription.NoChanges, instance.HasLocalChanges().Evaluation);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Implementation of WhenIHaveA<" + t.Name + "> is flawed",e);
                    }
                }

            }

            Assert.IsEmpty(notSupported, "The following Types were not supported by WhenIHaveA<T>:" +Environment.NewLine + string.Join(Environment.NewLine,notSupported.Select(t=>t.Name)));
        }
    }
}
