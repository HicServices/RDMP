using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.Interfaces.Data.DataTables;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Injection;

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
    public class SelectedDatasetsForcedJoin : DatabaseEntity, ISelectedDatasetsForcedJoin, IInjectKnown<TableInfo>
    {
        #region Database Properties

        private int _selectedDatasets_ID;
        private int _tableInfo_ID;
        private Lazy<TableInfo> _knownTableInfo;

        #endregion

        public int SelectedDatasets_ID
        {
            get { return _selectedDatasets_ID; }
            set { SetField(ref _selectedDatasets_ID, value); }
        }
        public int TableInfo_ID
        {
            get { return _tableInfo_ID; }
            set { SetField(ref _tableInfo_ID, value); }
        }
        
        [NoMappingToDatabase]
        public TableInfo TableInfo
        {
            get { return _knownTableInfo.Value; }
            
        }

        public SelectedDatasetsForcedJoin(IDataExportRepository repository,SelectedDataSets sds, TableInfo tableInfo)
        {
            repository.InsertAndHydrate(this, new Dictionary<string, object>()
            {
                {"SelectedDatasets_ID",sds.ID},
                {"TableInfo_ID",tableInfo.ID},
            });

            if (ID == 0 || Repository != repository)
                throw new ArgumentException("Repository failed to properly hydrate this class");

            ClearAllInjections();
        }
        internal SelectedDatasetsForcedJoin(IRepository repository, DbDataReader r): base(repository, r)
        {
            SelectedDatasets_ID = Convert.ToInt32(r["SelectedDatasets_ID"]);
            TableInfo_ID = Convert.ToInt32(r["TableInfo_ID"]);

            ClearAllInjections();
        }

        public void InjectKnown(TableInfo instance)
        {
            _knownTableInfo = new Lazy<TableInfo>(()=>instance);
        }

        public void ClearAllInjections()
        {
            _knownTableInfo = new Lazy<TableInfo>(FetchTableInfo);
        }

        private TableInfo FetchTableInfo()
        {
            return ((IDataExportRepository) Repository).CatalogueRepository.GetObjectByID<TableInfo>(TableInfo_ID);
        }
    }
}
