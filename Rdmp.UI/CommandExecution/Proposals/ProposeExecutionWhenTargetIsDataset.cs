// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Org.BouncyCastle.Asn1.X509.Qualified;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data.Datasets;
using Rdmp.Core.Datasets;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SubComponents;

namespace Rdmp.UI.CommandExecution.Proposals;

internal class
    ProposeExecutionWhenTargetIsDataset : RDMPCommandExecutionProposal<
        Dataset>
{
    public ProposeExecutionWhenTargetIsDataset(IActivateItems itemActivator) : base(
        itemActivator)
    {
    }

    public override bool CanActivate(Dataset target) => true;

    public override void Activate(Dataset target)
    {
        string PureAssembly = typeof(PureDatasetProvider).ToString();
        string HDRAssembly = typeof(HDRDatasetProvider).ToString();
        string JiraAssembly = typeof(JiraDatasetProvider).ToString();
        if (target.Type == PureAssembly)
        {
            ItemActivator.Activate<PureDatasetConfigurationUI,Dataset>(target);
            return;
        }
        if (target.Type == HDRAssembly)
        {
            ItemActivator.Activate<HDRDatasetConfigurationUI, Dataset>(target);
            return;
        }
        if (target.Type == JiraAssembly)
        {
            ItemActivator.Activate<JiraDatasetConfigurationUI, Dataset>(target);
            return;
        }


        ItemActivator.Activate<DatasetConfigurationUI, Dataset>(target);
    }


    [CanBeNull]
    public override ICommandExecution ProposeExecution(ICombineToMakeCommand cmd,
        Dataset target,
        InsertOption insertOption = InsertOption.Default) =>
        null;
}