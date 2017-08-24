using System.Collections.Generic;

namespace MapsDirectlyToDatabaseTable.Revertable
{
    public class RevertableObjectReport
    {
        public ChangeDescription Evaluation { get; set; }
        public List<RevertablePropertyDifference> Differences { get; private set; }

        public RevertableObjectReport()
        {
            Differences = new List<RevertablePropertyDifference>();
        }
        
    }
}