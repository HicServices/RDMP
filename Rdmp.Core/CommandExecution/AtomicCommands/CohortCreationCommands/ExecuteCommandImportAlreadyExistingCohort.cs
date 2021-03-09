// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands
{
    public class ExecuteCommandImportAlreadyExistingCohort : BasicCommandExecution, IAtomicCommand
    {
        private readonly ExternalCohortTable _externalCohortTable;
        private readonly Func<int?> _existingCohortSelectorDelegate;
        private int? _explicitOriginIDToImport;

        private ExecuteCommandImportAlreadyExistingCohort(IBasicActivateItems activator, ExternalCohortTable externalCohortTable):base(activator)
        {
            _externalCohortTable = externalCohortTable;
        }

        public ExecuteCommandImportAlreadyExistingCohort(IBasicActivateItems activator, ExternalCohortTable externalCohortTable, Func<int?> existingCohortSelectorDelegate) : this(activator,externalCohortTable)
        {
            this._existingCohortSelectorDelegate = existingCohortSelectorDelegate;
        }
        
        [UseWithObjectConstructor]
        public ExecuteCommandImportAlreadyExistingCohort(IBasicActivateItems activator, ExternalCohortTable externalCohortTable, int originIDToImport)  : this(activator,externalCohortTable)
        {
            _explicitOriginIDToImport = originIDToImport;
        }

        public override void Execute()
        {
            base.Execute();
            
            var newId = _explicitOriginIDToImport ?? _existingCohortSelectorDelegate();

            if(newId.HasValue)
            {
                new ExtractableCohort(BasicActivator.RepositoryLocator.DataExportRepository, _externalCohortTable, newId.Value);
                Publish(_externalCohortTable);
            }
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.CohortAggregate, OverlayKind.Import);
        }
    }
}