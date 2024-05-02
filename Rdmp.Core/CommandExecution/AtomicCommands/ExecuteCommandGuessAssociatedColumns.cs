// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Automatically associates <see cref="CatalogueItem" /> in a <see cref="Catalogue" /> with underlying columns in a
///     given <see cref="TableInfo" /> based on name
/// </summary>
public class ExecuteCommandGuessAssociatedColumns : BasicCommandExecution
{
    private readonly ICatalogue _catalogue;
    private readonly ITableInfo _tableInfo;

    public bool _allowPartial;
    public bool PromptForPartialMatching { get; set; } = false;

    public ExecuteCommandGuessAssociatedColumns(IBasicActivateItems activator, ICatalogue catalogue,
        ITableInfo tableInfo, bool allowPartial = true) : base(activator)
    {
        _catalogue = catalogue;
        _tableInfo = tableInfo;
        _allowPartial = allowPartial;
    }

    public override void Execute()
    {
        base.Execute();

        var itemsSeen = 0;
        var itemsQualifying = 0;
        var successCount = 0;
        var failCount = 0;

        var selectedTableInfo = _tableInfo ?? (ITableInfo)BasicActivator.SelectOne("Map to table",
            BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<TableInfo>());

        if (selectedTableInfo == null)
            return;

        //get all columns for the selected parent
        var guessPool = selectedTableInfo.ColumnInfos.ToArray();

        // copy to local variable to keep Command immutable
        var partial = _allowPartial;

        if (BasicActivator.IsInteractive && PromptForPartialMatching)
            partial = BasicActivator.YesNo(new DialogArgs
            {
                WindowTitle = "Allow Partial Matches",
                TaskDescription =
                    "Do you want to allow partial matches (contains).  This may result in incorrect mappings."
            });

        foreach (var catalogueItem in _catalogue.CatalogueItems)
        {
            itemsSeen++;
            //catalogue item already has one an associated column so skip it
            if (catalogueItem.ColumnInfo_ID != null)
                continue;

            //guess the associated columns
            var guesses = catalogueItem.GuessAssociatedColumn(guessPool, partial).ToArray();

            itemsQualifying++;

            //if there is exactly 1 column that matches the guess
            if (guesses.Length == 1)
            {
                catalogueItem.SetColumnInfo(guesses[0]);
                successCount++;
            }
            else
            {
                //note that this else branch also deals with case where guesses is empty

                var acceptedOne = false;

                for (var i = 0; i < guesses.Length; i++)
                    //ask confirmation
                    if (!BasicActivator.IsInteractive || BasicActivator.YesNo(
                            $"Found multiple matches, approve match?:{Environment.NewLine}{catalogueItem.Name}{Environment.NewLine}{guesses[i]}",
                            "Multiple matched guesses"))
                    {
                        catalogueItem.SetColumnInfo(guesses[i]);
                        successCount++;
                        acceptedOne = true;
                        break;
                    }

                if (!acceptedOne)
                    failCount++;
            }
        }

        BasicActivator.Show(
            $"Examined:{itemsSeen} CatalogueItems{Environment.NewLine}Orphans Seen:{itemsQualifying}{Environment.NewLine}Guess Success:{successCount}{Environment.NewLine}Guess Failed:{failCount}{Environment.NewLine}"
        );

        Publish(_catalogue);
    }
}