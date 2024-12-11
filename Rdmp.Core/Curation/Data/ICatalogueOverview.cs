using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rdmp.Core.ReusableLibraryCode.Annotations;

namespace Rdmp.Core.Curation.Data;

public interface ICatalogueOverview
{
    int Catalogue_ID { get; }
    DateTime? LastDataLoad { get; set; }
    DateTime? LastExtractionTime { get; set; }
    int NumberOfRecords { get; set; }
    int NumberOfPeople { get; set; }
    DateTime? StartDate {  get; set; }
    DateTime? EndDate {  get; set; }

    int DateColumn_ID { get; set; }
}
