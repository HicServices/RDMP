using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;

namespace CatalogueLibrary.QueryBuilding.Parameters
{
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
