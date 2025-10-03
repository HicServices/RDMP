// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.Alter;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.Sharing;
using System;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandExecution;

public class TestCommandsAreSupported : UnitTests
{
    private CommandInvoker invoker;

    [OneTimeSetUp]
    public void Init()
    {
        var activator = GetActivator();
        invoker = new CommandInvoker(activator);
    }


    [TestCase(typeof(ExecuteCommandAlterColumnType))]
    [TestCase(typeof(ExecuteCommandAlterTableAddArchiveTrigger))]
    [TestCase(typeof(ExecuteCommandAlterTableCreatePrimaryKey))]
    [TestCase(typeof(ExecuteCommandAlterTableMakeDistinct))]
    [TestCase(typeof(ExecuteCommandAlterTableName))]
    [TestCase(typeof(ExecuteCommandCreateNewCatalogueByExecutingAnAggregateConfiguration))]
    [TestCase(typeof(ExecuteCommandCreateNewCatalogueByImportingExistingDataTable))]
    [TestCase(typeof(ExecuteCommandCreateNewCatalogueByImportingFile))]
    [TestCase(typeof(ExecuteCommandCreateNewCatalogueFromTableInfo))]
    [TestCase(typeof(ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration))]
    [TestCase(typeof(ExecuteCommandCreateNewCohortFromCatalogue))]
    [TestCase(typeof(ExecuteCommandCreateNewCohortFromFile))]
    [TestCase(typeof(ExecuteCommandCreateNewCohortFromTable))]
    [TestCase(typeof(ExecuteCommandImportAlreadyExistingCohort))]
    [TestCase(typeof(ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer))]
    [TestCase(typeof(ExecuteCommandAddCatalogueToCohortIdentificationAsPatientIndexTable))]
    [TestCase(typeof(ExecuteCommandAddCatalogueToCohortIdentificationSetContainer))]
    [TestCase(typeof(ExecuteCommandAddCatalogueToGovernancePeriod))]
    [TestCase(typeof(ExecuteCommandAddCohortSubContainer))]
    [TestCase(typeof(ExecuteCommandAddCohortToExtractionConfiguration))]
    [TestCase(typeof(ExecuteCommandAddDatasetsToConfiguration))]
    [TestCase(typeof(ExecuteCommandAddDimension))]
    [TestCase(typeof(ExecuteCommandAddExtractionProgress))]
    [TestCase(typeof(ExecuteCommandAddFavourite))]
    [TestCase(typeof(ExecuteCommandAddMissingParameters))]
    [TestCase(typeof(ExecuteCommandAddNewAggregateGraph))]
    [TestCase(typeof(ExecuteCommandAddNewCatalogueItem))]
    [TestCase(typeof(ExecuteCommandAddNewExtractionFilterParameterSet))]
    [TestCase(typeof(ExecuteCommandAddNewFilterContainer))]
    [TestCase(typeof(ExecuteCommandAddNewGovernanceDocument))]
    [TestCase(typeof(ExecuteCommandAddNewSupportingDocument))]
    [TestCase(typeof(ExecuteCommandAddNewSupportingSqlTable))]
    [TestCase(typeof(ExecuteCommandAddPackageToConfiguration))]
    [TestCase(typeof(ExecuteCommandAddParameter))]
    [TestCase(typeof(ExecuteCommandAddPipelineComponent))]
    [TestCase(typeof(ExecuteCommandAddPlugins))]
    [TestCase(typeof(ExecuteCommandAssociateCatalogueWithLoadMetadata))]
    [TestCase(typeof(ExecuteCommandAssociateCohortIdentificationConfigurationWithProject))]
    [TestCase(typeof(ExecuteCommandBulkImportTableInfos))]
    [TestCase(typeof(ExecuteCommandChangeExtractionCategory))]
    [TestCase(typeof(ExecuteCommandChangeLoadStage))]
    [TestCase(typeof(ExecuteCommandCheck))]
    [TestCase(typeof(ExecuteCommandChooseCohort))]
    [TestCase(typeof(ExecuteCommandClearQueryCache))]
    [TestCase(typeof(ExecuteCommandCloneCohortIdentificationConfiguration))]
    [TestCase(typeof(ExecuteCommandCloneExtractionConfiguration))]
    [TestCase(typeof(ExecuteCommandClonePipeline))]
    [TestCase(typeof(ExecuteCommandConfirmLogs))]
    [TestCase(typeof(ExecuteCommandConvertAggregateConfigurationToPatientIndexTable))]
    [TestCase(typeof(ExecuteCommandCreateLookup))]
    [TestCase(typeof(ExecuteCommandCreateNewANOTable))]
    [TestCase(typeof(ExecuteCommandCreateNewCacheProgress))]
    [TestCase(typeof(ExecuteCommandCreateNewClassBasedProcessTask))]
    [TestCase(typeof(ExecuteCommandCreateNewCohortIdentificationConfiguration))]
    [TestCase(typeof(ExecuteCommandCreateNewCohortStore))]
    [TestCase(typeof(ExecuteCommandCreateNewDataLoadDirectory))]
    [TestCase(typeof(ExecuteCommandCreateNewEmptyCatalogue))]
    [TestCase(typeof(ExecuteCommandCreateNewExternalDatabaseServer))]
    [TestCase(typeof(ExecuteCommandCreateNewExtractableDataSetPackage))]
    [TestCase(typeof(ExecuteCommandCreateNewExtractionConfigurationForProject))]
    [TestCase(typeof(ExecuteCommandCreateNewFileBasedProcessTask))]
    [TestCase(typeof(ExecuteCommandCreateNewFilter))]
    [TestCase(typeof(ExecuteCommandCreateNewGovernancePeriod))]
    [TestCase(typeof(ExecuteCommandCreateNewLoadMetadata))]
    [TestCase(typeof(ExecuteCommandCreateNewLoadProgress))]
    [TestCase(typeof(ExecuteCommandCreateNewPermissionWindow))]
    [TestCase(typeof(ExecuteCommandCreateNewRemoteRDMP))]
    [TestCase(typeof(ExecuteCommandCreateNewStandardRegex))]
    [TestCase(typeof(ExecuteCommandCreatePrivateKey))]
    [TestCase(typeof(ExecuteCommandDelete))]
    [TestCase(typeof(ExecuteCommandDeprecate))]
    [TestCase(typeof(ExecuteCommandDescribe))]
    [TestCase(typeof(ExecuteCommandDisableOrEnable))]
    [TestCase(typeof(ExecuteCommandExecuteAggregateGraph))]
    [TestCase(typeof(ExecuteCommandExportLoggedDataToCsv))]
    [TestCase(typeof(ExecuteCommandExportObjectsToFile))]
    [TestCase(typeof(ExecuteCommandExportPlugins))]
    [TestCase(typeof(ExecuteCommandExtractMetadata))]
    [TestCase(typeof(ExecuteCommandFreezeCohortIdentificationConfiguration))]
    [TestCase(typeof(ExecuteCommandFreezeExtractionConfiguration))]
    [TestCase(typeof(ExecuteCommandGenerateReleaseDocument))]
    [TestCase(typeof(ExecuteCommandGuessAssociatedColumns))]
    [TestCase(typeof(ExecuteCommandImportCatalogueItemDescription))]
    [TestCase(typeof(ExecuteCommandImportCatalogueItemDescriptions))]
    [TestCase(typeof(ExecuteCommandImportCohortIdentificationConfiguration))]
    [TestCase(typeof(ExecuteCommandImportFilterContainerTree))]
    [TestCase(typeof(ExecuteCommandImportTableInfo))]
    [TestCase(typeof(ExecuteCommandLinkCatalogueItemToColumnInfo))]
    [TestCase(typeof(ExecuteCommandList))]
    [TestCase(typeof(ExecuteCommandListSupportedCommands))]
    [TestCase(typeof(ExecuteCommandListUserSettings))]
    [TestCase(typeof(ExecuteCommandMakeCatalogueItemExtractable))]
    [TestCase(typeof(ExecuteCommandMakeCatalogueProjectSpecific))]
    [TestCase(typeof(ExecuteCommandMakePatientIndexTableIntoRegularCohortIdentificationSetAgain))]
    [TestCase(typeof(ExecuteCommandMakeProjectSpecificCatalogueNormalAgain))]
    [TestCase(typeof(ExecuteCommandMergeCohortIdentificationConfigurations))]
    [TestCase(typeof(ExecuteCommandMoveAggregateIntoContainer))]
    [TestCase(typeof(ExecuteCommandMoveCohortAggregateContainerIntoSubContainer))]
    [TestCase(typeof(ExecuteCommandMoveContainerIntoContainer))]
    [TestCase(typeof(ExecuteCommandMoveFilterIntoContainer))]
    [TestCase(typeof(ExecuteCommandNewObject))]
    [TestCase(typeof(ExecuteCommandOverrideRawServer))]
    [TestCase(typeof(ExecuteCommandPrunePlugin))]
    [TestCase(typeof(ExecuteCommandQueryPlatformDatabase))]
    [TestCase(typeof(ExecuteCommandRefreshBrokenCohorts))]
    [TestCase(typeof(ExecuteCommandRename))]
    [TestCase(typeof(ExecuteCommandResetExtractionProgress))]
    [TestCase(typeof(ExecuteCommandRunSupportingSql))]
    [TestCase(typeof(ExecuteCommandScriptTable))]
    [TestCase(typeof(ExecuteCommandScriptTables))]
    [TestCase(typeof(ExecuteCommandSet))]
    [TestCase(typeof(ExecuteCommandSetAggregateDimension))]
    [TestCase(typeof(ExecuteCommandSetArgument))]
    [TestCase(typeof(ExecuteCommandSetAxis))]
    [TestCase(typeof(ExecuteCommandSetContainerOperation))]
    [TestCase(typeof(ExecuteCommandSetExtractionIdentifier))]
    [TestCase(typeof(ExecuteCommandSetFilterTreeShortcut))]
    [TestCase(typeof(ExecuteCommandSetGlobalDleIgnorePattern))]
    [TestCase(typeof(ExecuteCommandSetIgnoredColumns))]
    [TestCase(typeof(ExecuteCommandSetPermissionWindow))]
    [TestCase(typeof(ExecuteCommandSetPivot))]
    [TestCase(typeof(ExecuteCommandSetProjectExtractionDirectory))]
    [TestCase(typeof(ExecuteCommandSetQueryCachingDatabase))]
    [TestCase(typeof(ExecuteCommandSetUserSetting))]
    [TestCase(typeof(ExecuteCommandShow))]
    [TestCase(typeof(ExecuteCommandShowRelatedObject))]
    [TestCase(typeof(ExecuteCommandSimilar))]
    [TestCase(typeof(ExecuteCommandSyncTableInfo))]
    [TestCase(typeof(ExecuteCommandUnfreezeExtractionConfiguration))]
    [TestCase(typeof(ExecuteCommandUnMergeCohortIdentificationConfiguration))]
    [TestCase(typeof(ExecuteCommandUseCredentialsToAccessTableInfoData))]
    [TestCase(typeof(ExecuteCommandViewData))]
    [TestCase(typeof(ExecuteCommandViewExtractionSql))]
    [TestCase(typeof(ExecuteCommandViewFilterMatchData))]
    [TestCase(typeof(ExecuteCommandViewLogs))]
    [TestCase(typeof(ExecuteCommandExportInDublinCoreFormat))]
    [TestCase(typeof(ExecuteCommandImportCatalogueDescriptionsFromShare))]
    [TestCase(typeof(ExecuteCommandImportDublinCoreFormat))]
    [TestCase(typeof(ExecuteCommandImportFilterDescriptionsFromShare))]
    [TestCase(typeof(ExecuteCommandImportShareDefinitionList))]
    // [TestCase(typeof(ExecuteCommandSetDataAccessContextForCredentials))] // Not currently CLI compatible
    public void TestIsSupported(Type t)
    {
        Assert.That(invoker.WhyCommandNotSupported(t), Is.Null, $"Type {t} was not supported by CommandInvoker");
    }
}