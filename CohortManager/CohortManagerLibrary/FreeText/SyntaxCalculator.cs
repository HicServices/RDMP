using CatalogueLibrary.Data;

namespace CohortManagerLibrary.FreeText
{
    public class SyntaxCalculator
    {
        private readonly Catalogue[] _exctractableCatalogues;
        
        public SyntaxCalculator(Catalogue[] exctractableCatalogues)
        {
            _exctractableCatalogues = exctractableCatalogues;
        }
        
    }
}
