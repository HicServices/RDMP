// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using Tests.Common;

namespace Rdmp.UI.Tests
{
    internal class ExecuteCommandActivateTests: UITests
    {
        /// <summary>
        /// Tests that all DatabaseEntity objects can be constructed with <see cref="UnitTests.WhenIHaveA{T}()"/> and that if <see cref="ExecuteCommandActivate"/>  says
        /// they can be activated then they can be (without blowing up in a major way).
        /// </summary>
        [Test,UITimeout(50000)]
        public void Test_ExecuteCommandActivate_AllObjectsActivateable()
        {
            ForEachUI((ui)=>{AssertNoErrors(ExpectedErrorType.KilledForm);});
        }
    }
}