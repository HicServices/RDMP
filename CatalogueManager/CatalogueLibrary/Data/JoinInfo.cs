using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data
{
    public enum LookupType
    {
        Description,
        AnyKey,
        ForeignKey
    }

    public enum ExtractionJoinType
    {
        Left,
        Right,
        Inner
    }
    
    public class JoinInfo : IDeleteable, IJoin,IHasDependencies
    {
        public IRepository Repository { get; set; }
        //raw values read from database
        public int ForeignKey_ID { get; private set; }
        public int PrimaryKey_ID { get; private set; }

        //cached answer
        private ColumnInfo _foreignKey;
        private ColumnInfo _primaryKey;

        //properties for retrieving cached answers
        public ColumnInfo ForeignKey
        {
            get { return _foreignKey ?? (_foreignKey = Repository.GetObjectByID<ColumnInfo>(ForeignKey_ID)); }
        }

        public ColumnInfo PrimaryKey
        {
            get { return _primaryKey ?? (_primaryKey = Repository.GetObjectByID<ColumnInfo>(PrimaryKey_ID)); }
        }

        public string Collation { get; set; }
        public ExtractionJoinType ExtractionJoinType { get; set; }

        public JoinInfo(IRepository repository,DbDataReader r)
        {
            Repository = repository;
            ForeignKey_ID = Convert.ToInt32(r["ForeignKey_ID"]);
            PrimaryKey_ID = Convert.ToInt32(r["PrimaryKey_ID"]);

            Collation = r["Collation"] as string;

            ExtractionJoinType joinType;

            if (ExtractionJoinType.TryParse(r["ExtractionJoinType"].ToString(), true, out joinType))
                ExtractionJoinType = joinType;
            else
                throw new Exception("Did not recognise ExtractionJoinType:" + r["ExtractionJoinType"]);

            if (ForeignKey_ID == PrimaryKey_ID)
                throw new Exception("Join key 1 and 2 are the same, lookup is broken");
        }

        public override string ToString()
        {
            return " " + ForeignKey.Name + " = " + PrimaryKey.Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((JoinInfo)obj);
        }

        #region Database specific stuff for this table only
        
        public void DeleteInDatabase()
        {
            using(var con = ((CatalogueRepository)Repository).GetConnection())
            {
                //only add link if it doesn't already exist
                DbCommand cmd =
                     DatabaseCommandHelper.GetCommand(
                        "DELETE FROM JoinInfo WHERE ForeignKey_ID=@ForeignKey_ID AND PrimaryKey_ID=@PrimaryKey_ID",
                        con.Connection,con.Transaction);

                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@ForeignKey_ID", cmd));
                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@PrimaryKey_ID", cmd));

                cmd.Parameters["@ForeignKey_ID"].Value = ForeignKey_ID;
                cmd.Parameters["@PrimaryKey_ID"].Value = PrimaryKey_ID;

                int affectedRows = cmd.ExecuteNonQuery();
                if (affectedRows != 1)
                    throw new Exception("DELETE statement " + cmd.CommandText + " did not result in 1 affected rows, it resulted in:" + affectedRows);
            }
        }
        #endregion

        #region Resharper generated code to test for equality based on Foreign, Primary and Extraction type
        protected bool Equals(JoinInfo other)
        {
            return Equals(ForeignKey.ID, other.ForeignKey.ID) && Equals(PrimaryKey.ID, other.PrimaryKey.ID) && ExtractionJoinType == other.ExtractionJoinType;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (ForeignKey != null ? ForeignKey.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (PrimaryKey != null ? PrimaryKey.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)ExtractionJoinType;
                return hashCode;
            }
        }
        #endregion
         
        private List<JoinInfo> _queryTimeComboJoins = new List<JoinInfo>();
        
        public void AddQueryBuildingTimeComboJoinDiscovery(JoinInfo availableJoin)
        {
            if(availableJoin.Equals(this))
                throw new Exception("A JoinInfo cannot add QueryTimeComboJoin to itself");

            if(!_queryTimeComboJoins.Contains(availableJoin))
                _queryTimeComboJoins.Add(availableJoin);
        }

        public IEnumerable<ISupplementalJoin> GetSupplementalJoins()
        {
            //Supplemental Joins are not currently supported by JoinInfo, only Lookups
            return _queryTimeComboJoins.Select(j => new QueryTimeComboJoin()
            {
                Collation = j.Collation,
                PrimaryKey = j.PrimaryKey,
                ForeignKey = j.ForeignKey
            });
        }

        public ExtractionJoinType GetInvertedJoinType()
        {
            switch (ExtractionJoinType)
            {
                case ExtractionJoinType.Left:
                    return ExtractionJoinType.Right;
                case ExtractionJoinType.Right:
                    return ExtractionJoinType.Left;
                default:
                    return ExtractionJoinType;
            }
        }

        private class QueryTimeComboJoin :ISupplementalJoin
        {
            public ColumnInfo ForeignKey { get; set; }
            public ColumnInfo PrimaryKey { get; set; }
            public string Collation { get; set; }
        }

        public void SetKnownColumns(ColumnInfo primaryKey, ColumnInfo foreignKey)
        {
            if (PrimaryKey_ID != primaryKey.ID || ForeignKey_ID != foreignKey.ID)
                throw new Exception("Injected arguments did not match on ID");

            _primaryKey = primaryKey;
            _foreignKey = foreignKey;
        }

        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            return new[] {PrimaryKey, ForeignKey};
        }

        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            return new IHasDependencies[0];
        }
    }
}
