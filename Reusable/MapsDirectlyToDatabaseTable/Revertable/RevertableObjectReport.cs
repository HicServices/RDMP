using System.Collections.Generic;

namespace MapsDirectlyToDatabaseTable.Revertable
{
    /// <summary>
    /// Summarises the differences (if any) between the Properties of an IRevertable object vs the corresponding currently saved database record.  Changes
    /// can be the result of local in memory changes the user has made but not saved yet or changes other users have made and saved since the IRevertable 
    /// was fetched.
    /// </summary>
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