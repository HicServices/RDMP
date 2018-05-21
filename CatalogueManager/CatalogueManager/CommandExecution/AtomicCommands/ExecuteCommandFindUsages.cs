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
        private Gatherer _gatherer;
        private DatabaseEntity _o;

        public ExecuteCommandFindUsages(IActivateItems activator, DatabaseEntity o) : base(activator)
        {
            _gatherer = new Gatherer(activator.RepositoryLocator);
            if(!_gatherer.CanGatherDependencies(o))
                SetImpossible("Object Type " + o.GetType() + " is not compatible with Gatherer");
            _o = o;
        }

        public override void Execute()
        {
            base.Execute();

            var dependencies = _gatherer.GatherDependencies(_o);
            
            var cmd  = new ExecuteCommandViewDependencies(dependencies, new CatalogueObjectVisualisation(Activator.CoreIconProvider));
            cmd.Execute();
            
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return null;
        }
    }
}