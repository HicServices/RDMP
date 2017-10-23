using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueLibrary.Repositories.Construction;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Nodes;
using RDMPObjectVisualisation.Copying;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.Proposals;
using ReusableUIComponents.TreeHelper;
using ScintillaNET;

namespace CatalogueManager.CommandExecution
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
            
            var targetcatalogueItem = targetModel as CatalogueItem;
            if(targetcatalogueItem != null)
                return CreateWhenTargetIsCatalogueItem(cmd, targetcatalogueItem);

            // AND / OR - in data export or aggregation
            var targetContainer = targetModel as IContainer;
            if (targetContainer != null)
                return CreateWhenTargetIsContainer(cmd,targetContainer);

            /////////////Table Info Drop Targets///////////////////////////////////
            var targetTableInfo = targetModel as TableInfo;
            if (targetTableInfo != null)
                return CreateWhenTargetIsATableInfo(cmd, targetTableInfo);

            //////////////////////Cohort Drop Targets//////////////////
            
            var targetJoinableCollectionNode = targetModel as JoinableCollectionNode;
            if (targetJoinableCollectionNode != null)
                return CreateWhenTargetIsJoinableCollectionNode(cmd,targetJoinableCollectionNode);

            ////////////////Data Export Drop Targets ////////////////

            var targetExtractionConfiguration = targetModel as ExtractionConfiguration;
            if (targetExtractionConfiguration != null)
                return CreateWhenTargetIsExtractionConfiguration(cmd, targetExtractionConfiguration);

            var targetExtractableDatasetsNode = targetModel as ExtractableDataSetsNode;
            if (targetExtractableDatasetsNode != null)
                return CreateWhenTargetIsExtractableDataSetsNode(cmd,targetExtractableDatasetsNode);
            
            ///////////////Data Loading Drop Targets ///////////////////

            var targetProcessTask = targetModel as ProcessTask;
            if (targetProcessTask != null)
                return CreateWhenTargetIsProcessTask(cmd, targetProcessTask, insertOption);

            var targetStage = targetModel as LoadStageNode;
            if (targetStage != null)
                return CreateWhenTargetIsLoadStage(cmd, targetStage);

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

        private ICommandExecution CreateWhenTargetIsLoadStage(ICommand cmd, LoadStageNode targetStage)
        {
            var sourceProcessTaskCommand = cmd as ProcessTaskCommand;
            if (sourceProcessTaskCommand != null)
                return new ExecuteCommandChangeLoadStage(_activator, sourceProcessTaskCommand, targetStage);

            return null;

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

        
        private ICommandExecution CreateWhenTargetIsCatalogueItem(ICommand cmd, CatalogueItem targetcatalogueItem)
        {
            var sourceColumnInfo = cmd as ColumnInfoCommand;

            if (sourceColumnInfo != null && targetcatalogueItem != null)
                return new ExecuteCommandLinkCatalogueItemToColumnInfo(_activator, sourceColumnInfo, targetcatalogueItem);

            return null;
        }

        private ICommandExecution CreateWhenTargetIsATableInfo(ICommand cmd, TableInfo targetTableInfo)
        {
            var sourceDataAccessCredentialsCommand = cmd as DataAccessCredentialsCommand;

            if (sourceDataAccessCredentialsCommand != null)
                return new ExecuteCommandUseCredentialsToAccessTableInfoData(_activator,sourceDataAccessCredentialsCommand, targetTableInfo);

            return null;
        }

        private ICommandExecution CreateWhenTargetIsContainer(ICommand cmd, IContainer targetContainer)
        {
            var sourceFilterCommand = cmd as FilterCommand;

            //drag a filter into a container
            if (sourceFilterCommand != null)
            {
                //if filter is already in the target container
                if (sourceFilterCommand.ImmediateContainerIfAny.Equals(targetContainer))
                    return null;

                //if the target container is one that is part of the filters tree then it's a move
                if (sourceFilterCommand.AllContainersInEntireTreeFromRootDown.Contains(targetContainer))
                    return new ExecuteCommandMoveFilterIntoContainer(_activator, sourceFilterCommand, targetContainer);
                
                //otherwise it's an import    

                //so instead lets let them create a new copy (possibly including changing the type e.g. importing a master
                //filter into a data export AND/OR container
                return new ExecuteCommandImportNewCopyOfFilterIntoContainer(_activator, sourceFilterCommand,targetContainer);
                
            }

            
            var sourceContainerCommand = cmd as ContainerCommand;
            
            //drag a container into another container
            if (sourceContainerCommand != null)
            {
                //if the source and target are the same container
                if (sourceContainerCommand.Container.Equals(targetContainer))
                    return null;

                //is it a movement within the current container tree
                if (sourceContainerCommand.AllContainersInEntireTreeFromRootDown.Contains(targetContainer))
                    return new ExecuteCommandMoveContainerIntoContainer(_activator, sourceContainerCommand, targetContainer);
            }
            
            return null;
        }


        private ICommandExecution CreateWhenTargetIsExtractionConfiguration(ICommand cmd, ExtractionConfiguration targetExtractionConfiguration)
        {
            //user is trying to set the cohort of the configuration
            var sourceExtractableCohortComand = cmd as ExtractableCohortCommand;

            if (sourceExtractableCohortComand != null)
                return new ExecuteCommandAddCohortToExtractionConfiguration(_activator, sourceExtractableCohortComand,targetExtractionConfiguration);

            //user is trying to add datasets to a configuration
            var sourceExtractableDataSetCommand = cmd as ExtractableDataSetCommand;

            if (sourceExtractableDataSetCommand != null)
                return new ExecuteCommandAddDatasetsToConfiguration(_activator,sourceExtractableDataSetCommand,targetExtractionConfiguration);

            return null;
        }

        private ICommandExecution CreateWhenTargetIsExtractableDataSetsNode(ICommand cmd, ExtractableDataSetsNode targetExtractableDataSetsNode)
        {
            //user is trying to make a Catalogue extractable?
            var sourceCatalogueCommand = cmd as CatalogueCommand;

            if (sourceCatalogueCommand != null)
                return new ExecuteCommandMakeCatalogueExtractable(_activator,sourceCatalogueCommand, targetExtractableDataSetsNode);

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
