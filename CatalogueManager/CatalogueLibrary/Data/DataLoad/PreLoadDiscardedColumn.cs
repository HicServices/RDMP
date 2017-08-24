using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data.DataLoad
{
    public enum DiscardedColumnDestination
    {
        Oblivion=1,
        StoreInIdentifiersDump=2,
        Dilute=3

    }

    /// <summary>
    /// Describes a column that is provided to your institution by a data provider but which is not loaded into your LIVE database table.  This column might be very sensitive, 
    /// irrelevant to you etc.  Each discarded column has a destination (DiscardedColumnDestination)  e.g. it might be dropped completely or routed into an identifier dump for
    /// when you still want to store information such as Who an MRI was for but do not want it sitting in your live dataset for governance/anonymisation reasons.
    /// 
    /// Each instance is tied to a specific TableInfo and when a data load occurs from an unstructured format (e.g. CSV) which RequestsExternalDatabaseCreation then not only are the
    /// LIVE columns created in the RAW bubble but also the dropped columns described in PreLoadDiscardedColumn instances.  This allows the live system state to drive required formats/fields
    /// for data load resulting in a stricter/more maintainable data load model.
    /// </summary>
    public class PreLoadDiscardedColumn : VersionedDatabaseEntity, IPreLoadDiscardedColumn
    {
        #region Database Properties

        private int _tableInfoID;
        private DiscardedColumnDestination _destination;
        private string _runtimeColumnName;
        private string _sqlDataType;
        private int? _duplicateRecordResolutionOrder;
        private bool _duplicateRecordResolutionIsAscending;

        public int TableInfo_ID
        {
            get { return _tableInfoID; }
            set { SetField(ref  _tableInfoID, value); }
        }

        public DiscardedColumnDestination Destination
        {
            get { return _destination; }
            set { SetField(ref  _destination, value); }
        }

        public string RuntimeColumnName
        {
            get { return _runtimeColumnName; }
            set { SetField(ref  _runtimeColumnName, value); }
        }

        public string SqlDataType
        {
            get { return _sqlDataType; }
            set { SetField(ref  _sqlDataType, value); }
        }

        public int? DuplicateRecordResolutionOrder
        {
            get { return _duplicateRecordResolutionOrder; }
            set { SetField(ref  _duplicateRecordResolutionOrder, value); }
        }

        public bool DuplicateRecordResolutionIsAscending
        {
            get { return _duplicateRecordResolutionIsAscending; }
            set { SetField(ref  _duplicateRecordResolutionIsAscending, value); }
        }

        #endregion
        #region Relationships
        [NoMappingToDatabase]
        public ITableInfo TableInfo
        {
            get
            {
                return Repository.GetObjectByID<TableInfo>(TableInfo_ID);
            }
        }
        #endregion

        //required for IResolveDuplication
        /// <summary>
        /// For setting, use SqlDataType instead, it is an exact alias to allow for IResolveDuplication interface definition (see the fact that ColumnInfo also uses that interface and is also IMapsDirectlyToDatabaseTable)
        /// </summary>
        [NoMappingToDatabase]
        public string Data_type { get { return SqlDataType; } }

        public PreLoadDiscardedColumn(ICatalogueRepository repository, TableInfo parent, string name = null)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"TableInfo_ID", parent.ID},
                {"Destination", DiscardedColumnDestination.Oblivion},
                {"RuntimeColumnName", name ?? "NewColumn" + Guid.NewGuid()}
            });
        }

        public PreLoadDiscardedColumn(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            TableInfo_ID = int.Parse(r["TableInfo_ID"].ToString());
            Destination = (DiscardedColumnDestination) r["Destination"];
            RuntimeColumnName = r["RuntimeColumnName"] as string;
            SqlDataType = r["SqlDataType"] as string;
            
            if (r["DuplicateRecordResolutionOrder"] != DBNull.Value)
                DuplicateRecordResolutionOrder = int.Parse(r["DuplicateRecordResolutionOrder"].ToString());
            else
                DuplicateRecordResolutionOrder = null;

            DuplicateRecordResolutionIsAscending = Convert.ToBoolean(r["DuplicateRecordResolutionIsAscending"]);
        }

        public override string ToString()
        {
            return RuntimeColumnName + " (ID=" + ID+ ", TableInfo_ID=" + TableInfo_ID + ")";
        }

        public string GetRuntimeName()
        {
            //belt and bracers, the user could be typing something mental into this field in his database
            return SqlSyntaxHelper.GetRuntimeName(RuntimeColumnName);
        }
        
        public string GetRuntimeName(LoadStage stage)
        {
            return GetRuntimeName();
        }
        
        public void CloneForNewTable(TableInfo newTableInfo)
        {
            if(this.TableInfo_ID == newTableInfo.ID)
                throw new Exception("Cannot clone into same parent.  Clone must be into a different TableInfo");

            
            //create a new one
            var newPreLoadDiscardedColumn = new PreLoadDiscardedColumn((ICatalogueRepository) Repository, newTableInfo, RuntimeColumnName);
            
            //copy values across
            newPreLoadDiscardedColumn.SqlDataType = SqlDataType;
            newPreLoadDiscardedColumn.Destination = Destination;
            newPreLoadDiscardedColumn.SaveToDatabase();
        }

        /// <summary>
        /// true if destination for column is to store in identifier dump including undilluted versions of dillutes 
        /// (Dillution involves making clean values dirty for purposes of anonymisation and storing the clean values in
        /// the Identifier Dump).
        /// </summary>
        /// <returns></returns>
        public bool GoesIntoIdentifierDump()
        {
            return Destination == DiscardedColumnDestination.StoreInIdentifiersDump
                   ||
                   Destination == DiscardedColumnDestination.Dilute;
        }
    }
}
