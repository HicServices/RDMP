using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.DataHelper.RegexRedaction
{
    public interface IRegexRedactionConfiguration: IMapsDirectlyToDatabaseTable
    {
        ICatalogueRepository CatalogueRepository { get; }

        string Name { get; }
        string Description { get; }
        string RegexPattern { get; }
        string RedactionString { get; }
    }
}
