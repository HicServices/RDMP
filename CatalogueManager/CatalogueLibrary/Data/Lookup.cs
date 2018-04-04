using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using Microsoft.SqlServer.Management.Smo;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Describes a relationship between 3 ColumnInfos in which 2 are from a lookup table (e.g. z_drugName), these are a primary
    /// key (e.g. DrugCode) and a description (e.g. HumanReadableDrugName).  And a third ColumnInfo from a different table (e.g. 
    /// Prescribing) which is a foreign key (e.g. DrugPrescribed).
    /// 
    /// <para>The QueryBuilder uses this information to work out how to join together various tables in a query.  Note that it is possible
    /// to define the same lookup multiple times just with different foreign keys (e.g. Prescribing and DrugAbuse datasets might both
    /// share the same lookup table z_drugName).</para>
    /// 
    /// <para>It is not possible to create these lookup dependencies automatically because often an agency won't actually have relationships
    /// (referential integrity) between their lookup tables and main datasets due to dirty data / missing lookup values.  These are all
    /// concepts which the RDMP is familiar with and built to handle.</para>
    /// 
    /// <para>Note also that you can have one or more LookupCompositeJoinInfo for when you need to join particularly ugly lookups (e.g. if you 
    /// have the same DrugCode meaning different things based on the prescribing board - you need to join on both drugName and 
    /// prescriberHealthboard).</para>
    /// </summary>
    public class Lookup : VersionedDatabaseEntity, IJoin, IHasDependencies, ICheckable
    {
        //cached answers
        private ColumnInfo _description;
        private ColumnInfo _foreignKey;
        private ColumnInfo _primaryKey;


        #region Database Properties
        private int _description_ID;
        private int _foreignKey_ID;
        private int _primaryKey_ID;
        private string _collation;
        private ExtractionJoinType _extractionJoinType;

        public int Description_ID
        {
            get { return _description_ID; }
            set { SetField(ref _description_ID, value); }
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
        public ExtractionJoinType ExtractionJoinType
        {
            get { return _extractionJoinType; }
            set { SetField(ref _extractionJoinType, value); }
        }

        #endregion
        
        #region Relationships
        /// <summary>
        /// These are dereferenced cached versions of the entities to which the _ID properties refer to, to change them change the _ID version 
        /// </summary>
        [NoMappingToDatabase]
        public ColumnInfo Description
        {
            get
            {
                if (_description == null)
                    _description = Repository.GetObjectByID<ColumnInfo>(Description_ID);
                return _description;
            }
        }

        /// <summary>
        /// These are dereferenced cached versions of the entities to which the _ID properties refer to, to change them change the _ID version 
        /// </summary>
        [NoMappingToDatabase]
        public ColumnInfo ForeignKey
        {
            get
            {
                if (_foreignKey == null)
                    _foreignKey = Repository.GetObjectByID<ColumnInfo>(ForeignKey_ID);
                return _foreignKey;
            }
        }

        /// <summary>
        /// These are dereferenced cached versions of the entities to which the _ID properties refer to, to change them change the _ID version 
        /// </summary>
        [NoMappingToDatabase]
        public ColumnInfo PrimaryKey
        {
            get
            {
                if (_primaryKey == null)
                    _primaryKey = Repository.GetObjectByID<ColumnInfo>(PrimaryKey_ID);
                return _primaryKey;
            }
        }
        #endregion


        public Lookup(ICatalogueRepository repository, ColumnInfo description, ColumnInfo foreignKey, ColumnInfo primaryKey, ExtractionJoinType type, string collation)
        {
            //do checks before it hits the database.
            if (foreignKey.ID == primaryKey.ID)
                throw new ArgumentException("Join Key 1 and Join Key 2 cannot be the same");

            if (foreignKey.TableInfo_ID == primaryKey.TableInfo_ID)
                throw new ArgumentException("Join Key 1 and Join Key 2 are from the same table, this is not cool");

            if (description.TableInfo_ID != primaryKey.TableInfo_ID)
                throw new ArgumentException("Join Key 2 must be in the same table as the Description ColumnInfo (i.e. Primary Key)");

            if(description.ID == primaryKey.ID)
                throw new ArgumentException("Description Column and PrimaryKey Column cannot be the same column!");

            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"Description_ID", description.ID},
                {"ForeignKey_ID", foreignKey.ID},
                {"PrimaryKey_ID", primaryKey.ID},
                {"ExtractionJoinType", type.ToString()},
                {"Collation", string.IsNullOrWhiteSpace(collation) ? DBNull.Value : (object)collation}
            });
        }

        internal Lookup(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            Description_ID = int.Parse(r["Description_ID"].ToString());
            ForeignKey_ID = int.Parse(r["ForeignKey_ID"].ToString());
            PrimaryKey_ID = int.Parse(r["PrimaryKey_ID"].ToString());
            Collation = r["Collation"] as string;

            ExtractionJoinType joinType;

            if (ExtractionJoinType.TryParse(r["ExtractionJoinType"].ToString(), true, out joinType))
                ExtractionJoinType = joinType;
            else
                throw new Exception("Did not recognise ExtractionJoinType:" + r["ExtractionJoinType"]);

            if (ForeignKey_ID == PrimaryKey_ID)
                throw new ArgumentException("Join Key 1 and Join Key 2 cannot be the same");
        }


        public override string ToString()
        {
            return ToStringCached();
        }

        private string _cachedToString = null;
        private string ToStringCached()
        {
            return _cachedToString ?? (_cachedToString = " " + ForeignKey.Name + " = " + PrimaryKey.Name);
        }


        public static Lookup[] GetAllLookupsBetweenTables(TableInfo foreignKeyTable,TableInfo primaryKeyTable)
        {
            if(foreignKeyTable.Equals(primaryKeyTable))
                throw new NotSupportedException("Tables must be different");

            if(!foreignKeyTable.Repository.Equals(primaryKeyTable.Repository))
                throw new NotSupportedException("TableInfos come from different repositories!");

            var repo = ((CatalogueRepository) foreignKeyTable.Repository);
            using (var con = repo.GetConnection())
            {
                DbCommand cmd;
                cmd = DatabaseCommandHelper.GetCommand(@"SELECT * FROM [Lookup] 
  WHERE 
  (SELECT TableInfo_ID FROM ColumnInfo where ID = PrimaryKey_ID) = @primaryKeyTableID
  AND
  (SELECT TableInfo_ID FROM ColumnInfo where ID = [ForeignKey_ID]) = @foreignKeyTableID"
                                     , con.Connection,con.Transaction);

                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@primaryKeyTableID", cmd));
                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@foreignKeyTableID", cmd));

                cmd.Parameters["@primaryKeyTableID"].Value = primaryKeyTable.ID;
                cmd.Parameters["@foreignKeyTableID"].Value = foreignKeyTable.ID;

                DbDataReader r = cmd.ExecuteReader();

                List<Lookup> toReturn = new List<Lookup>();


                while (r.Read())
                {
                    toReturn.Add(new Lookup(repo,r));
                }

                r.Close();

                return toReturn.ToArray();
            }
        }
     

        
        public void Check(ICheckNotifier notifier)
        {
            
            if (ForeignKey.TableInfo_ID == PrimaryKey.TableInfo_ID)
                notifier.OnCheckPerformed(new CheckEventArgs("Foreign Key and Primary Key are from the same table for Lookup " + this.ID,CheckResult.Fail));

            if (Description.TableInfo_ID != PrimaryKey.TableInfo_ID)
                notifier.OnCheckPerformed(new CheckEventArgs("Description Key and Primary Key are from different tables (Not allowed) in Lookup " + this.ID, CheckResult.Fail));
        }
        
        public override void SaveToDatabase()
        {
            //do checks before it hits the database.
            if (ForeignKey.ID == PrimaryKey.ID)
                throw new ArgumentException("Join Key 1 and Join Key 2 cannot be the same");

            if (ForeignKey.TableInfo_ID == PrimaryKey.TableInfo_ID)
                throw new ArgumentException("Join Key 1 and Join Key 2 are from the same table, this is not cool");

            if (Description.TableInfo_ID != PrimaryKey.TableInfo_ID)
                throw new ArgumentException("Join Key 2 must be in the same table as the Description ColumnInfo (i.e. Primary Key)");

            base.SaveToDatabase();
        }


        public static int CountUniquePrimaryKeyTablesInLookupCollection(Lookup[] foreignKeyLookupInvolvement)
        {
            HashSet<int> primaryKeyTableIDsSeen = new HashSet<int>();

            foreach (Lookup l in foreignKeyLookupInvolvement)
                primaryKeyTableIDsSeen.Add(l.PrimaryKey.TableInfo_ID);

            return primaryKeyTableIDsSeen.Count;
        }

        public IEnumerable<ISupplementalJoin> GetSupplementalJoins()
        {
            return Repository.GetAllObjects<LookupCompositeJoinInfo>("WHERE OriginalLookup_ID=" + ID);
        }

        public ExtractionJoinType GetInvertedJoinType()
        {
            throw new NotSupportedException("Lookup joins should never be inverted... can't see why you would want to do that... they are always LEFT joined ");
        }

        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            return new[] {Description, ForeignKey, PrimaryKey};
        }

        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            return null;
        }

        public void SetKnownColumns(ColumnInfo primaryKey, ColumnInfo foreignKey, ColumnInfo descriptionColumn)
        {
            if(PrimaryKey_ID != primaryKey.ID || ForeignKey_ID != foreignKey.ID || Description_ID != descriptionColumn.ID)
                throw new Exception("Injected arguments did not match on ID");

            _primaryKey = primaryKey;
            _foreignKey = foreignKey;
            _description = descriptionColumn;

        }
    }
}
