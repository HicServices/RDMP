// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TypeGuesser;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public sealed class ExecuteCommandCreateNewPreLoadDiscardedColumn : BasicUICommandExecution
{
    private readonly TableInfo _tableInfo;
    private readonly ColumnInfo[] _prototypes;

    public ExecuteCommandCreateNewPreLoadDiscardedColumn(IActivateItems activator, TableInfo tableInfo) :
        base(activator)
    {
        _tableInfo = tableInfo;
    }

    public ExecuteCommandCreateNewPreLoadDiscardedColumn(IActivateItems activator, TableInfo tableInfo,
        ColumnInfoCombineable sourceColumnInfoCombineable) : this(activator, tableInfo)
    {
        _prototypes = sourceColumnInfoCombineable.ColumnInfos;

        var existing = tableInfo.PreLoadDiscardedColumns;
        foreach (var prototype in _prototypes)
        {
            var alreadyExists = existing.Any(c => c.GetRuntimeName()?.Equals(prototype.GetRuntimeName()) == true);

            if (alreadyExists)
                SetImpossible($"There is already a PreLoadDiscardedColumn called '{prototype.GetRuntimeName()}'");
        }
    }

    public override string GetCommandHelp() =>
        "Creates a virtual column that will be created in RAW during data load but not your LIVE database";

    public override void Execute()
    {
        base.Execute();

        if (_prototypes != null)
        {
            foreach (var prototype in _prototypes)
                Create(prototype.GetRuntimeName(), prototype.Data_type);

            Publish();
            return;
        }

        var varcharMaxDataType = _tableInfo.GetQuerySyntaxHelper().TypeTranslater
            .GetSQLDBTypeForCSharpType(new DatabaseTypeRequest(typeof(string), int.MaxValue));

        using var textDialog = new TypeTextOrCancelDialog("Column Name",
            "Enter name for column (this should NOT include any qualifiers e.g. database name)", 300);
        if (textDialog.ShowDialog() != DialogResult.OK)
            return;

        using var textDialog2 = new TypeTextOrCancelDialog("Column DataType",
            "Enter data type for column (e.g. 'varchar(10)')", 300, varcharMaxDataType);
        if (textDialog2.ShowDialog() != DialogResult.OK)
            return;

        var name = textDialog.ResultText;
        var dataType = textDialog2.ResultText;

        var created = Create(name, dataType);
        Publish();
        Emphasise(created);
        Activate(created);
    }

    private void Publish()
    {
        Publish(_tableInfo);
    }

    private PreLoadDiscardedColumn Create(string name, string dataType)
    {
        var discCol = new PreLoadDiscardedColumn(Activator.RepositoryLocator.CatalogueRepository, _tableInfo, name)
        {
            SqlDataType = dataType
        };
        discCol.SaveToDatabase();
        return discCol;
    }

    public override string GetCommandName() => "Add New Load Discarded Column";

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.PreLoadDiscardedColumn, OverlayKind.Add);
}