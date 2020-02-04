using System;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive.Picking;

namespace Rdmp.Core.Tests.CommandExecution
{
    class TestExecuteCommandImportTableInfo : CommandCliTests
    {
        [Test]
        public void Test_ImportTableInfo_NoArguments()
        {
            
            var ex = Assert.Throws<Exception>(() => GetInvoker().ExecuteCommand(typeof(ExecuteCommandImportTableInfo),
                new CommandLineObjectPicker(new string[0], RepositoryLocator)));

            StringAssert.StartsWith("Expected parameter at index 0 to be a FAnsi.Discovery.DiscoveredTable (for parameter 'table') but it was Missing",ex.Message);
        }

        [Test]
        public void Test_ImportTableInfo_MalformedArgument()
        {
            var ex = Assert.Throws<Exception>(() => GetInvoker().ExecuteCommand(typeof(ExecuteCommandImportTableInfo),
                new CommandLineObjectPicker(new string[]{ "MyTable"}, RepositoryLocator)));

            StringAssert.StartsWith("Expected parameter at index 0 to be a FAnsi.Discovery.DiscoveredTable (for parameter 'table') but it was MyTable",ex.Message);
        }

        [Test]
        public void Test_ImportTableInfo_NoTable()
        {
            var tbl = "Table:MyTable:DatabaseType:MicrosoftSQLServer:Server=myServerAddress;Database=myDataBase;Trusted_Connection=True";
            
            var ex = Assert.Throws<Exception>(() => GetInvoker().ExecuteCommand(typeof(ExecuteCommandImportTableInfo),
                new CommandLineObjectPicker(new string[]{ tbl,"true"}, RepositoryLocator)));
            
            StringAssert.StartsWith("Could not reach server myServerAddress",ex.Message);
        }

    }
}