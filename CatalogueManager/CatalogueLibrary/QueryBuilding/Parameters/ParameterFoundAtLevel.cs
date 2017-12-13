using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;

namespace CatalogueLibrary.QueryBuilding.Parameters
{
    /// <summary>
    /// Stores the fact that a ParameterManager found a particular ISqlParameter while evaluating all objects involved in a query being built (See ParameterManager for more
    ///  information).
    /// </summary>
    public class ParameterFoundAtLevel
    {
        public ISqlParameter Parameter { get; set; }
        public ParameterLevel Level { get; set; }

        public ParameterFoundAtLevel(ISqlParameter parameter,ParameterLevel level)
        {
            Parameter = parameter;
            Level = level;
        }

        public override string ToString()
        {
            return Parameter.ParameterName + " (At Level:" + Level + ")";
        }
    }
}
