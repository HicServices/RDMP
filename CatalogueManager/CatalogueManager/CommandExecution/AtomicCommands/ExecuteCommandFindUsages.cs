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
        private readonly ColumnInfo _columnInfo;
        private Gatherer _gatherer;

        public ExecuteCommandFindUsages(IActivateItems activator, ColumnInfo columnInfo) : base(activator)
        {
            _columnInfo = columnInfo;
            _gatherer = new Gatherer(activator.RepositoryLocator);
        }

        public override void Execute()
        {
            base.Execute();

            var dependencies = _gatherer.GatherDependencies(_columnInfo);
            
            var cmd  = new ExecuteCommandViewDependencies(dependencies, new CatalogueObjectVisualisation(Activator.CoreIconProvider));
            cmd.Execute();
            
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return null;
        }
    }
}