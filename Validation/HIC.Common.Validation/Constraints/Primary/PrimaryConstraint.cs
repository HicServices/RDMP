namespace HIC.Common.Validation.Constraints.Primary
{
    public abstract class PrimaryConstraint : IPrimaryConstraint
    {
        public Consequence? Consequence { get; set; }

        public abstract void RenameColumn(string originalName, string newName);
        public abstract string GetHumanReadableDescriptionOfValidation();
        public abstract ValidationFailure Validate(object value);
    }
}