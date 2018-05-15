using System.Collections.Generic;
using System.Linq;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;
using DataExportLibrary.ExtractionTime.UserPicks;

namespace DataExportManager.ProjectUI
{
    internal class ExtractCommandStateMonitor
    {
        Dictionary<IExtractCommand,ExtractCommandState> CommandStates = new Dictionary<IExtractCommand, ExtractCommandState>();
        private Dictionary<IExtractCommand, Dictionary<object, ExtractCommandState>> CommandSubStates = new Dictionary<IExtractCommand, Dictionary<object, ExtractCommandState>>();

        private Dictionary<object, ExtractCommandState> GlobalsStates = new Dictionary<object, ExtractCommandState>();

        public bool Contains(IExtractCommand cmd)
        {
            return CommandStates.ContainsKey(cmd);
        }

        public void Add(IExtractDatasetCommand cmd)
        {
            CommandStates.Add(cmd,cmd.State);
            CommandSubStates.Add(cmd,cmd.DatasetBundle.States);
        }

        public void SaveState(IExtractDatasetCommand cmd)
        {
            CommandStates[cmd] = cmd.State;

            var toUpdateSubstates = CommandSubStates[cmd];

            foreach (KeyValuePair<object, ExtractCommandState> substate in cmd.DatasetBundle.States.ToArray())
            {
                if(!toUpdateSubstates.ContainsKey(substate.Key))
                    toUpdateSubstates.Add(substate.Key,substate.Value);

                toUpdateSubstates[substate.Key] = substate.Value;
            }
        }
        
        public IEnumerable<object> GetAllChangedObjects(IExtractDatasetCommand cmd)
        {
            if (CommandStates[cmd] != cmd.State)
                yield return cmd;

            foreach (var substate in cmd.DatasetBundle.States.ToArray())
                if (CommandSubStates[cmd][substate.Key] != substate.Value)
                    yield return substate.Key;
        }
        public void SaveState(GlobalsBundle globals)
        {
            foreach (var gkvp in globals.States)
            {
                if(!GlobalsStates.ContainsKey(gkvp.Key))
                    GlobalsStates.Add(gkvp.Key,gkvp.Value);

                GlobalsStates[gkvp.Key] = gkvp.Value;
            }
        }

        public IEnumerable<object> GetAllChangedObjects(GlobalsBundle globals)
        {
            foreach (var gkvp in globals.States)
            {
                if (!GlobalsStates.ContainsKey(gkvp.Key))
                {
                    GlobalsStates.Add(gkvp.Key, gkvp.Value);
                    yield return gkvp.Key;//new objects also are returned as changed
                }
                else
                //State has changed since last save
                if (GlobalsStates[gkvp.Key] != gkvp.Value)
                    yield return gkvp.Key;
            }
            
        }
    }
}