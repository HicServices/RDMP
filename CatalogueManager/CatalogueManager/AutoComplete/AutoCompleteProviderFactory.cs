using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode;
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

        public AutoCompleteProvider Create(IHasDependencies forObject)
        {
            var provider = new AutoCompleteProvider(_activator);

            if(forObject != null)
                RecursiveAdd(provider, forObject, 0);

            provider.AddSQLKeywords();

            return provider;
        }

        private int maxRecursionDepth = 10;

        private void RecursiveAdd(AutoCompleteProvider provider, IHasDependencies forObject,int depth)
        {
            //don't add the root object to autocomplete
            if(depth != 0)
            {
                var col = forObject as IColumn;
                var tbl = forObject as TableInfo;

                if(col != null)
                    provider.Add(col);

                if(tbl != null)
                    provider.Add(tbl);
            }
            
            if(depth >= maxRecursionDepth)
                return;

            var dependencies = forObject.GetObjectsThisDependsOn();
            
            if(dependencies == null)
                return;
            
            foreach (var dependant in dependencies)
                RecursiveAdd(provider, dependant,depth + 1);
        }
    }
}
