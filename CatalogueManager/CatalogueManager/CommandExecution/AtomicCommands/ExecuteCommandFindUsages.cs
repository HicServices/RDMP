using System;
using System.Collections.Generic;
using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueManager.AggregationUIs;
using CatalogueManager.ItemActivation;
using CatalogueManager.ObjectVisualisation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.ChecksUI;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using Sharing.Dependency.Gathering;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandFindUsages : BasicUICommandExecution,IAtomicCommand
    {
        private GatheredObject _dependencies;

        public ExecuteCommandFindUsages(IActivateItems activator, DatabaseEntity o) : base(activator)
        {
            var gatherer = new Gatherer(activator.RepositoryLocator);
            if(!gatherer.CanGatherDependencies(o))
                SetImpossible("Object Type " + o.GetType() + " is not compatible with Gatherer");
            else
                _dependencies = gatherer.GatherDependencies(o);
        }

        public override void Execute()
        {
            base.Execute();
            
            var cmd  = new ExecuteCommandViewDependencies(_dependencies, new CatalogueObjectVisualisation(Activator.CoreIconProvider));
            cmd.Execute();
            
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return null;
        }
    }
}