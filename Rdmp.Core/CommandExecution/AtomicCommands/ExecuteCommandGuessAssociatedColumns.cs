using Rdmp.Core.Curation.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    /// <summary>
    /// Automatically associates <see cref="CatalogueItem"/> in a <see cref="Catalogue"/> with underlying columns in a given <see cref="TableInfo"/> based on name
    /// </summary>
    public class ExecuteCommandGuessAssociatedColumns : BasicCommandExecution
    {
        private readonly ICatalogue _catalogue;
        private readonly ITableInfo _tableInfo;

        public ExecuteCommandGuessAssociatedColumns(IBasicActivateItems activator,ICatalogue catalogue,ITableInfo tableInfo):base(activator)
        {
            this._catalogue = catalogue;
            this._tableInfo = tableInfo;
        }
        public override void Execute()
        {
            base.Execute();

            int itemsSeen = 0;
            int itemsQualifying = 0;
            int successCount = 0;
            int failCount = 0;

            var selectedTableInfo = _tableInfo ?? (ITableInfo)BasicActivator.SelectOne("Map to table",BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<TableInfo>());

            if(selectedTableInfo  == null)
                return;

            //get all columns for the selected parent
            ColumnInfo[] guessPool = selectedTableInfo.ColumnInfos.ToArray();

            foreach (CatalogueItem catalogueItem in _catalogue.CatalogueItems)
            {
                itemsSeen++;
                //catalogue item already has one an associated column so skip it
                if (catalogueItem.ColumnInfo_ID != null)
                    continue;

                //guess the associated columns
                ColumnInfo[] guesses = catalogueItem.GuessAssociatedColumn(guessPool).ToArray();

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

                    bool acceptedOne = false;
                    
                    for (int i = 0; i < guesses.Length; i++)
                    {
                        //ask confirmation 
                        if (!BasicActivator.IsInteractive || BasicActivator.YesNo(
                                "Found multiple matches, approve match?:" + Environment.NewLine + catalogueItem.Name +
                                Environment.NewLine + guesses[i], "Multiple matched guesses"))
                        {
                            catalogueItem.SetColumnInfo(guesses[i]);
                            successCount++;
                            acceptedOne = true;
                            break;
                        }
                    }

                    if (!acceptedOne)
                        failCount++;
                }
            }

            BasicActivator.Show(
                "Examined:" + itemsSeen + " CatalogueItems" + Environment.NewLine +
                "Orphans Seen:" + itemsQualifying + Environment.NewLine +
                "Guess Success:" + successCount + Environment.NewLine +
                "Guess Failed:" + failCount + Environment.NewLine
                );

            Publish(_catalogue);
        }
    }
}
