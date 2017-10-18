using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Pipelines;
using NUnit.Framework;
using Tests.Common;

namespace DataLoadEngineTests.Integration.PipelineTests
{
    public class PipelineArgumentTests:DatabaseTests
    {
        [Test]
        [TestCase(typeof(int?),null)]
        [TestCase(typeof(float?), null)]
        [TestCase(typeof(double?), null)]
        [TestCase(typeof(char?), null)]
        [TestCase(typeof(DateTime?), null)]
        

        [TestCase(typeof(int?), 3)]
        [TestCase(typeof(float?), 10.01f)]
        [TestCase(typeof(double?), 10.999)]
        [TestCase(typeof(char?), 'K')]
        [TestCase(typeof(DateTime?), "now")] //sadly can't pass DateTime.Now

        public void TestIArgumentsForNullableTypes(Type nullableType,object value)
        {
            if (String.Equals(value as String, "now")) //sadly can't pass DateTime.Now 
                value = new DateTime(2001, 01, 01, 3, 20, 11); //hey btw when you put in milliseconds into DateTime IArgument it drops them... due to DateTime.Parse? or DateTime.ToString()?

            var p = new Pipeline(CatalogueRepository);
            var pc = new PipelineComponent(CatalogueRepository,p,
                GetType() //Normally this would be the PipelineComponent hosted class which would be a proper class declared as a MEF export with DemandsInitialization etc but we don't need all that
                ,0
                ,"My imaginary Pipe Component");
            var arg = new PipelineComponentArgument(CatalogueRepository,pc);
            try
            {
                arg.SetType(nullableType);
                arg.SetValue(value);

                Assert.AreEqual(nullableType,arg.GetSystemType());
                Assert.AreEqual(value,arg.GetValueAsSystemType());

            }
            finally
            {
                p.DeleteInDatabase();
            }
        }
    }
}
