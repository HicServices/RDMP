using System;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.Settings;

namespace CatalogueManager.Tutorials
{
    public class Tutorial
    {
        public readonly ICommandExecution CommandExecution;

        public string Name { get; set; }
        public Guid Guid { get; set; }
        public Type CommandType { get; private set; }

        public bool UserHasSeen
        {
            get { return UserSettings.GetTutorialDone(Guid); }
            set {  UserSettings.SetTutorialDone(Guid,value); }
        }

        public Tutorial(string name, ICommandExecution commandExecutionExecution, Guid guid)
        {
            CommandExecution = commandExecutionExecution;
            Name = name;
            Guid = guid;
            CommandType = commandExecutionExecution.GetType();
        }

    }
}