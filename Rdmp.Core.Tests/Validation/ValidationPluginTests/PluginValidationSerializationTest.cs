// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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
        Validator v = new Validator();
        var iv = new ItemValidator("fish");
        iv.PrimaryConstraint = new FishConstraint();

        //validation should be working
        Assert.IsNull(iv.ValidateAll("Fish", new object[0], new string[0]));
        Assert.IsNotNull(iv.ValidateAll("Potato", new object[0], new string[0]));

        v.ItemValidators.Add(iv);

        Assert.AreEqual(1, v.ItemValidators.Count);
        Assert.AreEqual(typeof(FishConstraint), v.ItemValidators[0].PrimaryConstraint.GetType());

        string xml = v.SaveToXml();

        var newV = Validator.LoadFromXml(xml);

        Assert.AreEqual(1,newV.ItemValidators.Count);
        Assert.AreEqual(typeof(FishConstraint), newV.ItemValidators[0].PrimaryConstraint.GetType());

    }
}

public class FishConstraint : PluginPrimaryConstraint
{
    public override void RenameColumn(string originalName, string newName)
    {
            
    }

    public override string GetHumanReadableDescriptionOfValidation()
    {
        return "Fish Constraint For Testing";
    }

    public override ValidationFailure Validate(object value)
    {
        if (value == null)
            return null;

        string result = value as string ?? value.ToString();

        if (result.Equals("Fish"))
            return null;

        return new ValidationFailure("Value '" + value +"' was not 'Fish'!",this);
    }
}