// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.DataQualityEngine.Data;
using Rdmp.Core.Repositories.Construction;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Repositories
{
    /// <summary>
    /// Pointer to the Data Qualilty Engine Repository database in which all DatabaseEntities related to Data Quality Engine runs are stored (e.g. Evaluation).  Every 
	/// DatabaseEntity class must exist in a Microsoft Sql Server Database (See DatabaseEntity) and each object is compatible only with a specific type of TableRepository
	///	(i.e. the database that contains the table matching their name).
    /// 
    /// <para>This class allows you to fetch objects and should be passed into constructors of classes you want to construct in the Data Quality database.</para>
    /// 
    /// <para>Data Qualilty Engine databases are only valid when you have a CatalogueRepository database too and are always paired to a specific CatalogueRepository database (i.e. 
    /// there are IDs in the dqe database that specifically map to objects in the Catalogue database).  You can use the CatalogueRepository property to fetch/create objects
    /// in the paired Catalogue database.</para>
    /// </summary>
    public class DQERepository : TableRepository
    {
        public ICatalogueRepository CatalogueRepository { get; private set; }

        public DQERepository(ICatalogueRepository catalogueRepository)
        {
            CatalogueRepository = catalogueRepository;

            var server = CatalogueRepository.GetServerDefaults().GetDefaultFor(PermissableDefaults.DQE);

            if (server == null)
                throw new NotSupportedException("There is no DataQualityEngine Reporting Server (ExternalDatabaseServer).  You will need to create/set one in CatalogueManager by using 'Locations=>Manage External Servers...'");

             DiscoveredServer = DataAccessPortal.GetInstance().ExpectServer(server, DataAccessContext.InternalDataProcessing);
             _connectionStringBuilder = DiscoveredServer.Builder;
        }

        public virtual Evaluation GetMostRecentEvaluationFor(ICatalogue c)
        {
            return GetEvaluationsWhere("where DateOfEvaluation = (select MAX(DateOfEvaluation) from Evaluation where CatalogueID = " + c.ID + ")").SingleOrDefault();
        }

        public IEnumerable<Evaluation> GetAllEvaluationsFor(ICatalogue catalogue)
        {
            return GetEvaluationsWhere("where CatalogueID = " + catalogue.ID + " order by DateOfEvaluation asc");
        }

        public bool HasEvaluations(ICatalogue catalogue)
        {
            using (var con = GetConnection())
            {
                //get all the row level data 1 to 1 join with evaluation
                using(var cmdGetEvaluations = DatabaseCommandHelper.GetCommand("select count(*) from Evaluation where CatalogueID = " + catalogue.ID ,con.Connection, con.Transaction))
                    return Convert.ToInt32(cmdGetEvaluations.ExecuteScalar()) > 0;
            }
        }

        public IEnumerable<Evaluation> GetEvaluationsWhere(string whereSQL)
        {
            
            List<Evaluation> toReturn = new List<Evaluation>();

            using(var con = GetConnection())
            {
                //get all the row level data 1 to 1 join with evaluation
                using(var cmdGetEvaluations = DatabaseCommandHelper.GetCommand("select * from Evaluation " + whereSQL, con.Connection, con.Transaction))
                    using (DbDataReader r = cmdGetEvaluations.ExecuteReader())
                    {

                        while (r.Read())
                        {
                            Evaluation toAdd = new Evaluation(this, r);
                            toReturn.Add(toAdd);
                        }
                    }
                
                //use a separate command to read the children to prevent multiple active results sets problems
                foreach (Evaluation evaluation in toReturn)
                {
                    List<RowState> states = new List<RowState>();

                    //get all the row level data
                    using (var cmdGetRowStates = DatabaseCommandHelper.GetCommand(
                        "select * from RowState WHERE Evaluation_ID =" + evaluation.ID, con.Connection,
                        con.Transaction))
                    {
                        using(var r2 = cmdGetRowStates.ExecuteReader())
                            while (r2.Read())
                                states.Add(new RowState(r2));
                    }
                    
                    evaluation.RowStates = states.ToArray();

                    //get all the column level data
                    using(var cmdGetColumnStates = DatabaseCommandHelper.GetCommand("select * from ColumnState WHERE ColumnState.Evaluation_ID =" + evaluation.ID, con.Connection, con.Transaction))
                        using(var r2 = cmdGetColumnStates.ExecuteReader())
                        {
                            List<ColumnState> columnStates = new List<ColumnState>();

                            while (r2.Read())
                                columnStates.Add(new ColumnState(r2));

                            evaluation.ColumnStates = columnStates.ToArray();
                            r2.Close();
                        }
                }
            }

            return toReturn;
        }

        private readonly ObjectConstructor _constructor = new ObjectConstructor();
        protected override IMapsDirectlyToDatabaseTable ConstructEntity(Type t, DbDataReader reader)
        {
            return _constructor.ConstructIMapsDirectlyToDatabaseObject(t,this, reader);
        }


        public bool HasAnyEvaluations(Catalogue catalogue)
        {
            using (var con = GetConnection())
            {
                return Convert.ToBoolean(DiscoveredServer.GetCommand("SELECT case when exists (select 1 from Evaluation where CatalogueID = "+catalogue.ID+") then 1 else 0 end", con).ExecuteScalar());
            }
        }
    }
}
