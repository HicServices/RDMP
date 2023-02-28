// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.Automation;
using Rdmp.Core.CommandExecution.AtomicCommands.Sharing;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.Repositories;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.UI.Tests.DesignPatternTests
{
    public class RunUITests:DatabaseTests
    {
        private List<Type> allowedToBeIncompatible
            = new List<Type>(new[]
            {
                typeof(ExecuteCommandShow),
                typeof(ExecuteCommandSetDataAccessContextForCredentials),
                typeof(ExecuteCommandActivate),
                typeof(ExecuteCommandCreateNewExternalDatabaseServer),
                typeof(ExecuteCommandShowTooltip),
                typeof(ExecuteCommandShowKeywordHelp),
                typeof(ExecuteCommandCollapseChildNodes),
                typeof(ExecuteCommandExpandAllNodes),
                typeof(ExecuteCommandViewCohortAggregateGraph),
                typeof(ExecuteCommandExecuteExtractionAggregateGraph),
                
                typeof(ExecuteCommandAddNewCatalogueItem),

                typeof(ExecuteCommandCreateNewFilter),

                //requires a use case
                typeof(ExecuteCommandCreateNewPipeline),
                typeof(ExecuteCommandEditPipelineWithUseCase),

                typeof(ExecuteCommandExportLoggedDataToCsv),
                typeof(ExecuteCommandGenerateRunCommand),
                typeof(ExecuteCommandRunDetached),


                typeof(ExecuteCommandShowXmlDoc),
                typeof(ImpossibleCommand),
     
typeof(ExecuteCommandChangeLoadStage),
typeof(ExecuteCommandReOrderProcessTask),
typeof(ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer),
typeof(ExecuteCommandAddCatalogueToCohortIdentificationSetContainer),
typeof(ExecuteCommandAddCohortToExtractionConfiguration),
typeof(ExecuteCommandAddDatasetsToConfiguration),
typeof(ExecuteCommandConvertAggregateConfigurationToPatientIndexTable),
typeof(ExecuteCommandMakePatientIndexTableIntoRegularCohortIdentificationSetAgain),
typeof(ExecuteCommandMoveAggregateIntoContainer),
typeof(ExecuteCommandMoveCohortAggregateContainerIntoSubContainer),
typeof(ExecuteCommandMoveContainerIntoContainer),
typeof(ExecuteCommandMoveFilterIntoContainer),
typeof(ExecuteCommandPutIntoFolder),
typeof(ExecuteCommandReOrderAggregate),
typeof(ExecuteCommandReOrderAggregateContainer),
typeof(ExecuteCommandUseCredentialsToAccessTableInfoData),
typeof(ExecuteCommandCreateLookup),
typeof(ExecuteCommandImportFilterDescriptionsFromShare),
typeof(ExecuteCommandSetArgument),
typeof(ExecuteCommandAddToSession)

            });

        [Test]
        public void AllCommandsCompatible()
        {
            Console.WriteLine("Looking in" + typeof (ExecuteCommandCreateNewExtractableDataSetPackage).Assembly);
            Console.WriteLine("Looking in" + typeof(ExecuteCommandViewCohortAggregateGraph).Assembly);
            Console.WriteLine("Looking in" + typeof(ExecuteCommandAddToSession).Assembly);

            var uiTests = new UITests();
            var activator = new TestActivateItems(uiTests, new MemoryDataExportRepository());
            activator.RepositoryLocator.CatalogueRepository.MEF = CatalogueRepository.MEF;

            allowedToBeIncompatible.AddRange(activator.GetIgnoredCommands());

            var commandCaller = new CommandInvoker(activator);
            
            Assert.IsTrue(commandCaller.IsSupported(typeof(ExecuteCommandDelete)));

            var notSupported = RepositoryLocator.CatalogueRepository.MEF.GetAllTypes()
                .Where(t=>typeof(IAtomicCommand).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface) //must be something we would normally expect to be a supported Type
                .Where(t => !commandCaller.IsSupported(t)) //but for some reason isn't
                .Except(allowedToBeIncompatible) //and isn't a permissable one
                .ToArray();
            
            Assert.AreEqual(0,notSupported.Length,"The following commands were not compatible with RunUI:" + Environment.NewLine + string.Join(Environment.NewLine,notSupported.Select(t=>t.Name)));

            var supported = RepositoryLocator.CatalogueRepository.MEF.GetAllTypes().Where(commandCaller.IsSupported).ToArray();

            Console.WriteLine("The following commands are supported:" + Environment.NewLine + string.Join(Environment.NewLine,supported.Select(cmd=>cmd.Name)));

        }

        [TestCase(typeof(ExecuteCommandDelete))]
        [TestCase(typeof(ExecuteCommandList))]
        [TestCase(typeof(ExecuteCommandExportLoggedDataToCsv))]
        [TestCase(typeof(TestCommand_DiscoveredDatabase))]
        [TestCase(typeof(TestCommand_LotsOfParameters))]
        [TestCase(typeof(TestCommand_TypeParameter))]
        public void Test_IsSupported_BasicActivator(Type t)
        {
            IBasicActivateItems basic = new ConsoleInputManager(RepositoryLocator,new ThrowImmediatelyCheckNotifier());

            var commandCaller = new CommandInvoker(basic);
            
            Assert.IsTrue(commandCaller.IsSupported(t));
        }

        private class TestCommand_DiscoveredDatabase:BasicCommandExecution
        {   
            public TestCommand_DiscoveredDatabase(IActivateItems activator,DiscoveredDatabase db):base(activator)
            {
                
            }
        }

        private class TestCommand_LotsOfParameters : BasicCommandExecution
        {
            public TestCommand_LotsOfParameters(IRDMPPlatformRepositoryServiceLocator repositoryLocator, DiscoveredDatabase databaseToCreateInto, DirectoryInfo projectDirectory):base()
            {
                
            }
        }

        private class TestCommand_TypeParameter : BasicCommandExecution
        {
            public TestCommand_TypeParameter(IRDMPPlatformRepositoryServiceLocator repositoryLocator, Type myType):base()
            {
                
            }
        }



    }

    
}
