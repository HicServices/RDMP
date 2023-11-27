using FAnsi.Naming;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HICPlugin.Curation.Data;

public interface IRedactedCHI :IMapsDirectlyToDatabaseTable
{

    ICatalogueRepository CatalogueRepository { get; }

    string PotentialCHI { get; }
    string CHIContext{ get; }

    string CHILocation { get; }
}
