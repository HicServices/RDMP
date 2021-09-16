// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.Providers.Nodes.PipelineNodes;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;

using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution;
using Rdmp.UI.SimpleDialogs;

namespace Rdmp.UI.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsPipeline : RDMPCommandExecutionProposal<Pipeline>
    {
        
        public ProposeExecutionWhenTargetIsPipeline(IActivateItems itemActivator): base(itemActivator)
        {
        }


        public override void Activate(Pipeline target)
        {
            var dialog = new SelectDialog<StandardPipelineUseCaseNode>(
                ItemActivator,
                ItemActivator.CoreChildProvider.PipelineUseCases.ToArray(),false,false);

            dialog.Text = "What is this Pipeline supposed to be used for?";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var useCase = dialog.Selected;
                if(useCase != null)
                {
                    var cmd = new ExecuteCommandEditPipelineWithUseCase(ItemActivator,target, useCase.UseCase);
                    cmd.Execute();
                }
            }
        }

        public override bool CanActivate(Pipeline target)
        {
            return true;
        }

        public override ICommandExecution ProposeExecution(ICombineToMakeCommand cmd, Pipeline target, InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}
