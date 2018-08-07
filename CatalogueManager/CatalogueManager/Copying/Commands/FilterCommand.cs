using System.Collections.Generic;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.FilterImporting.Construction;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.TreeHelper;

namespace CatalogueManager.Copying.Commands
{
    public class FilterCommand : ICommand
    {
        public IFilter Filter { get; private set; }
        
        public IContainer ImmediateContainerIfAny { get; private set; }
        public IContainer RootContainerIfAny { get; private set; }

        public Catalogue SourceCatalogueIfAny { get; private set; }

        /// <summary>
        /// All the containers that are in the current filter tree (includes the Root).
        /// </summary>
        public List<IContainer> AllContainersInEntireTreeFromRootDown { get; private set; } 

        public FilterCommand(IFilter filter)
        {
            Filter = filter;

            FindContainers();

            if (ImmediateContainerIfAny != null)
                SourceCatalogueIfAny = ImmediateContainerIfAny.GetCatalogueIfAny();
            
        }
        
        private void FindContainers()
        {
            ImmediateContainerIfAny = Filter.FilterContainer;
            AllContainersInEntireTreeFromRootDown = new List<IContainer>();
            
            if(ImmediateContainerIfAny != null)
            {
                RootContainerIfAny = ImmediateContainerIfAny.GetRootContainerOrSelf();
                
                //so we can determine whether we are being draged into a new heirarchy tree (copy) or just being dragged around inside our own tree (move)
                AllContainersInEntireTreeFromRootDown.Add(RootContainerIfAny);
                AllContainersInEntireTreeFromRootDown.AddRange(RootContainerIfAny.GetAllSubContainersRecursively());
                
            }
        }

        

        public string GetSqlString()
        {
            return Filter.WhereSQL;
        }
    }
}