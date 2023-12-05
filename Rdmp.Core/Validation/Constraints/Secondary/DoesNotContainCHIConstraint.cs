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
