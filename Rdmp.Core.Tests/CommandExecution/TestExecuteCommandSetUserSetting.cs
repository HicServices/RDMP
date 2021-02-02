using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive.Picking;
using ReusableLibraryCode.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rdmp.Core.Tests.CommandExecution
{
    class TestExecuteCommandSetUserSetting : CommandCliTests
    {
        [Test]
        public void Test_CatalogueDescription_Normal()
        {
            UserSettings.AllowIdentifiableExtractions = false;

            GetInvoker().ExecuteCommand(typeof(ExecuteCommandSetUserSetting),new CommandLineObjectPicker(new []{"AllowIdentifiableExtractions","true"},RepositoryLocator));

            Assert.IsTrue(UserSettings.AllowIdentifiableExtractions);

            GetInvoker().ExecuteCommand(typeof(ExecuteCommandSetUserSetting),new CommandLineObjectPicker(new []{"AllowIdentifiableExtractions","false"},RepositoryLocator));
            
            Assert.IsFalse(UserSettings.AllowIdentifiableExtractions);

        }
    }
}
