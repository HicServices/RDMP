// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.Automation;
using CatalogueManager.CommandExecution.AtomicCommands.Sharing;
using CatalogueManager.SimpleDialogs.NavigateTo;
using CohortManager.CommandExecution.AtomicCommands;
using DataExportManager.CommandExecution.AtomicCommands;
using NUnit.Framework;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using Tests.Common;

namespace CatalogueLibraryTests.SourceCodeEvaluation
{
    public class RunUITests:DatabaseTests
    {
        private List<Type> allowedToBeIncompatible
            = new List<Type>(new[]
            {
                typeof(ExecuteCommandExportObjectsToFileUI),
                typeof(ExecuteCommandShow),
                typeof(ExecuteCommandSetDataAccessContextForCredentials),
                typeof(ExecuteCommandActivate),
                typeof(ExecuteCommandCreateNewExternalDatabaseServer),
                typeof(ExecuteCommandDelete),
                typeof(ExecuteCommandRename),
                typeof(ExecuteCommandShowKeywordHelp),
                typeof(ExecuteCommandCollapseChildNodes),
                typeof(ExecuteCommandExpandAllNodes),
                typeof(ExecuteCommandViewDependencies),
                typeof(ExecuteCommandViewCohortAggregateGraph),
                typeof(ExecuteCommandExecuteExtractionAggregateGraph),
                
                typeof(ExecuteCommandAddNewCatalogueItem),

                typeof(ExecuteCommandCreateNewFilter),
                typeof(ExecuteCommandCreateNewFilterFromCatalogue),
                typeof(ExecuteCommandViewSqlParameters),

                //requires a use case
                typeof(ExecuteCommandCreateNewPipeline),
                typeof(ExecuteCommandEditPipelineWithUseCase),

                typeof(ExecuteCommandExportLoggedDataToCsv),
                typeof(ExecuteCommandCopyRunCommandToClipboard),
                
                typeof(ExecuteCommandDisableOrEnable),
                typeof(ExecuteCommandRunDetached),
                
                typeof(ExecuteCommandShowXmlDoc),

            });

        [Test]
        public void AllCommandsCompatible()
        {
            List<Exception> ex;
            
            Console.WriteLine("Looking in" + typeof (ExecuteCommandCreateNewExtractableDataSetPackage).Assembly);
            Console.WriteLine("Looking in" + typeof(ExecuteCommandViewCohortAggregateGraph).Assembly);
            Console.WriteLine("Looking in" + typeof(ExecuteCommandUnpin).Assembly);
            
            allowedToBeIncompatible.AddRange(RunUI.GetIgnoredCommands());

            var notSupported = RepositoryLocator.CatalogueRepository.MEF.GetAllTypesFromAllKnownAssemblies(out ex)
                .Where(t=>typeof(IAtomicCommand).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface) //must be something we would normally expect to be a supported Type
                .Where(t => !RunUI.IsSupported(t)) //but for some reason isn't
                .Except(allowedToBeIncompatible) //and isn't a permissable one
                .ToArray();
            
            Assert.AreEqual(0,notSupported.Length,"The following commands were not compatible with RunUI:" + Environment.NewLine + string.Join(Environment.NewLine,notSupported.Select(t=>t.Name)));

            var supported = RepositoryLocator.CatalogueRepository.MEF.GetAllTypesFromAllKnownAssemblies(out ex).Where(RunUI.IsSupported).ToArray();

            Console.WriteLine("The following commands are supported:" + Environment.NewLine + string.Join(Environment.NewLine,supported.Select(cmd=>cmd.Name)));

        }
    }
}
