using System.Collections.Generic;
using System.Text.RegularExpressions;
using Moq;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandExecution
{
    class TestExecuteCommandDescribeCommand : UnitTests
    {
        [Test]
        public void Test_DescribeDeleteCommand()
        {
            var mock = new Mock<IBasicActivateItems>();
            mock.Setup(m => m.RepositoryLocator).Returns(RepositoryLocator);
            mock.Setup(m => m.GetDelegates()).Returns(new List<CommandInvokerDelegate>());
            mock.Setup(m => m.Show(It.IsAny<string>()));

            var cmd = new ExecuteCommandDescribeCommand(mock.Object, typeof(ExecuteCommandDelete));
            Assert.IsFalse(cmd.IsImpossible,cmd.ReasonCommandImpossible);

            cmd.Execute();

            string contents = Regex.Escape(@"cmd Delete <deletable> 
PARAMETERS:
deletable	IDeleteable	The object you want to delete");

            // Called once
            mock.Verify(m => m.Show(It.IsRegex(contents)), Times.Once());

            
        }
    }
}