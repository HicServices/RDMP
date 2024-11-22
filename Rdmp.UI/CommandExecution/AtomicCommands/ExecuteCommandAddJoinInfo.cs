// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ExtractionUIs.JoinsAndLookups;
using Rdmp.UI.ItemActivation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public class ExecuteCommandAddJoinInfo : BasicUICommandExecution
{
    private readonly TableInfo _tableInfo;
    private TableInfo _otherTableInfo;

    public ExecuteCommandAddJoinInfo(IActivateItems activator, TableInfo tableInfo) : base(activator)
    {
        _tableInfo = tableInfo;
    }

    public override string GetCommandName() => $"Configure JoinInfo where '{_tableInfo}' is a Primary Key Table";

    public override string GetCommandHelp() =>
        "Tells RDMP that two TableInfos can be joined together (including the direction LEFT/RIGHT/INNER, collation etc)";

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        Activator.CoreIconProvider.GetImage(RDMPConcept.JoinInfo, OverlayKind.Add);

    public void SetInitialJoinToTableInfo(TableInfo otherTableInfo)
    {
        if (_tableInfo.Equals(otherTableInfo))
            SetImpossible("Cannot join a TableInfo to itself");

        _otherTableInfo = otherTableInfo;
    }

    public override void Execute()
    {
        base.Execute();

        var jc = Activator.Activate<JoinConfigurationUI, TableInfo>(_tableInfo);

        if (_otherTableInfo != null)
            jc.SetOtherTableInfo(_otherTableInfo);
    }
}