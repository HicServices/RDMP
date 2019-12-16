// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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