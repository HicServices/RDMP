// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;
using NUnit.Framework;
using Rdmp.Core.Validation;
using Rdmp.Core.Validation.Constraints.Primary;

namespace Rdmp.Core.Tests.Validation;

[Category("Unit")]
public class ValidationDeserializationMemoryTest
{
    [Test]
    public void TestMemoryLeak()
    {
        var v = new Validator();
        v.ItemValidators.Add(new ItemValidator("CHI") { PrimaryConstraint = new Chi() });
        var xml = v.SaveToXml();

        var bytesAtStart = Process.GetCurrentProcess().WorkingSet64;

        for (var i = 0; i < 1000; i++)
        {
            Validator.LoadFromXml(xml);

            if (i % 500 == 0)
            {
                GC.Collect();
                Console.WriteLine($"Committed Bytes:{Process.GetCurrentProcess().WorkingSet64}");
            }
        }

        var bytesAtEnd = Process.GetCurrentProcess().WorkingSet64;

        Assert.That(bytesAtEnd, Is.LessThan(bytesAtStart * 2),
            $"Should not be using double the working memory as many bytes by the end, at start we were using {bytesAtStart} at end we were using {bytesAtEnd} (Increase of {(float)bytesAtEnd / bytesAtStart} times)");
    }
}