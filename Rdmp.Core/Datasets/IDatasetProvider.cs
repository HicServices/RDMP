using Rdmp.Core.Curation.Data.Datasets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Datasets;

public interface IDatasetProvider
{

    List<Curation.Data.Datasets.Dataset> FetchDatasets();

    Curation.Data.Datasets.Dataset FetchDatasetByID(int id);

}
