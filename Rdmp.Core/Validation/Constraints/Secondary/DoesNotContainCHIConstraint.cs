// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Validation.Constraints.Secondary;
/// <summary>
/// TODO
/// </summary>
public class DoesNotContainCHIConstraint : SecondaryConstraint, ICheckable
{
    private readonly IRepository _repository;

    public DoesNotContainCHIConstraint() {
        _repository = Validator.LocatorForXMLDeserialization.CatalogueRepository;
    }


    public void Check(ICheckNotifier notifier)
    {
    }

    public override string GetHumanReadableDescriptionOfValidation()
    {
        return "TODO";
    }

    public override void RenameColumn(string originalName, string newName)
    {
    }

    public override ValidationFailure Validate(object value, object[] otherColumns, string[] otherColumnNames)
    {
        if (value == null || value == DBNull.Value)
            return null;

        if (string.IsNullOrWhiteSpace(value.ToString()))
            return null;
        var potentialCHI = CHIColumnFinder.GetPotentialCHI(value.ToString());
        if(string.IsNullOrWhiteSpace(potentialCHI)) return null;
        return new ValidationFailure($"Potential CHI {potentialCHI} was found.",this);
    }
}
