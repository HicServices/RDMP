// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories.Construction;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Unit;

[Category("Unit")]
public class ObjectConstructorTests : UnitTests
{
    [Test]
    public void ConstructValidTests()
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

        //valid because even though both constructors are valid there is one that matches EXACTLY on Type
        constructor.Construct(typeof(TestClass4), testarg2);

        var testarg3 = new TestArg3();

        //not valid because there are 2 constructors that are both base classes of TestArg3 so ObjectConstructor doesn't know which to invoke
        var ex = Assert.Throws<ObjectLacksCompatibleConstructorException>(()=>constructor.Construct(typeof (TestClass4), testarg3));
        Assert.IsTrue(ex.Message.Contains("Could not pick the correct constructor between"));

        //exactly the same as the above case but one constructor has been decorated with [UseWithObjectConstructor] attribute
        constructor.Construct(typeof (TestClass5), testarg3);
    }

    [Test]
    public void ConstructIfPossibleTests_BlankConstructors()
    {
        var constructor = new ObjectConstructor();
            
        //blank constructors are only used if no params are specified
        Assert.IsNotNull(constructor.ConstructIfPossible(typeof(TestClassDefaultConstructor)));
            
        //no constructor taking an int
        Assert.IsNull(constructor.ConstructIfPossible(typeof(TestClassDefaultConstructor),8));
    }

    [Test]
    public void GetRepositoryConstructor_AllDatabaseEntities_OneWinningConstructor()
    {
        SetupMEF();
            
        int countCompatible = 0;

        var badTypes = new Dictionary<Type,Exception>();
        foreach (Type t in MEF.GetAllTypes().Where(typeof(DatabaseEntity).IsAssignableFrom))
        {
            try
            {
                var oc = new ObjectConstructor();
                Assert.IsNotNull(oc.GetRepositoryConstructor(typeof(Catalogue)));
                countCompatible++;
            }
            catch (Exception e)
            {
                badTypes.Add(t,e);
            }
        }

        Assert.IsEmpty(badTypes);
        Assert.GreaterOrEqual(countCompatible,10);
        Console.WriteLine("Found compatible constructors on " + countCompatible + " objects");
    }

    class TestClassDefaultConstructor
    {
            
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

    class TestClass4
    {
        public TestArg A { get; set; }

        public TestClass4(TestArg a)
        {
            A = a;
        }

        public TestClass4(TestArg2 a)
        {
            A = a;
        }
    }


    class TestClass5
    {
        public TestArg A { get; set; }

        public TestClass5(TestArg a)
        {
            A = a;
        }
        [UseWithObjectConstructor]
        public TestClass5(TestArg2 a)
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
    class TestArg3 : TestArg2
    {

    }
}