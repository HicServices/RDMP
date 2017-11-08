using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Repositories.Construction;
using CatalogueLibrary.Repositories.Construction.Exceptions;
using NUnit.Framework;

namespace CatalogueLibraryTests.Unit
{
    public class ObjectConstructorTests
    {
        [Test]
        public void ConstructValid()
        {
            var constructor =new ObjectConstructor();
            var testarg = new TestArg(){Text = "amagad"};
            var testarg2 = new TestArg2() { Text = "amagad" };

            //anyone can construct on object!
            constructor.Construct(typeof(TestClass1),testarg);
            constructor.Construct(typeof(TestClass1), testarg2);
            
            //basic case - identical Type parameter
            var instance = (TestClass2)constructor.Construct(typeof(TestClass2), testarg);
            Assert.AreEqual(instance.A.Text, "amagad");
            //also allowed because testarg2 is a testarg derrived class 
            constructor.Construct(typeof(TestClass2), testarg2);

            //not allowed because class 3 explicitly requires a TestArg2 
            Assert.Throws<ObjectLacksCompatibleConstructorException>(()=>constructor.Construct(typeof(TestClass3), testarg));
            
            //allowed
            constructor.Construct(typeof(TestClass3), testarg2);

        }

        class TestClass1
        {
            public TestClass1(object o)
            {
                
            }
        }
        class TestClass2
        {
            public TestArg A { get; set; }

            public TestClass2(TestArg a)
            {
                A = a;
            }
        }
        class TestClass3
        {
            public TestArg2 A { get; set; }

            public TestClass3(TestArg2 a)
            {
                A = a;
            }
        }

        class TestArg
        {
            public string Text { get; set; }
        }

        class TestArg2:TestArg
        {
             
        }
    }
}
