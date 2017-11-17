using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Describes to QueryBuilder a secondary/tertiary etc join requirement when making a Lookup join (see description of Lookup.cs)
    /// </summary>
    public class LookupCompositeJoinInfo : VersionedDatabaseEntity, ISupplementalJoin
    {
        #region Database Properties
        private int _originalLookup_ID;
        private int _foreignKey_ID;
        private int _primaryKey_ID;
        private string _collation;

        public int OriginalLookup_ID
        {
            get { return _originalLookup_ID; }
            set { SetField(ref _originalLookup_ID, value); }
        }
        public int ForeignKey_ID
        {
            get { return _foreignKey_ID; }
            set { SetField(ref _foreignKey_ID, value); }
        }
        public int PrimaryKey_ID
        {
            get { return _primaryKey_ID; }
            set { SetField(ref _primaryKey_ID, value); }
        }
        public string Collation
        {
            get { return _collation; }
            set { SetField(ref _collation, value); }
        }

        #endregion

        #region Relationships
        [NoMappingToDatabase]
        public ColumnInfo ForeignKey { get { return Repository.GetObjectByID<ColumnInfo>(ForeignKey_ID); } }

        [NoMappingToDatabase]
        public ColumnInfo PrimaryKey {
            get { return Repository.GetObjectByID<ColumnInfo>(PrimaryKey_ID); }
        }
        #endregion

        public LookupCompositeJoinInfo(ICatalogueRepository repository, Lookup parent, ColumnInfo foreignKey,
            ColumnInfo primaryKey, string collation = null)
        {
            if (foreignKey.ID == primaryKey.ID)
                throw new ArgumentException("Join Key 1 and Join Key 2 cannot be the same");

            if (foreignKey.TableInfo_ID == primaryKey.TableInfo_ID)
                throw new ArgumentException("Join Key 1 and Join Key 2 are from the same table, this is not cool");

            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"OriginalLookup_ID", parent.ID},
                {"ForeignKey_ID", foreignKey.ID},
                {"PrimaryKey_ID", primaryKey.ID},
                {"Collation", string.IsNullOrWhiteSpace(collation) ? DBNull.Value : (object) collation}
            });
        }

        public LookupCompositeJoinInfo(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            Collation = r["Collation"] as string;
            OriginalLookup_ID = int.Parse(r["OriginalLookup_ID"].ToString());

            ForeignKey_ID = int.Parse(r["ForeignKey_ID"].ToString());
            PrimaryKey_ID = int.Parse(r["PrimaryKey_ID"].ToString());
        }

        
        public override string ToString()
        {
            return ToStringCached();
        }

        private string _cachedToString = null;
        private string ToStringCached()
        {
            return _cachedToString ?? (_cachedToString = ForeignKey.Name + " = " + PrimaryKey.Name);
        }

        public override void SaveToDatabase()
        {
            if (ForeignKey.ID == PrimaryKey.ID)
                throw new ArgumentException("Join Key 1 and Join Key 2 cannot be the same");

            if (ForeignKey.TableInfo_ID == PrimaryKey.TableInfo_ID)
                throw new ArgumentException("Join Key 1 and Join Key 2 are from the same table, this is not cool");

            base.SaveToDatabase();
        }

        public IEnumerable<IJoin> GetSupplementalJoins()
        {
            return null;
        }
    }
}
