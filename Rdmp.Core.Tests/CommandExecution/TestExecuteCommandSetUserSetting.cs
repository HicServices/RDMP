﻿// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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
