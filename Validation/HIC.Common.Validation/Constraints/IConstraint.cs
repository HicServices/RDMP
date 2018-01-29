namespace HIC.Common.Validation.Constraints
{
    /// <summary>
    /// Base interface for all validation rules (and accompanying failure Consqeuences)
    /// </summary>
    public interface IConstraint
    {
        Consequence? Consequence { get; set; }
       
        void RenameColumn(string originalName, string newName);

        string GetHumanReadableDescriptionOfValidation();
    }
}