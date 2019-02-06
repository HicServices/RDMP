// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace ReusableLibraryCode.CommandExecution
{
    /// <summary>
    /// A potentially executable object.  Can be translated into an ICommandExecution by an ICommandExecutionFactory.  For example ICommand CatalogueCommand can 
    /// be translated into ExecuteCommandPutCatalogueIntoCatalogueFolder (an ICommandExecution) by combining it with a CatalogueFolder.  But you could equally
    /// turn it into an ExecuteCommandAddCatalogueToCohortIdentificationSetContainer (also an ICommandExecution) by combining it with a CohortAggregateContainer.
    /// 
    /// <para>ICommand should reflect a single object and contain all useful information discovered about the object so that the ICommandExecutionFactory can make a 
    /// good decision about what ICommandExecution to create as the user drags it about the place.</para>
    /// </summary>
    public interface ICommand
    {
        string GetSqlString();
    }
}
