using System.Collections.Generic;
using System.Linq;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;
using DataExportLibrary.Interfaces.ExtractionTime.UserPicks;

namespace DataExportLibrary.ExtractionTime.UserPicks
{
    /// <summary>
    /// Ostensibly data extraction is simple: 1 Extraction Configuration, x datasets + 1 cohort.  In practice there are bundled Lookup tables, SupportingDocuments
    /// for each dataset, Global SupportingDocuments and even Custom Cohort Data.  The user doesn't nessesarily want to extract everything all the time.  
    /// Bundles are the collection classes for recording what subset of an ExtractionConfiguration should be run.
    /// </summary>
    public abstract class Bundle
    {
        public Dictionary<object, ExtractCommandState> States { get; private set; }

        public object[] Contents
        {
            get { return States.Keys.ToArray(); }
        }
        
        protected Bundle(object[] finalObjectsDoNotAddToThisLater)
        {
            //Add states for all objects
            States = new Dictionary<object, ExtractCommandState>();

            foreach (object o in finalObjectsDoNotAddToThisLater)
                States.Add(o, ExtractCommandState.NotLaunched);
        }
        
        public void SetAllStatesTo(ExtractCommandState state)
        {
            foreach (var k in States.Keys.ToArray())
                States[k] = state;
        }

        public void DropContent(object toDrop)
        {
            //remove the state information
            States.Remove(toDrop);

            //tell child to remove the object too
            OnDropContent(toDrop);
        }
        protected abstract void OnDropContent(object toDrop);
    }
}
