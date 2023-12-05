// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NUnit.Framework;
using Rdmp.Core.Validation;
using Rdmp.Core.Validation.Constraints.Primary;

namespace Rdmp.Core.Tests.Validation.ValidationPluginTests;

[Category("Unit")]
public class PluginValidationSerializationTest
{
    [Test]
    public void TestSerialization()
    {
        var v = new Validator();
        var iv = new ItemValidator("fish")
        {
            PrimaryConstraint = new FishConstraint()
        };

        Assert.Multiple(() =>
        {
            //validation should be working
            Assert.That(iv.ValidateAll("Fish", Array.Empty<object>(), Array.Empty<string>()), Is.Null);
            Assert.That(iv.ValidateAll("Potato", Array.Empty<object>(), Array.Empty<string>()), Is.Not.Null);
        });

        v.ItemValidators.Add(iv);

        Assert.That(v.ItemValidators, Has.Count.EqualTo(1));
        Assert.That(v.ItemValidators[0].PrimaryConstraint.GetType(), Is.EqualTo(typeof(FishConstraint)));

        var xml = v.SaveToXml();

        var newV = Validator.LoadFromXml(xml);

        Assert.That(newV.ItemValidators, Has.Count.EqualTo(1));
        Assert.That(newV.ItemValidators[0].PrimaryConstraint.GetType(), Is.EqualTo(typeof(FishConstraint)));
    }
}

public class FishConstraint : PluginPrimaryConstraint
{
    public override void RenameColumn(string originalName, string newName)
    {
    }

    public override string GetHumanReadableDescriptionOfValidation() => "Fish Constraint For Testing";

    public override ValidationFailure Validate(object value)
    {
        if (value == null)
            return null;

        var result = value as string ?? value.ToString();

        return result.Equals("Fish") ? null : new ValidationFailure($"Value '{value}' was not 'Fish'!", this);
    }
}