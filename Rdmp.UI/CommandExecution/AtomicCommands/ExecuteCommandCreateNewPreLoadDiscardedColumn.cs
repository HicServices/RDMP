// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using SixLabors.ImageSharp;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;
using SixLabors.ImageSharp.PixelFormats;
using TypeGuesser;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public class ExecuteCommandCreateNewPreLoadDiscardedColumn:BasicUICommandExecution,IAtomicCommand
{
    private readonly TableInfo _tableInfo;
    private ColumnInfo[] _prototypes;

    public ExecuteCommandCreateNewPreLoadDiscardedColumn(IActivateItems activator,TableInfo tableInfo) : base(activator)
    {
        _tableInfo = tableInfo;
    }

    public ExecuteCommandCreateNewPreLoadDiscardedColumn(IActivateItems activator, TableInfo tableInfo, ColumnInfoCombineable sourceColumnInfoCombineable):this(activator,tableInfo)
    {
        _prototypes = sourceColumnInfoCombineable.ColumnInfos;

        var existing = tableInfo.PreLoadDiscardedColumns;
        foreach (var prototype in _prototypes)
        {
            var alreadyExists = existing.Any(c => c.GetRuntimeName().Equals(prototype.GetRuntimeName()));

            if (alreadyExists)
                SetImpossible($"There is already a PreLoadDiscardedColumn called '{prototype.GetRuntimeName()}'");
        }
          
    }

    public override string GetCommandHelp()
    {
        return "Creates a virtual column that will be created in RAW during data load but not your LIVE database";
    }

    public override void Execute()
    {
        base.Execute();

        string name = null;
        string dataType = null;

        if(_prototypes == null)
        {
            var varcharMaxDataType = _tableInfo.GetQuerySyntaxHelper().TypeTranslater.GetSQLDBTypeForCSharpType(new DatabaseTypeRequest(typeof (string), int.MaxValue));

            var textDialog = new TypeTextOrCancelDialog("Column Name","Enter name for column (this should NOT include any qualifiers e.g. database name)", 300);
            if (textDialog.ShowDialog() == DialogResult.OK)
                name = textDialog.ResultText;
            else
                return;

            textDialog = new TypeTextOrCancelDialog("Column DataType", "Enter data type for column (e.g. 'varchar(10)')", 300, varcharMaxDataType);
            if (textDialog.ShowDialog() == DialogResult.OK)
                dataType = textDialog.ResultText;
            else
                return;

            var created = Create(name, dataType);
            Publish();
            Emphasise(created);
            Activate(created);

        }
        else
        {
            foreach (var prototype in _prototypes)
                Create(prototype.GetRuntimeName(), prototype.Data_type);

            Publish();
        }
    }

    private void Publish()
    {
        Publish(_tableInfo);
    }

    private PreLoadDiscardedColumn Create(string name, string dataType)
    {
        var discCol = new PreLoadDiscardedColumn(Activator.RepositoryLocator.CatalogueRepository, _tableInfo, name);
        discCol.SqlDataType = dataType;
        discCol.SaveToDatabase();
        return discCol;
    }

    public override string GetCommandName()
    {
        return "Add New Load Discarded Column";
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.PreLoadDiscardedColumn, OverlayKind.Add);
    }
}