using ReusableLibraryCode;

namespace CatalogueLibrary.Reports
{
    public class RequiresMicrosoftOffice : IHasRequirements
    {
        public bool RequirementsMet()
        {
            if (OfficeVersionFinder.GetMajorVersion(OfficeVersionFinder.OfficeComponent.Word) == 0)
                return false;

            if (OfficeVersionFinder.GetMajorVersion(OfficeVersionFinder.OfficeComponent.Excel) == 0)
                return false;

            return true;
        }

        public string RequirementsDescription()
        {
            return "This component requires Microsoft Office to function";
        }
    }
}