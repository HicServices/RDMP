using System;
using ReusableUIComponents.Copying;

namespace CatalogueManager.Tutorials
{
    public class Tutorial
    {
        public readonly ICommandExecution CommandExecution;

        public Tutorial(string name, ICommandExecution commandExecutionExecution)
        {
            CommandExecution = commandExecutionExecution;
            Name = name;
        }

        public string Name { get; set; }
    }
}