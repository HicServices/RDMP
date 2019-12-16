using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Rdmp.Core.CommandLine.Interactive;
using ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandLine.Interactive
{
    class ConsoleInputManagerTests : UnitTests
    {
        [Test]
        public void TestDisallowInput()
        {
            var manager = new ConsoleInputManager(RepositoryLocator, new ThrowImmediatelyCheckNotifier());
            manager.DisallowInput = true;
            
            Assert.Throws<InputDisallowedException>(()=>manager.GetString("bob", null));
        }
    }
}
