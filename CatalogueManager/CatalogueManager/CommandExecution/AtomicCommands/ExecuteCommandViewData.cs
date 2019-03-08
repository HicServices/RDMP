using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueManager.DataViewing;
using CatalogueManager.DataViewing.Collections;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandViewData : BasicUICommandExecution,IAtomicCommand
    {
        private readonly IViewSQLAndResultsCollection _collection;
        private readonly ViewType _viewType;

        /// <summary>
        /// Fetches the <paramref name="viewType"/> of the data in <see cref="ColumnInfo"/> <paramref name="c"/>
        /// </summary>
        /// <param name="activator"></param>
        /// <param name="viewType"></param>
        /// <param name="c"></param>
        public ExecuteCommandViewData(IActivateItems activator,ViewType viewType, ColumnInfo c) : base(activator)
        {
            _collection = new ViewColumnInfoExtractUICollection(c, viewType);
            _viewType = viewType;

            if (!c.IsNumerical() && viewType == ViewType.Distribution)
                SetImpossible("Column is not numerical");
        }

        public override string GetCommandName()
        {
            return "View " + _viewType.ToString().Replace("_"," ");
        }

        /// <summary>
        /// Views the top 100 records of the <paramref name="tableInfo"/>
        /// </summary>
        /// <param name="activator"></param>
        /// <param name="tableInfo"></param>
        public ExecuteCommandViewData(IActivateItems activator, TableInfo tableInfo) : base(activator)
        {
            _viewType = ViewType.TOP_100;
            _collection = new ViewTableInfoExtractUICollection(tableInfo,_viewType);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return null;
        }

        public override void Execute()
        {
            Activator.ViewDataSample(_collection);
        }
    }
}