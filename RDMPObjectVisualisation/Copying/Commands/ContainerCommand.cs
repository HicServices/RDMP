using System.Collections.Generic;
using CatalogueLibrary.Data;
using ReusableUIComponents.CommandExecution;

namespace RDMPObjectVisualisation.Copying.Commands
{
    public class ContainerCommand : ICommand
    {
        public IContainer Container { get; private set; }

        /// <summary>
        /// All the containers that are further down the container heirarchy from this Container.  This includes all children containers, their children and their children and so on. 
        /// </summary>
        public List<IContainer> AllSubContainersRecursive { get; private set; }

        /// <summary>
        /// All the containers that are in the current filter tree (includes the Root - which might be us btw and all children).  If Container is the Root then this property
        /// will be the same as AllSubContainersRecursive except that it will also include the Root
        /// </summary>
        public List<IContainer> AllContainersInEntireTreeFromRootDown { get; private set; }

        public ContainerCommand(IContainer container)
        {
            Container = container;
            AllSubContainersRecursive = Container.GetAllSubContainersRecursively();

            var root = Container.GetRootContainerOrSelf();
            AllContainersInEntireTreeFromRootDown = root.GetAllSubContainersRecursively();
            AllContainersInEntireTreeFromRootDown.Add(root);
        }
        
        public string GetSqlString()
        {
            return null;
        }
    }
}