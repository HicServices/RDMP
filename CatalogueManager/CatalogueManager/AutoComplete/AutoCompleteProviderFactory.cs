using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ScintillaNET;

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
