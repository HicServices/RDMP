// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Linq;
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
        private int? _explicitOriginIDToImport;

        public ExecuteCommandImportAlreadyExistingCohort(IBasicActivateItems activator, ExternalCohortTable externalCohortTable):base(activator)
        {
            _externalCohortTable = externalCohortTable;
        }

        
        [UseWithObjectConstructor]
        public ExecuteCommandImportAlreadyExistingCohort(IBasicActivateItems activator, ExternalCohortTable externalCohortTable, int originIDToImport)  : this(activator,externalCohortTable)
        {
            _explicitOriginIDToImport = originIDToImport;
        }

        public override void Execute()
        {
            base.Execute();

            var ect = _externalCohortTable;

            if (ect == null)
            {
                var available = BasicActivator.RepositoryLocator.DataExportRepository.GetAllObjects<ExternalCohortTable>();
                if(!SelectOne(available,out ect,null,true))
                {
                    return;
                }   
            }


            var newId = _explicitOriginIDToImport ?? GetWhichCohortToImport(ect);

            
            if(newId.HasValue)
            {
                new ExtractableCohort(BasicActivator.RepositoryLocator.DataExportRepository, ect, newId.Value);
                Publish(ect);
            }
        }

        private int? GetWhichCohortToImport(ExternalCohortTable ect)
        {

            var available = ExtractableCohort.GetImportableCohortDefinitions(ect).ToArray();

            if(BasicActivator.SelectObject("Import Cohort",available, out CohortDefinition cd))
            {
                return cd.ID;
            }

            return null;
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.CohortAggregate, OverlayKind.Import);
        }
    }
}