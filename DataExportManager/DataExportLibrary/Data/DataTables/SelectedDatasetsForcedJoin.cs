using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.LinkCreators;
using MapsDirectlyToDatabaseTable;

namespace DataExportLibrary.Data.DataTables
{
    /// <summary>
    /// Specifies that the given data export dataset in configuration x (SelectedDataSets) should include a mandatory join on the table TableInfo regardless of 
    /// what columns are selected in the extraction query.  The most common use case for this is to extract a dataset with WhereSQL that references a custom data
    /// table e.g. 'Questionnaire answer x > 5'.  In that scenario the <see cref="TableInfo"/> would be the 'Project Specific Catalogue' dataset 'Questionnaire'
    /// and the <see cref="SelectedDataSets"/> would be the dataset you were extracting in your study e.g. 'biochemistry'.
    /// 
    /// A <see cref="JoinInfo"/> must still exist to tell <see cref="QueryBuilder"/> how to write the Join section of the query.
    /// </summary>
    public class SelectedDatasetsForcedJoin : DatabaseEntity
    {
        #region Database Properties

        private int _selectedDataset_ID;
        private int _tableInfo_ID;
        #endregion

        public int SelectedDataset_ID
        {
            get { return _selectedDataset_ID; }
            set { SetField(ref _selectedDataset_ID, value); }
        }
        public int TableInfo_ID
        {
            get { return _tableInfo_ID; }
            set { SetField(ref _tableInfo_ID, value); }
        }
        public SelectedDatasetsForcedJoin(IDataExportRepository repository,SelectedDataSets sds, TableInfo tableInfo)
        {
            repository.InsertAndHydrate(this, new Dictionary<string, object>()
            {
                {"SelectedDataset_ID",sds.ID},
                {"TableInfo_ID",tableInfo.ID},
            });

            if (ID == 0 || Repository != repository)
                throw new ArgumentException("Repository failed to properly hydrate this class");
        }
        public SelectedDatasetsForcedJoin(IRepository repository, DbDataReader r)
            : base(repository, r)
        {
            SelectedDataset_ID = Convert.ToInt32(r["SelectedDataset_ID"]);
            TableInfo_ID = Convert.ToInt32(r["TableInfo_ID"]);
        }
    }
}
