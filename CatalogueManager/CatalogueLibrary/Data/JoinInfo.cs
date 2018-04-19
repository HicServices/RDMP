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
    /// <summary>
    /// Lookup relationships in RDMP are defined using 3 columns, a PrimaryKey from one table and a ForeignKey which appears in the lookup and the Description column
    /// which must also appear in the ForeignKey table.  This Enum is used to identify which ColumnInfo you are addressing in this relationship.
    /// </summary>
    public enum LookupType
    {
        /// <summary>
        /// The column in the Lookup table which contains the description of what a code means
        /// </summary>
        Description,

        /// <summary>
        /// Used for Fetching only, this value reflects either the PrimaryKey or the ForeignKey (but not the Description).  Used for example to find out 
        /// all the Lookup involvements of a given ColumnInfo.
        /// </summary>
        AnyKey,

        /// <summary>
        /// The column in the lookup table which contains the code
        /// </summary>
        ForeignKey
    }

    /// <summary>
    /// The type of ANSI Sql Join to direction e.g. Left/Right
    /// </summary>
    public enum ExtractionJoinType
    {
        /// <summary>
        /// All records from the table on the left and any matching ones from the table on the right (otherwise null for those fields)
        /// </summary>
        Left,

        /// <summary>
        /// All records from the table on the right and any matching ones from the table on the left (otherwise null for those fields)
        /// </summary>
        Right,

        /// <summary>
        /// Only records where the primary/foreign keys match exactly between both tables (the right and the left)
        /// </summary>
        Inner
    }
    
    /// <summary>
    /// Persistent reference in the Catalogue database that records how to join two TableInfos.  You can create instances of this class via JoinHelper (which is available as
    /// a property on ICatalogueRepository).  JoinInfos are processed by during query building in the following way:
    /// 
    /// <para>1. Query builder identifies all the TablesUsedInQuery (from the columns selected, forced table inclusions etc)
    /// 2. Query builder identifies all available JoinInfos between the TablesUsedInQuery (See SqlQueryBuilderHelper.FindRequiredJoins)
    /// 3. Query builder merges JoinInfos that reference the same tables together into Combo Joins (See AddQueryBuildingTimeComboJoinDiscovery)
    /// 4. Query builder creates final Join Sql </para>
    /// 
    /// <para>'Combo Joins' (or ISupplementalJoin) are when you need to use multiple columns to do the join e.g. A Left Join B on A.x = B.x AND A.y = B.y.  You can define
    /// these by simply declaring additional JoinInfos for the other column pairings with the same ExtractionJoinType.</para>
    /// </summary>
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

        /// <summary>
        /// Constructor to be used to create already existing JoinInfos out of the database only.  If you want to create new JoinInfos use JoinInfoFinder in CatalogueRepository.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="r"></param>
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
