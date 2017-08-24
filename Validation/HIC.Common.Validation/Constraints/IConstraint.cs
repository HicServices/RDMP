namespace HIC.Common.Validation.Constraints
{
    public interface IConstraint
    {
        Consequence? Consequence { get; set; }
       
        void RenameColumn(string originalName, string newName);

        string GetHumanReadableDescriptionOfValidation();
    }
}