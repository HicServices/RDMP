// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandRedactCHIsFromCatalogue : BasicCommandExecution, IAtomicCommand
{

    private readonly ICatalogue _catalogue;
    private readonly IBasicActivateItems _activator;
    public int redactionCount = 0;
    private readonly string _allowListLocation = "";
    private CHIRedactionHelpers redactionHelper;

    public ExecuteCommandRedactCHIsFromCatalogue(IBasicActivateItems activator, [DemandsInitialization("The catalogue to search")] ICatalogue catalogue, string allowListLocation = null) : base(activator)
    {
        _catalogue = catalogue;
        _activator = activator;
        _allowListLocation = allowListLocation;
        redactionHelper = new CHIRedactionHelpers(activator, catalogue);
    }

    public override void Execute()
    {
        base.Execute();
        var cmd = new ExecuteCommandIdentifyCHIInCatalogue(_activator, _catalogue, false, _allowListLocation);
        cmd.Execute();
        DataTable results = cmd.foundChis;
        foreach(DataRow row in results.Rows)
        {
            redactionCount++;
            try
            {
                redactionHelper.Redact(row);
            }
            catch { 
                //todo some sort of warning
            }
        }
        results.Dispose();
    }
}
