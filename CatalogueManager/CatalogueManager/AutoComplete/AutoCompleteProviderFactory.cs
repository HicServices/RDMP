using CatalogueManager.ItemActivation;
using FAnsi.Discovery.QuerySyntax;

namespace CatalogueManager.AutoComplete
{
    public class AutoCompleteProviderFactory
    {
        private readonly IActivateItems _activator;

        public AutoCompleteProviderFactory(IActivateItems activator)
        {
            _activator = activator;
        }

        public AutoCompleteProvider Create()
        {
            return Create(null);
        }

        public AutoCompleteProvider Create(IQuerySyntaxHelper helper)
        {
            var provider = new AutoCompleteProvider(_activator);

            if (helper != null)
                provider.AddSQLKeywords(helper);

            return provider;
        }
    }
}
