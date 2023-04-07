// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using NUnit.Framework;
using Rdmp.Core.Validation;
using Rdmp.Core.Validation.Constraints;
using Rdmp.Core.Validation.Constraints.Secondary;

namespace Rdmp.Core.Tests.Validation.Constraints.Primary;

[Category("Unit")]
class BoundsValidationIntegerTest : ValidationTests
{

    [Test]
    public void simple_integer_bounds()
    {
        var v = new Validator();

        var b = (BoundDouble)Validator.CreateConstraint("bounddouble",Consequence.Wrong);
        b.Lower = 5;
        b.Upper = 120;

        var i = new ItemValidator();
        i.AddSecondaryConstraint(b);
        v.AddItemValidator(i, "number", typeof(int));

        var d = new Dictionary<string, object>();
        d.Add("number", 119);

        Assert.IsNull(v.Validate(d));
    }
}