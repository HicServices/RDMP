using System;
using System.Collections.Generic;
using System.Linq;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.QueryBuilding
{
    public class QueryBuildingException : Exception
    {
        public List<IMapsDirectlyToDatabaseTable> ProblemObjects {get;private set;}

        public QueryBuildingException(string message, IEnumerable<IMapsDirectlyToDatabaseTable> problemObjects,
            Exception innerException = null) : base(message, innerException)
        {
            ProblemObjects = new List<IMapsDirectlyToDatabaseTable>(problemObjects);
        }

        public QueryBuildingException(string message, Exception innerException) : base(message, innerException)
        {
            ProblemObjects = new List<IMapsDirectlyToDatabaseTable>();
        }

        public QueryBuildingException(string message):base (message)
        {
            ProblemObjects = new List<IMapsDirectlyToDatabaseTable>();
        }

        /// <summary>
        /// Type unsafe overload of the IEnumerable'IMapsDirectlyToDatabaseTable' constructor, objects that are not of type IMapsDirectlyToDatabaseTable will be ignored, use if you are slopy coding and have objects of interface type which might be concrete IMapsDirectlyToDatabaseTable objects or might be spontaneous objects or nulls! - oh yeah that's what this constructor does
        /// </summary>
        /// <param name="message"></param>
        /// <param name="problemObjects"></param>
        public QueryBuildingException(string message, params object[] problemObjects) : base(message)
        {
            ProblemObjects = new List<IMapsDirectlyToDatabaseTable>();
            ProblemObjects.AddRange(problemObjects.OfType<IMapsDirectlyToDatabaseTable>());
        }
    }
}
