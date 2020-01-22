// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    /// <summary>
    /// Deletes objects out of the RDMP database
    /// </summary>
    public class ExecuteCommandDelete : BasicCommandExecution
    {
        private readonly IList<IDeleteable> _deletables;

        [UseWithObjectConstructor]
        public ExecuteCommandDelete(IBasicActivateItems activator, 
            [DemandsInitialization("The object you want to delete",Mandatory = true)]
            IDeleteable deletable) : this(activator,new []{ deletable})
        {
        }

        
        public ExecuteCommandDelete(IBasicActivateItems activator, IDeleteable[] deletables) : base(activator)
        {
            _deletables = deletables;

            if(_deletables.Any( d => d is CohortAggregateContainer c && c.IsRootContainer()))
                SetImpossible("Cannot delete root containers");
        }
        public override void Execute()
        {
            base.Execute();
            
            if(_deletables.Count == 1)
                BasicActivator.DeleteWithConfirmation(_deletables[0]);
            else
            if(_deletables.Count>1)
            {
                if(YesNo("Delete " + _deletables.Count + " Items?","Delete Items"))
                {
                    var publishMe = _deletables.OfType<DatabaseEntity>().First();

                    try
                    {
                        foreach (IDeleteable d in _deletables)
                            if (!(d is DatabaseEntity exists) || exists.Exists()) //don't delete stuff that doesn't exist!
                                d.DeleteInDatabase();
                    }
                    finally
                    {
                        if (publishMe != null)
                            try
                            {
                                Publish(publishMe);
                            }
                            catch(Exception ex)
                            {
                                GlobalError("Failed to publish after delete", ex);
                            }
                    }
                }
            }
        }
    }
}