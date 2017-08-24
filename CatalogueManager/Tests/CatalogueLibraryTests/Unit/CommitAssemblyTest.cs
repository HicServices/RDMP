using System;
using LoadModules.Generic;
using NUnit.Framework;

namespace CatalogueLibraryTests.Unit
{
    [Category("Unit")]
    public class CommitAssemblyTest
    {
        [Test]
        public void TestGetTypeByName()
        {
            const ScheduleStrategy s = ScheduleStrategy.Test;
            Console.Write(s.GetType().FullName);
            
            var t = Type.GetType(s.GetType().AssemblyQualifiedName);
            
            Assert.AreEqual(s.GetType(),t);
        }
    }
}
