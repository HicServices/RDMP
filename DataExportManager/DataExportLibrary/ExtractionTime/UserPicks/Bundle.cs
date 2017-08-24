using System.Collections.Generic;
using System.Linq;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;
using DataExportLibrary.Interfaces.ExtractionTime.UserPicks;

namespace DataExportLibrary.ExtractionTime.UserPicks
{
    /// <summary>
    /// Tracks the states of a number of objects that reflect an extractable set of objects
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
