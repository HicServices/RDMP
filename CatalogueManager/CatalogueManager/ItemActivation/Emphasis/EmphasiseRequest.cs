using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapsDirectlyToDatabaseTable;

namespace CatalogueManager.ItemActivation.Emphasis
{
    public class EmphasiseRequest
    {
        public IMapsDirectlyToDatabaseTable ObjectToEmphasise { get; set; }
        public int ExpansionDepth { get; set; }

        public EmphasiseRequest(IMapsDirectlyToDatabaseTable objectToEmphasise, int expansionDepth = 0)
        {
            ObjectToEmphasise = objectToEmphasise;
            ExpansionDepth = expansionDepth;
        }
    }
}
