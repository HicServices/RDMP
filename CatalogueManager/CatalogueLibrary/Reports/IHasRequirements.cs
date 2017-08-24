namespace CatalogueLibrary.Reports
{
    public interface IHasRequirements
    {
        bool RequirementsMet();
        string RequirementsDescription();
    }
}