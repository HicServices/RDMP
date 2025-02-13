// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.CommandExecution.Proposals;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.CommandExecution;

public class RDMPCommandExecutionFactory : ICommandExecutionFactory
{
    private readonly IActivateItems _activator;
    private readonly Dictionary<ICombineToMakeCommand, Dictionary<CachedDropTarget, ICommandExecution>> _cachedAnswers = new();
    private readonly Lock _oLockCachedAnswers = new();
    private readonly List<ICommandExecutionProposal> _proposers = [];

    public RDMPCommandExecutionFactory(IActivateItems activator)
    {
        _activator = activator;

        foreach (var proposerType in MEF.GetTypes<ICommandExecutionProposal>())
            try
            {
                _proposers.Add((ICommandExecutionProposal)ObjectConstructor.Construct(proposerType, activator));
            }
            catch (Exception ex)
            {
                activator.GlobalErrorCheckNotifier.OnCheckPerformed(
                    new CheckEventArgs($"Could not instantiate ICommandExecutionProposal '{proposerType}'",
                        CheckResult.Fail, ex));
            }
    }

    public ICommandExecution Create(ICombineToMakeCommand cmd, object targetModel,
        InsertOption insertOption = InsertOption.Default)
    {
        lock (_oLockCachedAnswers)
        {
            var proposition = new CachedDropTarget(targetModel, insertOption);

            //typically user might start a drag and then drag it all over the place so cache answers to avoid hammering database/loading donuts
            if (_cachedAnswers.TryGetValue(cmd, out var cacheLine))
            {
                //if we already have a cached execution for the command and the target
                if (cacheLine.TryGetValue(proposition, out var hit))
                    return hit; //return from cache
            }
            else
            {
                _cachedAnswers.Add(cmd,
                    cacheLine = new Dictionary<CachedDropTarget, ICommandExecution>()); //novel command
            }

            var result = CreateNoCache(cmd, targetModel, insertOption);
            cacheLine.Add(new CachedDropTarget(targetModel, insertOption), result);

            return result;
        }
    }

    private ICommandExecution CreateNoCache(ICombineToMakeCommand cmd, object targetModel,
        InsertOption insertOption = InsertOption.Default)
    {
        ///////////////Catalogue or ambiguous Drop Targets ////////////////////////
        if (targetModel is IFolderNode folder)
            return CreateWhenTargetIsFolder(cmd, folder);

        /////////////Table Info Drop Targets///////////////////////////////////
        if (targetModel is TableInfo targetTableInfo)
            return CreateWhenTargetIsATableInfo(cmd, targetTableInfo);

        //////////////////////Cohort Drop Targets//////////////////

        if (targetModel is JoinableCollectionNode targetJoinableCollectionNode)
            return CreateWhenTargetIsJoinableCollectionNode(cmd, targetJoinableCollectionNode);

        ///////////////Data Loading Drop Targets ///////////////////

        if (targetModel is ProcessTask targetProcessTask)
            return CreateWhenTargetIsProcessTask(cmd, targetProcessTask, insertOption);

        /////////////Table Info Collection Drop Targets////////////////////

        if (targetModel is PreLoadDiscardedColumnsNode targetPreLoadDiscardedColumnsNode)
            return CreateWhenTargetIsPreLoadDiscardedColumnsNode(cmd, targetPreLoadDiscardedColumnsNode);

        foreach (var proposals in _proposers.Where(p => p.IsCompatibleTarget(targetModel)))
        {
            var ce = proposals.ProposeExecution(cmd, targetModel, insertOption);
            if (ce != null)
                return ce;
        }

        //no valid combinations
        return null;
    }


    public void Activate(object target)
    {
        foreach (var proposals in _proposers.Where(p => p.IsCompatibleTarget(target)))
            proposals.Activate(target);
    }

    public bool CanActivate(object target)
    {
        return _proposers.Any(p => p.CanActivate(target));
    }

    private ICommandExecution CreateWhenTargetIsProcessTask(ICombineToMakeCommand cmd, ProcessTask targetProcessTask,
        InsertOption insertOption) =>
        cmd is ProcessTaskCombineable sourceProcessTaskCommand
            ? new ExecuteCommandReOrderProcessTask(_activator, sourceProcessTaskCommand, targetProcessTask,
                insertOption)
            : (ICommandExecution)null;


    private ICommandExecution CreateWhenTargetIsFolder(ICombineToMakeCommand cmd, IFolderNode targetFolder)
    {
        return cmd switch
        {
            IHasFolderCombineable sourceFolderable => new ExecuteCommandPutIntoFolder(_activator, sourceFolderable,
                targetFolder.FullName),
            ManyCataloguesCombineable sourceManyCatalogues => new ExecuteCommandPutIntoFolder(_activator,
                sourceManyCatalogues, targetFolder.FullName),
            FileCollectionCombineable file when file.Files.Length == 1 => new
                ExecuteCommandCreateNewCatalogueByImportingFileUI(_activator, file.Files[0])
            {
                TargetFolder = targetFolder.FullName
            },
            _ => null
        };
    }

    private ICommandExecution CreateWhenTargetIsATableInfo(ICombineToMakeCommand cmd, TableInfo targetTableInfo) =>
        cmd is DataAccessCredentialsCombineable sourceDataAccessCredentialsCombineable
            ? new ExecuteCommandUseCredentialsToAccessTableInfoData(_activator,
                sourceDataAccessCredentialsCombineable.DataAccessCredentials, targetTableInfo)
            : (ICommandExecution)null;


    private ICommandExecution CreateWhenTargetIsJoinableCollectionNode(ICombineToMakeCommand cmd,
        JoinableCollectionNode targetJoinableCollectionNode)
    {
        if (cmd is AggregateConfigurationCombineable sourceAggregateConfigurationCombineable)
            if (sourceAggregateConfigurationCombineable.Aggregate.IsCohortIdentificationAggregate)
                return new ExecuteCommandConvertAggregateConfigurationToPatientIndexTable(_activator,
                    sourceAggregateConfigurationCombineable, targetJoinableCollectionNode.Configuration);

        return cmd is CatalogueCombineable sourceCatalogueCombineable
            ? new ExecuteCommandAddCatalogueToCohortIdentificationAsPatientIndexTable(_activator,
                sourceCatalogueCombineable, targetJoinableCollectionNode.Configuration)
            : (ICommandExecution)null;
    }


    private ICommandExecution CreateWhenTargetIsPreLoadDiscardedColumnsNode(ICombineToMakeCommand cmd,
        PreLoadDiscardedColumnsNode targetPreLoadDiscardedColumnsNode) =>
        cmd is ColumnInfoCombineable sourceColumnInfoCombineable
            ? new ExecuteCommandCreateNewPreLoadDiscardedColumn(_activator, targetPreLoadDiscardedColumnsNode.TableInfo,
                sourceColumnInfoCombineable)
            : (ICommandExecution)null;
}