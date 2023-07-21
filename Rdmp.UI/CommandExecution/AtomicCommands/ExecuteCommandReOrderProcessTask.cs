// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

internal class ExecuteCommandReOrderProcessTask : BasicUICommandExecution
{
    private readonly ProcessTask _targetProcessTask;
    private readonly InsertOption _insertOption;
    private ProcessTask _sourceProcessTask;

    public ExecuteCommandReOrderProcessTask(IActivateItems activator,
        ProcessTaskCombineable sourceProcessTaskCombineable, ProcessTask targetProcessTask,
        InsertOption insertOption) : base(activator)
    {
        _targetProcessTask = targetProcessTask;
        _insertOption = insertOption;
        _sourceProcessTask = sourceProcessTaskCombineable.ProcessTask;

        if (_sourceProcessTask.LoadMetadata_ID != targetProcessTask.LoadMetadata_ID)
            SetImpossible("ProcessTasks must belong to the same Load");
        else if (_sourceProcessTask.LoadStage != targetProcessTask.LoadStage)
            SetImpossible("ProcessTasks must belong in the same LoadStage to be ReOrdered");
        else if (_insertOption == InsertOption.Default)
            SetImpossible("Drag above or below to ReOrder");
    }

    public override void Execute()
    {
        base.Execute();

        var destinationOrder = 0;

        var lmd = _targetProcessTask.LoadMetadata;

        if (_insertOption == InsertOption.InsertAbove)
        {
            destinationOrder = _targetProcessTask.Order - 1;

            foreach (var pt in lmd.ProcessTasks)
            {
                //don't change the current one again
                if (pt.Equals(_sourceProcessTask))
                    continue;

                if (pt.Order <= destinationOrder)
                {
                    pt.Order--;
                    pt.SaveToDatabase();
                }
            }
        }
        else
        {
            destinationOrder = _targetProcessTask.Order + 1;

            foreach (var pt in lmd.ProcessTasks)
            {
                //don't change the current one again
                if (pt.Equals(_sourceProcessTask))
                    continue;

                if (pt.Order >= destinationOrder)
                {
                    pt.Order++;
                    pt.SaveToDatabase();
                }
            }
        }

        _sourceProcessTask.Order = destinationOrder;
        _sourceProcessTask.SaveToDatabase();
        Publish(lmd);
    }
}