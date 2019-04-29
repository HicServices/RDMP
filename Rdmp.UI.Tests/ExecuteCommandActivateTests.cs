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
using Tests.Common;

namespace Rdmp.UI.Tests
{
    internal class ExecuteCommandActivateTests: UITests
    {

        //These types do not have to be supported by the method WhenIHaveA
        private HashSet<string> _skipTheseTypes = new HashSet<string>(new string[]
        {
            "TestColumn",
            "ExtractableCohort",
            "DQEGraphAnnotation",
            "WindowLayout"
        });

        /// <summary>
        /// Tests that all DatabaseEntity objects can be constructed with <see cref="UnitTests.WhenIHaveA{T}()"/> and that if <see cref="ExecuteCommandActivate"/>  says
        /// they can be activated then they can be (without blowing up in a major way).
        /// </summary>
        [Test,UITimeout(50000)]
        public void Test_ExecuteCommandActivate_AllObjectsActivateable()
        {
            SetupMEF();
            
            List<Exception> ex;
            var types = Repository.CatalogueRepository.MEF.GetAllTypesFromAllKnownAssemblies(out ex)
                .Where(t => t != null && typeof (DatabaseEntity).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface).ToArray();

            var methods = typeof (UnitTests).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
            var method = methods.Single(m => m.Name.Equals("WhenIHaveA") && !m.GetParameters().Any());

            foreach (Type t in types)
            {
                //ignore these types too
                if (_skipTheseTypes.Contains(t.Name) || t.Name.StartsWith("Spontaneous") ||
                    typeof (SpontaneousObject).IsAssignableFrom(t))
                    continue;

                //ensure that the method supports the Type
                var generic = method.MakeGenericMethod(t);
                var instance = (DatabaseEntity) generic.Invoke(this, null);

                var cmd = new ExecuteCommandActivate(ItemActivator, instance);
                
                if(!cmd.IsImpossible)
                {
                    try
                    {
                        cmd.Execute();
                        AssertNoErrors(ExpectedErrorType.KilledForm);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Could not Activate Type:" + t.Name,e);
                    }
                }
            }
        }
    }
}