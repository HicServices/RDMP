using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Providers;
public class DatasetChildProvider : CatalogueChildProvider
{
    public DatasetChildProvider(ICatalogueRepository repository, IChildProvider[] pluginChildProviders, ICheckNotifier errorsCheckNotifier, CatalogueChildProvider previousStateIfKnown) : base(repository, pluginChildProviders, errorsCheckNotifier, previousStateIfKnown)
    {
    }
}

