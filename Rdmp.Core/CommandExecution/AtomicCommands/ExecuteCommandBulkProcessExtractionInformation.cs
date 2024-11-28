// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.


using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories.Construction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandBulkProcessExtractionInformation : BasicCommandExecution
{
    private List<ExtractionInformation> _extractionInformations = new List<ExtractionInformation>();
    private string _newSelectQuery;

    public ExecuteCommandBulkProcessExtractionInformation(IBasicActivateItems activator, string newSelectQuery, IMapsDirectlyToDatabaseTable[] eiIDs) : base(activator)
    {
        foreach (var eiID in eiIDs)
        {
            var ei = activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<ExtractionInformation>("ID", eiID.ID).FirstOrDefault();
            if (ei is not null)
            {
                _extractionInformations.Add(ei);
            }
        }
        _newSelectQuery = newSelectQuery;
    }

    public override void Execute()
    {
        base.Execute();
        foreach (var extractionInformation in _extractionInformations)
        {
            var defaultRunTimeName = extractionInformation.CatalogueItem.ColumnInfo.GetFullyQualifiedName();
            var alias = extractionInformation.CatalogueItem.ColumnInfo.GetRuntimeName();
            Console.WriteLine(defaultRunTimeName);
            var newSQL = _newSelectQuery.Replace("$RTN", defaultRunTimeName) + $" AS {alias}";
            extractionInformation.SelectSQL = newSQL;
            extractionInformation.Alias = alias;
            extractionInformation.SaveToDatabase();
        }
    }
}
