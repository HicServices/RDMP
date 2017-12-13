using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueLibrary.Triggers
{
    /// <summary>
    /// Container class for constant variables for the names of special columns required by the backup trigger (_Archive table and general DLE audit columns).
    /// </summary>
    public class SpecialFieldNames
    {
        public const string ValidFrom = "hic_validFrom";
        public const string DataLoadRunID = "hic_dataLoadRunID";
    }
}
