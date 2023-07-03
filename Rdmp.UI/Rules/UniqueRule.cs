// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.Rules;

internal class UniqueRule<T> : BinderRule<T> where T : IMapsDirectlyToDatabaseTable
{
    private readonly string _problemDescription;

    public UniqueRule(IActivateItems activator, T toTest, Func<T, object> propertyToCheck, Control control, string propertyToCheckName)
        : base(activator, toTest, propertyToCheck, control, propertyToCheckName)
    {
        _problemDescription = $"Must be unique amongst all {toTest.GetType().Name}s";

    }

    protected override string IsValid(object currentValue, Type typeToTest)
    {
        //never check for uniqueness on null values
        if (currentValue == null || string.IsNullOrWhiteSpace(currentValue.ToString()))
            return null;

        return Activator.CoreChildProvider.GetAllSearchables()
            .Keys.OfType<T>()
            .Except(new[] { ToTest })
            .Where(t => t.GetType() == typeToTest)
            .Any(v => AreEqual(v, currentValue))
            ? _problemDescription
            : null;
    }

    private bool AreEqual(T arg, object currentValue)
    {
        return currentValue is string s
            ? string.Equals(s, PropertyToCheck(arg) as string, StringComparison.CurrentCultureIgnoreCase)
            : Equals(currentValue, PropertyToCheck(arg));
    }
}