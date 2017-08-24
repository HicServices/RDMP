using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Construction;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;

namespace DataQualityEngine.Data
{
    public class DQERepository : TableRepository
    {
        public CatalogueRepository CatalogueRepository { get; private set; }

        public DQERepository(CatalogueRepository catalogueRepository)
        {
            CatalogueRepository = catalogueRepository;
            
            ServerDefaults defaults = new ServerDefaults(CatalogueRepository);
            var server = defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.DQE);

            if (server == null)
                throw new NotSupportedException("There is no DataQualityEngine Reporting Server (ExternalDatabaseServer).  You will need to create/set one in CatalogueManager by using 'Locations=>Manage External Servers...'");

             DiscoveredServer = DataAccessPortal.GetInstance().ExpectServer(server, DataAccessContext.InternalDataProcessing);
             _connectionStringBuilder = DiscoveredServer.Builder;

            catalogueRepository.AddToHelp(GetType().Assembly);
        }

        public Evaluation GetMostRecentEvaluationFor(Catalogue c)
        {
            return GetEvaluationsWhere("where DateOfEvaluation = (select MAX(DateOfEvaluation) from Evaluation where CatalogueID = " + c.ID + ")").SingleOrDefault();
        }

        public IEnumerable<Evaluation> GetAllEvaluationsFor(Catalogue catalogue)
        {
            return GetEvaluationsWhere("where CatalogueID = " + catalogue.ID + " order by DateOfEvaluation asc");
        }

        public IEnumerable<Evaluation> GetEvaluationsWhere(string whereSQL)
        {
            
            List<Evaluation> toReturn = new List<Evaluation>();

            using(var con = GetConnection())
            {
                //get all the row level data 1 to 1 join with evaluation
                var cmdGetEvaluations = DatabaseCommandHelper.GetCommand("select * from Evaluation " + whereSQL, con.Connection, con.Transaction);
                using (DbDataReader r = cmdGetEvaluations.ExecuteReader())
                {

                    while (r.Read())
                    {
                        Evaluation toAdd = new Evaluation(this,r);
                        toReturn.Add(toAdd);
                    }

                    r.Close();
                    
                    //use a separate connection to read the children to prevent multiple active results sets problems
                    foreach (Evaluation evaluation in toReturn)
                    {
                        //get all the row level data
                        var cmdGetRowStates = DatabaseCommandHelper.GetCommand("select * from RowState WHERE Evaluation_ID =" + evaluation.ID, con.Connection, con.Transaction);

                        var r2 = cmdGetRowStates.ExecuteReader();
                        List<RowState> states = new List<RowState>();

                        while (r2.Read())
                            states.Add(new RowState(r2));
                        r2.Close();
                        evaluation.RowStates = states.ToArray();

                        //get all the column level data
                        var cmdGetColumnStates = DatabaseCommandHelper.GetCommand("select * from ColumnState WHERE ColumnState.Evaluation_ID =" + evaluation.ID, con.Connection, con.Transaction);
                        r2 = cmdGetColumnStates.ExecuteReader();

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