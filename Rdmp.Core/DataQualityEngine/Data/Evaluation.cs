// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace DataQualityEngine.Data
{
    /// <summary>
    /// Root object for a DQE run including the time the DQE engine was run, the Catalogue being evaluated and all the results.  This class basically follows an 
    /// IMapsDirectlyToDatabaseTable/DatabaseEntity pattern except that it doesn't allow for modification/saving since a DQE run is immutable and only created after
    /// a succesful run.
    /// </summary>
    public class Evaluation:IDeleteable
    {
        public int ID { get; private set; }
        public Catalogue Catalogue { get; private set; }
        public DateTime DateOfEvaluation { get; private set; }

        public RowState[] RowStates { get; set; }
        public ColumnState[] ColumnStates { get; set; }

        public DQERepository DQERepository { get; set; }

        public IEnumerable<DQEGraphAnnotation> GetAllDQEGraphAnnotations(string pivotCategory = null)
        {
            return DQERepository.GetAllObjects<DQEGraphAnnotation>().
                Where(a => a.Evaluation_ID == ID && a.PivotCategory.Equals(pivotCategory ?? "ALL"));
        }

        /// <summary>
        /// Starts a new evaluation with the given transaction
        /// </summary>
        public Evaluation(DQERepository dqeRepository,Catalogue c)
        {
            DQERepository = dqeRepository;

            using(var con = dqeRepository.GetConnection())
            {
                DateOfEvaluation = DateTime.Now;
                Catalogue = c;

                var cmd = DatabaseCommandHelper.GetCommand("INSERT INTO Evaluation(DateOfEvaluation,CatalogueID) VALUES (@dateOfEvaluation," + c.ID + "); SELECT @@IDENTITY;", con.Connection, con.Transaction);
                DatabaseCommandHelper.AddParameterWithValueToCommand("@dateOfEvaluation",cmd,DateOfEvaluation);

                ID = int.Parse(cmd.ExecuteScalar().ToString());
            }
        }

        #region Getting values out of the database
        
        //reading data out constructor
        internal Evaluation(DQERepository dqeRepository, DbDataReader r)
        {
            DQERepository = dqeRepository;

            ID = int.Parse(r["ID"].ToString());
            DateOfEvaluation = DateTime.Parse(r["DateOfEvaluation"].ToString());

            int catalogueID = int.Parse(r["CatalogueID"].ToString());

            try
            {
                Catalogue = DQERepository.CatalogueRepository.GetObjectByID<Catalogue>(catalogueID);
            }
            catch (Exception e)
            {
                throw new Exception("Could not create a DataQualityEngine.Evaluation for Evaluation with ID "+ID+" because it is a report of an old Catalogue that has been deleted or otherwise does not exist/could not be retrieved (CatalogueID was:" + catalogueID+").  See inner exception for full details",e);
            }
        }
        #endregion

        public void AddRowState( int dataLoadRunID, int correct, int missing, int wrong, int invalid, string validatorXml,string pivotCategory,DbConnection con, DbTransaction transaction)
        {
            new RowState(this, dataLoadRunID, correct, missing, wrong, invalid, validatorXml, pivotCategory, con, transaction);
        }

        public string[] GetPivotCategoryValues()
        {
                List<string> toReturn = new List<string>();
                string sql = "select distinct PivotCategory From RowState where Evaluation_ID  = " + ID;

                using (var con = DQERepository.GetConnection())
                {
                    var cmd = DatabaseCommandHelper.GetCommand(sql, con.Connection, con.Transaction);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                            toReturn.Add((string) r["PivotCategory"]);
                    }
                }

                return toReturn.ToArray();
            
        }

        public void DeleteInDatabase()
        {
            int affectedRows = DQERepository.Delete("DELETE FROM Evaluation where ID = " + ID);

            if(affectedRows == 0)
                throw new Exception("Delete statement resulted in " + affectedRows + " affected rows");
        }
    }
}
