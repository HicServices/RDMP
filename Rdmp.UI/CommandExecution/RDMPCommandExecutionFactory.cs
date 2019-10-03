// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Repositories.Construction;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.CommandExecution.Proposals;
using Rdmp.UI.Copying.Commands;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Checks;

namespace Rdmp.UI.CommandExecution
{
    public class RDMPCommandExecutionFactory : ICommandExecutionFactory
    {
        private readonly IActivateItems _activator;
        private Dictionary<ICommand, Dictionary<CachedDropTarget, ICommandExecution>> _cachedAnswers = new Dictionary<ICommand, Dictionary<CachedDropTarget, ICommandExecution>>();
        private object oLockCachedAnswers = new object();
        private List<ICommandExecutionProposal> _proposers = new List<ICommandExecutionProposal>();

        public RDMPCommandExecutionFactory(IActivateItems activator)
        {
            _activator = activator;

            foreach (Type proposerType in _activator.RepositoryLocator.CatalogueRepository.MEF.GetTypes<ICommandExecutionProposal>())
            {
                try
                {
                    ObjectConstructor constructor = new ObjectConstructor();
                    _proposers.Add((ICommandExecutionProposal)constructor.Construct(proposerType,activator));

                }
                catch (Exception ex)
                {
                    activator.GlobalErrorCheckNotifier.OnCheckPerformed(
                        new CheckEventArgs("Could not instantiate ICommandExecutionProposal '" + proposerType + "'",
                            CheckResult.Fail, ex));
                }
            }
        }

        public ICommandExecution Create(ICommand cmd, object targetModel,InsertOption insertOption = InsertOption.Default)
        {
            lock (oLockCachedAnswers)
            {
                CachedDropTarget proposition = new CachedDropTarget(targetModel, insertOption);

                //typically user might start a drag and then drag it all over the place so cache answers to avoid hammering database/loading donuts
                if (_cachedAnswers.ContainsKey(cmd))
                {
                    //if we already have a cached execution for the command and the target
                    if (_cachedAnswers[cmd].ContainsKey(proposition))
                        return _cachedAnswers[cmd][proposition];//return from cache
                }
                else
                    _cachedAnswers.Add(cmd, new Dictionary<CachedDropTarget, ICommandExecution>()); //novel command

                var result  = CreateNoCache(cmd, targetModel, insertOption);
                _cachedAnswers[cmd].Add(new CachedDropTarget(targetModel,insertOption), result);

                return result;
            }
        }

        private ICommandExecution CreateNoCache(ICommand cmd, object targetModel,InsertOption insertOption = InsertOption.Default)
        {
            ///////////////Catalogue or ambiguous Drop Targets ////////////////////////
            var targetCatalogueFolder = targetModel as CatalogueFolder;
            if (targetCatalogueFolder != null)
                return CreateWhenTargetIsFolder(cmd, targetCatalogueFolder);
            
            /////////////Table Info Drop Targets///////////////////////////////////
            var targetTableInfo = targetModel as TableInfo;
            if (targetTableInfo != null)
                return CreateWhenTargetIsATableInfo(cmd, targetTableInfo);

            //////////////////////Cohort Drop Targets//////////////////
            
            var targetJoinableCollectionNode = targetModel as JoinableCollectionNode;
            if (targetJoinableCollectionNode != null)
                return CreateWhenTargetIsJoinableCollectionNode(cmd,targetJoinableCollectionNode);

            ///////////////Data Loading Drop Targets ///////////////////

            var targetProcessTask = targetModel as ProcessTask;
            if (targetProcessTask != null)
                return CreateWhenTargetIsProcessTask(cmd, targetProcessTask, insertOption);
            
            /////////////Table Info Collection Drop Targets////////////////////

            var targetPreLoadDiscardedColumnsNode = targetModel as PreLoadDiscardedColumnsNode;
            if (targetPreLoadDiscardedColumnsNode != null)
                return CreateWhenTargetIsPreLoadDiscardedColumnsNode(cmd, targetPreLoadDiscardedColumnsNode);

            foreach (ICommandExecutionProposal proposals in _proposers.Where(p => p.IsCompatibleTarget(targetModel)))
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
            foreach (ICommandExecutionProposal proposals in _proposers.Where(p => p.IsCompatibleTarget(target)))
                proposals.Activate(target);
        }

        public bool CanActivate(object target)
        {
            return _proposers.Any(p => p.CanActivate(target));
        }
        
        private ICommandExecution CreateWhenTargetIsProcessTask(ICommand cmd, ProcessTask targetProcessTask, InsertOption insertOption)
        {
            var sourceProcessTaskCommand = cmd as ProcessTaskCommand;
            if (sourceProcessTaskCommand != null)
                return new ExecuteCommandReOrderProcessTask(_activator,sourceProcessTaskCommand, targetProcessTask, insertOption);

            return null;
        }


        private ICommandExecution CreateWhenTargetIsFolder(ICommand cmd, CatalogueFolder targetCatalogueFolder)
        {
            var sourceCatalogue = cmd as CatalogueCommand;
            var sourceManyCatalogues = cmd as ManyCataloguesCommand;
            var file = cmd as FileCollectionCommand;

            if (sourceCatalogue != null)
                return new ExecuteCommandPutCatalogueIntoCatalogueFolder(_activator, sourceCatalogue, targetCatalogueFolder);
            
            if (sourceManyCatalogues != null)
                return new ExecuteCommandPutCatalogueIntoCatalogueFolder(_activator, sourceManyCatalogues, targetCatalogueFolder);

            if(file != null)
                if(file.Files.Length == 1)
                {
                    var toReturn = new ExecuteCommandCreateNewCatalogueByImportingFile(_activator,file.Files[0]);
                    toReturn.TargetFolder = targetCatalogueFolder;
                    return toReturn;
                }

            return null;
        }

        private ICommandExecution CreateWhenTargetIsATableInfo(ICommand cmd, TableInfo targetTableInfo)
        {
            var sourceDataAccessCredentialsCommand = cmd as DataAccessCredentialsCommand;

            if (sourceDataAccessCredentialsCommand != null)
                return new ExecuteCommandUseCredentialsToAccessTableInfoData(_activator,sourceDataAccessCredentialsCommand, targetTableInfo);

            return null;
        }


        private ICommandExecution CreateWhenTargetIsJoinableCollectionNode(ICommand cmd, JoinableCollectionNode targetJoinableCollectionNode)
        {
            var sourceAggregateConfigurationCommand = cmd as AggregateConfigurationCommand;
            if(sourceAggregateConfigurationCommand != null)
                if (sourceAggregateConfigurationCommand.Aggregate.IsCohortIdentificationAggregate)
                    return new ExecuteCommandConvertAggregateConfigurationToPatientIndexTable(_activator,sourceAggregateConfigurationCommand, targetJoinableCollectionNode.Configuration);

            var sourceCatalogueCommand = cmd as CatalogueCommand;
            if (sourceCatalogueCommand != null)
                return new ExecuteCommandAddCatalogueToCohortIdentificationAsPatientIndexTable(_activator,sourceCatalogueCommand, targetJoinableCollectionNode.Configuration);

            return null;
        }


        private ICommandExecution CreateWhenTargetIsPreLoadDiscardedColumnsNode(ICommand cmd, PreLoadDiscardedColumnsNode targetPreLoadDiscardedColumnsNode)
        {
            var sourceColumnInfoCommand = cmd as ColumnInfoCommand;

            if(sourceColumnInfoCommand != null)
                return new ExecuteCommandCreateNewPreLoadDiscardedColumn(_activator,targetPreLoadDiscardedColumnsNode.TableInfo,sourceColumnInfoCommand);

            return null;
        }
    }
}
