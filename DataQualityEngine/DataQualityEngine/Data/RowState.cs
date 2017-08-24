using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReusableLibraryCode;

namespace DataQualityEngine.Data
{
    public class RowState
    {

        public int Correct { get; private set; }
        public int Missing { get; private set; }
        public int Wrong { get; private set; }
        public int Invalid { get; private set; }
        public int DataLoadRunID { get; private set; }
        public string ValidatorXML { get; private set; }
        public string PivotCategory { get; private set; }


    public RowState(DbDataReader r)
        {
            Correct = Convert.ToInt32(r["Correct"]);
            Missing = Convert.ToInt32(r["Missing"]);
            Wrong   = Convert.ToInt32(r["Wrong"]);
            Invalid = Convert.ToInt32(r["Invalid"]);
            PivotCategory = (string)r["PivotCategory"];
            DataLoadRunID = Convert.ToInt32(r["DataLoadRunID"]);
            ValidatorXML = r["ValidatorXML"].ToString();
        }

        

        public RowState(Evaluation evaluation, int dataLoadRunID, int correct, int missing, int wrong,int invalid, string validatorXml,string pivotCategory, DbConnection con, DbTransaction transaction)
        {

            var sql = string.Format(
                "INSERT INTO [dbo].[RowState]([Evaluation_ID],[Correct],[Missing],[Wrong],[Invalid],[DataLoadRunID],[ValidatorXML],[PivotCategory])VALUES({0},{1},{2},{3},{4},{5},@validatorXML,{6})",
                evaluation.ID,
                correct,
                missing,
                wrong,
                invalid,
                dataLoadRunID,
                "@pivotCategory"
                );

            var cmd = DatabaseCommandHelper.GetCommand(sql, con, transaction);
            DatabaseCommandHelper.AddParameterWithValueToCommand("@validatorXML",cmd,validatorXml);
            DatabaseCommandHelper.AddParameterWithValueToCommand("@pivotCategory", cmd, pivotCategory);
            cmd.ExecuteNonQuery();

            Correct = correct;
            Missing = missing;
            Wrong = wrong;
            Invalid = invalid;
            ValidatorXML = validatorXml;
            DataLoadRunID = dataLoadRunID;
        }
    }
}
