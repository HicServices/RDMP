// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandDelete : BasicUICommandExecution, IAtomicCommand
    {
        private readonly IList<IDeleteable> _deletables;

        public ExecuteCommandDelete(IActivateItems activator, IDeleteable deletable) : base(activator)
        {
            _deletables = new []{ deletable};
        }
        public ExecuteCommandDelete(IActivateItems activator, IList<IDeleteable> deletables) : base(activator)
        {
            _deletables = deletables;
        }
        public override void Execute()
        {
            base.Execute();
            
            if(_deletables.Count == 1)
                Activator.DeleteWithConfirmation(this, _deletables[0]);
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