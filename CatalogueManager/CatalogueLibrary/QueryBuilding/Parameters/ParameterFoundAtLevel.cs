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
        /// <summary>
        /// The <see cref="ISqlParameter"/> that was found
        /// </summary>
        public ISqlParameter Parameter { get; set; }

        /// <summary>
        /// The <see cref="ParameterLevel"/> that the <see cref="Parameter"/> was found at during query building.  This allows parameters declared in individual <see cref="IFilter"/> to
        /// be overridden by parameters declared at higher scopes
        /// </summary>
        public ParameterLevel Level { get; set; }

        /// <summary>
        /// Defines that a given parameter was found at a given level during query building.
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="level"></param>
        public ParameterFoundAtLevel(ISqlParameter parameter,ParameterLevel level)
        {
            Parameter = parameter;
            Level = level;
        }

        /// <summary>
        /// Provides human readable description of the parameter and where it was found
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Parameter.ParameterName + " (At Level:" + Level + ")";
        }
    }
}
