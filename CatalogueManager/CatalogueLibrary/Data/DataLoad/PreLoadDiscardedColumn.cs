using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft;

namespace CatalogueLibrary.Data.DataLoad
{
    /// <summary>
    /// Describes where a PreLoadDiscardedColumn will ultimately end up.
    /// </summary>
    public enum DiscardedColumnDestination
    {
        /// <summary>
        /// Column appears in RAW and might be used in AdjustRaw but is droped completely prior to migration to Staging
        /// </summary>
        Oblivion=1,

        /// <summary>
        /// Column appears in RAW but is seperated off and stored in an IdentifierDump (See IdentifierDumper) and not passed through to Staging
        /// </summary>
        StoreInIdentifiersDump=2,

        /// <summary>
        /// Column appears in RAW but is Diluted during AdjustStaging prior to joining the live dataset e.g. by rounding dates to the nearest quarter.  The undilted value may be stored in the IdentifierDump (See IdentifierDumper).
        /// </summary>
        Dilute=3

    }

    /// <summary>
    /// Describes a column that is provided to your institution by a data provider but which is not loaded into your LIVE database table.  This column might be very sensitive, 
    /// irrelevant to you etc.  Each discarded column has a destination (DiscardedColumnDestination)  e.g. it might be dropped completely or routed into an identifier dump for
    /// when you still want to store information such as Who an MRI was for but do not want it sitting in your live dataset for governance/anonymisation reasons.
    /// 
    /// <para>Each instance is tied to a specific TableInfo and when a data load occurs from an unstructured format (e.g. CSV) which RequestsExternalDatabaseCreation then not only are the
    /// LIVE columns created in the RAW bubble but also the dropped columns described in PreLoadDiscardedColumn instances.  This allows the live system state to drive required formats/fields
    /// for data load resulting in a stricter/more maintainable data load model.</para>
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

        /// <inheritdoc cref="IPreLoadDiscardedColumn.TableInfo"/>
        public int TableInfo_ID
        {
            get { return _tableInfoID; }
            set { SetField(ref  _tableInfoID, value); }
        }
        /// <inheritdoc/>
        public DiscardedColumnDestination Destination
        {
            get { return _destination; }
            set { SetField(ref  _destination, value); }
        }
        /// <inheritdoc/>
        public string RuntimeColumnName
        {
            get { return _runtimeColumnName; }
            set { SetField(ref  _runtimeColumnName, value); }
        }
        /// <inheritdoc/>
        public string SqlDataType
        {
            get { return _sqlDataType; }
            set { SetField(ref  _sqlDataType, value); }
        }
        /// <inheritdoc/>
        public int? DuplicateRecordResolutionOrder
        {
            get { return _duplicateRecordResolutionOrder; }
            set { SetField(ref  _duplicateRecordResolutionOrder, value); }
        }
        /// <inheritdoc/>
        public bool DuplicateRecordResolutionIsAscending
        {
            get { return _duplicateRecordResolutionIsAscending; }
            set { SetField(ref  _duplicateRecordResolutionIsAscending, value); }
        }

        #endregion
        #region Relationships
        /// <inheritdoc/>
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

        /// <summary>
        /// Creates a new virtual column that will be created in RAW during data loads but does not appear in the LIVE table schema.  This allows
        /// identifiable data to be loaded and processed in a data load without ever hitting the live database.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        public PreLoadDiscardedColumn(ICatalogueRepository repository, TableInfo parent, string name = null)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"TableInfo_ID", parent.ID},
                {"Destination", DiscardedColumnDestination.Oblivion},
                {"RuntimeColumnName", name ?? "NewColumn" + Guid.NewGuid()}
            });
        }

        internal PreLoadDiscardedColumn(ICatalogueRepository repository, DbDataReader r)
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

        /// <inheritdoc/>
        public override string ToString()
        {
            return RuntimeColumnName + " (" + Destination + ")";
        }
        /// <inheritdoc/>
        public string GetRuntimeName()
        {
            //belt and bracers, the user could be typing something mental into this field in his database
            return new MicrosoftQuerySyntaxHelper().GetRuntimeName(RuntimeColumnName);
        }
        /// <inheritdoc/>
        public string GetRuntimeName(LoadStage stage)
        {
            return GetRuntimeName();
        }
        
        /// <summary>
        /// true if destination for column is to store in identifier dump including undiluted versions of dilutes 
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
