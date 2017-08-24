using System;

namespace HIC.Common.Validation.Constraints.Secondary
{
    public class NotNull : SecondaryConstraint
    {
        public override ValidationFailure Validate(object value, object[] otherColumns, string[] otherColumnNames)
        {
            if (value == null || value == DBNull.Value)
                return new ValidationFailure("Value cannot be null",this);

            if (value is string && string.IsNullOrWhiteSpace(value.ToString()))
                return new ValidationFailure("Value cannot be whitespace only", this);

            return null;
        }
        
        public override void RenameColumn(string originalName, string newName)
        {
            
        }

        public override string GetHumanReadableDescriptionOfValidation()
        {
            return "not null";
        }

    }
}
