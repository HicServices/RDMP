using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandExecution
{
    class TestExecuteCommandDescribe : UnitTests
    {
        [Test]
        public void TestDescribeCatalogue()
        {
            var mock = new Mock<IBasicActivateItems>();
            mock.Setup(m => m.Show(It.IsAny<string>()));
            
            var c = WhenIHaveA<Catalogue>();
            c.Description = "fish";
            
            var describe = new ExecuteCommandDescribe(mock.Object,new []{c});
            Assert.IsFalse(describe.IsImpossible,describe.ReasonCommandImpossible);

            describe.Execute();

            // Called once
            mock.Verify(m => m.Show(It.IsRegex(".*Description:fish.*")), Times.Once());
        }
    }
}
