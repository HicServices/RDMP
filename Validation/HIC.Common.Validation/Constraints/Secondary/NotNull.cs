using System;

namespace HIC.Common.Validation.Constraints.Secondary
{
    /// <summary>
    /// Values must appear in this column, if there are nulls (or whitespace) then the validation will fail.  While this kind of thing is trivially easy to implement
    /// at database level you might decided that (especially for unimportant columns) you are happy to load missing data rather than crash the data load.  That
    /// is why this constraint exists. 
    /// </summary>
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
