﻿// Copyright (c) The University of Dundee 2018-2019
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
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.Repositories;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation;

internal class UnitTestsAllObjectsSupported : UnitTests
{
    /// <summary>
    /// Who tests the tester? this method does! It makes sure that <see cref="UnitTests.WhenIHaveA{T}()"/> supports all <see cref="DatabaseEntity"/> classes (except
    /// those listed in <see cref="UnitTests.SkipTheseTypes"/>) and returns a valid value.
    /// </summary>
    [Test]
    public void TestAllSupported()
    {
        //load all DatabaseEntity types
        var types = MEF.GetAllTypes()
            .Where(static t => typeof(DatabaseEntity).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface).ToArray();

        var methods = typeof(UnitTests).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
        var method = methods.Single(m => m.Name.Equals("WhenIHaveA") && !m.GetParameters().Any());

        var notSupported = new List<Type>();

        foreach (var t in types)
        {
            //ignore these types too
            if (SkipTheseTypes.Contains(t.Name) || t.Name.StartsWith("Spontaneous", StringComparison.Ordinal) ||
                typeof(SpontaneousObject).IsAssignableFrom(t))
                continue;

            DatabaseEntity instance = null;

            try
            {
                //ensure that the method supports the Type
                var generic = method.MakeGenericMethod(t);
                instance = (DatabaseEntity)generic.Invoke(this, null);
            }
            catch (TargetInvocationException exception)
            {
                if (exception.InnerException is TestCaseNotWrittenYetException)
                    notSupported.Add(t);
                else
                    throw;
            }

            //if the instance returned by MakeGenericMethod does not pass checks that's a dealbreaker!
            if (instance != null)
                try
                {
                    //and that it returns an instance
                    Assert.That(instance, Is.Not.Null);
                    Assert.Multiple(() =>
                    {
                        Assert.That(instance.Exists());
                        Assert.That(instance.HasLocalChanges().Evaluation, Is.EqualTo(ChangeDescription.NoChanges),
                            "Type was '" + t.Name + "'");
                    });
                }
                catch (Exception e)
                {
                    throw new Exception($"Implementation of WhenIHaveA<{t.Name}> is flawed", e);
                }
        }

        Assert.That(notSupported, Is.Empty,
            $"The following Types were not supported by WhenIHaveA<T>:{Environment.NewLine}{string.Join(Environment.NewLine, notSupported.Select(t => t.Name))}");
    }
}