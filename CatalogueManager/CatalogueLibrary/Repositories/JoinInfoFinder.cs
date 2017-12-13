using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Data;
using ReusableLibraryCode;

namespace CatalogueLibrary.Repositories
{
    public enum JoinInfoType
    {
        AnyKey,
        ForeignKey,
        PrimaryKey
    }

    /// <summary>
    /// Handles creation, discovery and deletion of JoinInfos.  JoinInfos are not IMapsDirectlyToDatabase classes because they are mostly just a m-m relationship
    /// table between ColumnInfos (with join direction / collation).
    /// </summary>
    public class JoinInfoFinder
    {
        private readonly CatalogueRepository _repository;

        public JoinInfoFinder(CatalogueRepository repository)
        {
            _repository = repository;
        }

        public JoinInfo[] GetAllJoinInfosBetweenColumnInfoSets(ColumnInfo[] set1, ColumnInfo[] set2)
        {
            //assemble the IN SQL arrays
            string inSQL1 = " IN (";
            string inSQL2 = " IN (";

            if (set1.Length == 0)
                throw new NullReferenceException("Cannot find joins because column set 1 was empty");
            if (set2.Length == 0)
                throw new NullReferenceException("Cannot find joins because column set 2 was empty");

            int tableID1 = set1[0].TableInfo_ID;
            int tableID2 = set2[0].TableInfo_ID;

            foreach (var col in set1)
            {
                if (col.TableInfo_ID != tableID1)
                    throw new Exception("Columns in set1 come from different tables, they must all belong to the same table!");

                inSQL1 += col.ID + ",";
            }

            foreach (var col in set2)
            {
                if (col.TableInfo_ID != tableID2)
                    throw new Exception("Columns in set2 come from different tables, they must all belong to the same table!");

                inSQL2 += col.ID + ",";
            }

            inSQL1 = inSQL1.TrimEnd(new char[] { ',' });
            inSQL2 = inSQL2.TrimEnd(new char[] { ',' });

            inSQL1 += ")";
            inSQL2 += ")";

            using(var con = _repository.GetConnection())
            {
                DbCommand cmd;
                cmd = DatabaseCommandHelper.GetCommand("SELECT * FROM JoinInfo WHERE " +
                                     "(ForeignKey_ID " + inSQL1 + " AND PrimaryKey_ID " + inSQL2 + " )" +
                                     " OR " +
                                     "(ForeignKey_ID " + inSQL2 + " AND PrimaryKey_ID " + inSQL1 + " )"
                                     , con.Connection,con.Transaction);


                DbDataReader r = cmd.ExecuteReader();

                List<JoinInfo> toReturn = new List<JoinInfo>();

                while (r.Read())
                {
                    JoinInfo toAdd = new JoinInfo(_repository,r);
                    toReturn.Add(toAdd);
                }

                r.Close();

                return toReturn.ToArray();
            }
        }

        public JoinInfo[] GetAllJoinInfosWhereTableContains(TableInfo tableInfo,JoinInfoType type)
        {
            string sql;
            switch (type)
            {
                case JoinInfoType.AnyKey:
                    sql = "SELECT * FROM JoinInfo WHERE ForeignKey_ID in (Select ID from ColumnInfo where TableInfo_ID = " +tableInfo.ID + ") OR PrimaryKey_ID in (Select ID from ColumnInfo where TableInfo_ID = " +tableInfo.ID + ")";
                    break;
                case JoinInfoType.ForeignKey:
                    sql = "SELECT * FROM JoinInfo WHERE ForeignKey_ID in (Select ID from ColumnInfo where TableInfo_ID = " +tableInfo.ID + ")";
                    break;
                case JoinInfoType.PrimaryKey:
                    sql = "SELECT * FROM JoinInfo WHERE PrimaryKey_ID in (Select ID from ColumnInfo where TableInfo_ID = " +tableInfo.ID + ")";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }

            using (var con = _repository.GetConnection())
            {
                DbCommand cmd;
                cmd = DatabaseCommandHelper.GetCommand(sql, con.Connection, con.Transaction);

                DbDataReader r = cmd.ExecuteReader();

                List<JoinInfo> toReturn = new List<JoinInfo>();

                while (r.Read())
                {
                    JoinInfo toAdd = new JoinInfo(_repository, r);
                    toReturn.Add(toAdd);
                }

                r.Close();

                return toReturn.ToArray();
            }
        }

        public JoinInfo[] GetAllJoinInfos()
        {
            using(var con = _repository.GetConnection())
            {
                DbCommand cmd;
                cmd = DatabaseCommandHelper.GetCommand("SELECT * FROM JoinInfo", con.Connection,con.Transaction);
                
                DbDataReader r = cmd.ExecuteReader();

                List<JoinInfo> toReturn = new List<JoinInfo>();

                while (r.Read())
                {
                    JoinInfo toAdd = new JoinInfo(_repository,r);
                    toReturn.Add(toAdd);

                }

                r.Close();

                return toReturn.ToArray();
            }
        }
        public JoinInfo[] GetAllJoinInfoForColumnInfoWhereItIsAForeignKey(ColumnInfo columnInfo)
        {

            using (var con = _repository.GetConnection())
            {
                DbCommand cmd;
                cmd = DatabaseCommandHelper.GetCommand("SELECT * FROM JoinInfo WHERE ForeignKey_ID=" + columnInfo.ID, con.Connection,con.Transaction);

                DbDataReader r = cmd.ExecuteReader();

                List<JoinInfo> toReturn = new List<JoinInfo>();

                while (r.Read())
                {
                    JoinInfo toAdd = new JoinInfo(_repository,r);
                    toReturn.Add(toAdd);
                }

                r.Close();

                return toReturn.ToArray();
            }
        }

        public void AddJoinInfo(ColumnInfo ForeignKey, ColumnInfo PrimaryKey, ExtractionJoinType type, string Collation)
        {

            if (ForeignKey.ID == PrimaryKey.ID)
                throw new ArgumentException("Joink Key 1 and Join Key 2 cannot be the same");

            if (ForeignKey.TableInfo_ID == PrimaryKey.TableInfo_ID)
                throw new ArgumentException("Joink Key 1 and Join Key 2 are from the same table, this is not cool");


            using (var con = _repository.GetConnection())
            {
                //only add link if it doesn't already exist
                DbCommand cmd =
                     DatabaseCommandHelper.GetCommand(
                        "INSERT INTO JoinInfo(ForeignKey_ID,PrimaryKey_ID,ExtractionJoinType,Collation) VALUES(@ForeignKey_ID,@PrimaryKey_ID,@ExtractionJoinType,@Collation)",
                        con.Connection,con.Transaction);
                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@ForeignKey_ID", cmd));
                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@PrimaryKey_ID", cmd));
                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@ExtractionJoinType", cmd));
                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@Collation", cmd));


                cmd.Parameters["@ForeignKey_ID"].Value = ForeignKey.ID;
                cmd.Parameters["@PrimaryKey_ID"].Value = PrimaryKey.ID;
                cmd.Parameters["@ExtractionJoinType"].Value = type.ToString();

                if (string.IsNullOrWhiteSpace(Collation))
                    cmd.Parameters["@Collation"].Value = DBNull.Value;
                else
                    cmd.Parameters["@Collation"].Value = Collation;

                if (cmd.ExecuteNonQuery() != 1)
                    throw new Exception("INSERT statement " + cmd.CommandText + " did not result in 1 affected rows");

            }
        }
    }
}