// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.DataLoadUIs.ANOUIs.ANOTableManagement;
using Rdmp.UI.ItemActivation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

internal class ExecuteCommandAnonymiseColumnInfo : BasicUICommandExecution, IAtomicCommand
{
    private readonly ColumnInfo _columnInfo;

    public ExecuteCommandAnonymiseColumnInfo(IActivateItems activator, ColumnInfo columnInfo) : base(activator)
    {
        _columnInfo = columnInfo;
        if (columnInfo.GetRuntimeName().StartsWith(ANOTable.ANOPrefix, StringComparison.CurrentCultureIgnoreCase))
            SetImpossible($"ColumnInfo is already anonymised (Starts with \"{ANOTable.ANOPrefix}\"");

        if (columnInfo.ANOTable_ID != null)
            SetImpossible("ColumnInfo is already anonymised");

        if (Activator.ServerDefaults.GetDefaultFor(PermissableDefaults.ANOStore) == null)
            SetImpossible("No Default ANOStore has been configured");

        if (string.IsNullOrWhiteSpace(_columnInfo.TableInfo.Server))
            SetImpossible("Parent TableInfo is missing a value for Server");

        if (string.IsNullOrWhiteSpace(_columnInfo.TableInfo.Database))
            SetImpossible("Parent TableInfo is missing a value for Database");
    }

    public override void Execute()
    {
        base.Execute();

        Activator.Activate<ColumnInfoToANOTableConverterUI, ColumnInfo>(_columnInfo);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.ANOColumnInfo);
    }
}