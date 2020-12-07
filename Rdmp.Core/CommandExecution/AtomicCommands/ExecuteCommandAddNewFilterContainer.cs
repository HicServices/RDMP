// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandAddNewFilterContainer : BasicCommandExecution
    {
        private IRootFilterContainerHost _host;
        private IContainer _container;

        public ExecuteCommandAddNewFilterContainer(IBasicActivateItems activator, IRootFilterContainerHost host):base(activator)
        {
            if(host.RootFilterContainer_ID != null)
                SetImpossible("There is already a root filter container on this object");
            
            if(host is AggregateConfiguration ac && ac.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID != null)
                SetImpossible("Aggregate is set to use another's filter container tree");

            _host = host;
        }
        public ExecuteCommandAddNewFilterContainer(IBasicActivateItems activator, IContainer container):base(activator)
        {
            _container = container;
        }
        public override void Execute()
        {
            base.Execute();
            
            var factory = _container?.GetFilterFactory() ?? _host.GetFilterFactory();
            var newContainer = factory.CreateNewContainer();
            
            if(_host != null)
            {
                _host.RootFilterContainer_ID = newContainer .ID;
                _host.SaveToDatabase();
            }
            else
            {
                _container.AddChild(newContainer);
            }
            

            Publish(_host ?? (IMapsDirectlyToDatabaseTable)newContainer);
            Emphasise(newContainer);
        }
    }
}