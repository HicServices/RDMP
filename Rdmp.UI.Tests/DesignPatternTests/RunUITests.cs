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
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Tests.Common;

namespace Rdmp.UI.Tests.DesignPatternTests;

public class RunUITests : DatabaseTests
{
    private List<Type> allowedToBeIncompatible
        = new(new[]
        {
            typeof(ExecuteCommandShow),
            typeof(ExecuteCommandSetDataAccessContextForCredentials),
            typeof(ExecuteCommandActivate),
            typeof(ExecuteCommandCreateNewExternalDatabaseServer),
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
            typeof(ExecuteCommandAddToSession),
            typeof(ExecuteCommandDeletePlugin),
            typeof(ExecuteCommandPerformRegexRedactionOnCatalogue)
        });

    [Test]
    public void AllCommandsCompatible()
    {
        var uiTests = new UITests();
        var activator = new TestActivateItems(uiTests, new MemoryDataExportRepository());

        allowedToBeIncompatible.AddRange(activator.GetIgnoredCommands());

        var commandCaller = new CommandInvoker(activator);

        Assert.That(commandCaller.WhyCommandNotSupported(typeof(ExecuteCommandDelete)) is null);

        var notSupported = MEF.GetAllTypes()
            .Where(t => typeof(IAtomicCommand).IsAssignableFrom(t) && !t.IsAbstract &&
                        !t.IsInterface) //must be something we would normally expect to be a supported Type
            .Except(allowedToBeIncompatible) //and isn't a permissible one
            .Where(t => commandCaller.WhyCommandNotSupported(t) is not null) //but for some reason isn't
            .ToArray();

        Assert.That(notSupported, Is.Empty,
            "The following commands were not compatible with RunUI:" + Environment.NewLine +
            string.Join(Environment.NewLine, notSupported.Select(t => t.Name)));
    }

    [Test]
    public void Test_IsSupported_BasicActivator()
    {
        IBasicActivateItems basic = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet);

        var commandCaller = new CommandInvoker(basic);
        foreach (var t in new[]
                 {
                     typeof(ExecuteCommandDelete), typeof(ExecuteCommandList),
                     typeof(ExecuteCommandExportLoggedDataToCsv), typeof(TestCommandDiscoveredDatabase),
                     typeof(TestCommandLotsOfParameters), typeof(TestCommandTypeParameter)
                 })
        {
            var isSupported = commandCaller.WhyCommandNotSupported(t);
            Assert.That(isSupported is null, $"Unsupported type {t} due to {isSupported}");
        }
    }

    private class TestCommandDiscoveredDatabase : BasicCommandExecution
    {
        public TestCommandDiscoveredDatabase(IBasicActivateItems activator, DiscoveredDatabase _) : base(activator)
        {
        }
    }

    private class TestCommandLotsOfParameters : BasicCommandExecution
    {
        public TestCommandLotsOfParameters(IRDMPPlatformRepositoryServiceLocator _1, DiscoveredDatabase _2,
            DirectoryInfo _3) : base()
        {
        }
    }

    private class TestCommandTypeParameter : BasicCommandExecution
    {
        public TestCommandTypeParameter(IRDMPPlatformRepositoryServiceLocator _1, Type _2) : base()
        {
        }
    }
}