using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data;

public interface ICatalogueOverviewDataPoint
{
    int CatalogueOverview_ID { get; }
    DateTime Date { get; set; }
    int Count { get;set; }

}
