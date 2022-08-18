// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Providers.Nodes.LoadMetadataNodes;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandChangeLoadStage : BasicCommandExecution
    {
        private readonly ProcessTask _sourceProcessTask;
        private readonly LoadStageNode _targetStage;
        
        [UseWithObjectConstructor]
        public ExecuteCommandChangeLoadStage(IBasicActivateItems activator, ProcessTask processTask, LoadStage stage) :
            this(activator, new ProcessTaskCombineable(processTask), new LoadStageNode(processTask.LoadMetadata,stage))
        {

        }
        public ExecuteCommandChangeLoadStage(IBasicActivateItems activator, ProcessTaskCombineable sourceProcessTaskCombineable, LoadStageNode targetStage) : base(activator)
        {
            _sourceProcessTask = sourceProcessTaskCombineable.ProcessTask;
            _targetStage = targetStage;

            if(sourceProcessTaskCombineable.ProcessTask.LoadMetadata_ID != targetStage.LoadMetadata.ID)
                SetImpossible("ProcessTask belongs to a different LoadMetadata");

            if (!ProcessTask.IsCompatibleStage(_sourceProcessTask.ProcessTaskType, _targetStage.LoadStage))
                SetImpossible("Task type '" + _sourceProcessTask.ProcessTaskType +"' cannot run in " + _targetStage.LoadStage);
        }

        public override void Execute()
        {
            base.Execute();

            _sourceProcessTask.LoadStage = _targetStage.LoadStage;
            _sourceProcessTask.SaveToDatabase();
            Publish(_sourceProcessTask.LoadMetadata);
        }
    }
}