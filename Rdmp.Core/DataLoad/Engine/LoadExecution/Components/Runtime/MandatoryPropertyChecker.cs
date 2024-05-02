// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;

/// <summary>
///     Checks that all Properties on the supplied classInstanceToCheck that are decorated with a [DemandsInitialization]
///     where the Mandatory flag is true have
///     a value.
/// </summary>
public class MandatoryPropertyChecker : ICheckable
{
    private readonly object _classInstanceToCheck;

    public MandatoryPropertyChecker(object classInstanceToCheck)
    {
        _classInstanceToCheck = classInstanceToCheck;
    }

    public void Check(ICheckNotifier notifier)
    {
        //get all possible properties that we could set
        foreach (var propertyInfo in _classInstanceToCheck.GetType().GetProperties())
        {
            //see if any demand initialization
            var demand = Attribute.GetCustomAttributes(propertyInfo).OfType<DemandsInitializationAttribute>()
                .FirstOrDefault();

            //this one does
            if (demand is { Mandatory: true })
            {
                var value = propertyInfo.GetValue(_classInstanceToCheck);
                if (value == null || string.IsNullOrEmpty(value.ToString()))
                    notifier.OnCheckPerformed(new CheckEventArgs(
                        $"DemandsInitialization Property '{propertyInfo.Name}' is marked Mandatory but does not have a value",
                        CheckResult.Fail));
            }
        }
    }
}