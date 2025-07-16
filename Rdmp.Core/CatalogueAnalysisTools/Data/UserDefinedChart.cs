using CommandLine;
using Microsoft.Data.SqlClient;
using NPOI.OpenXmlFormats.Dml.Diagram;
using NPOI.SS.Formula.Functions;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.CatalogueAnalysisTools.Data
{
    public class UserDefinedChart : DatabaseEntity
    {
        private DQERepository _DQERepository { get; set; }
        private Catalogue _catalogue;
        private string _queryString;
        private int _chartType; //SeriesChartType
        private string _title;
        private string _seriesName;


        [NoMappingToDatabase]
        public Catalogue Catalogue { get => _catalogue; set => SetField(ref _catalogue, value); }
        public String QueryString { get => _queryString; set => SetField(ref _queryString, value); }
        public String Title { get => _title; set => SetField(ref _title, value); }
        public String SeriesName { get => _seriesName; set => SetField(ref _seriesName, value); }
        public int ChartType { get => _chartType; set => SetField(ref _chartType, value); }

        public void Generate()
        {
            //seems to be an issue with the dbdatareader already being open
            var existingResults = _DQERepository.GetAllObjectsWhere<UserDefinedChartResult>("UserDefinedChart_ID", this.ID);
            foreach(var result in existingResults)
            {
                result.DeleteInDatabase();
            }
            var server = _catalogue.GetDistinctLiveDatabaseServer(ReusableLibraryCode.DataAccess.DataAccessContext.InternalDataProcessing,false);
            var con = server.GetConnection();
            con.Open();
            var cmd = server.GetCommand(_queryString, con);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                var y = r[r.GetName(0)].ToString();
                var x = r[r.GetName(1)].ToString();
                var result = new UserDefinedChartResult(new DQERepository(_DQERepository.CatalogueRepository), this, x, y);
                result.SaveToDatabase();
            }
        }


        public DataTable GetResults()
        {
            //seems to be an issue with the dbdatareader already being open
            var dt = new DataTable();
            dt.Columns.Add("X");
            dt.Columns.Add("Y");
            var results = _DQERepository.GetAllObjectsWhere<UserDefinedChartResult>("UserDefinedChart_ID", this.ID);
            foreach(var result in results)
            {
                dt.Rows.Add([result.X, result.Y]);
            }

            return dt;
        }

        public UserDefinedChart(DQERepository repository, Catalogue catalogue, string queryString, int chartType,string title = null, string seriesName = null)
        {
            _DQERepository = repository;
            _catalogue = catalogue;
            _queryString = queryString;
            _title = title;
            _seriesName = seriesName;
            _chartType = chartType;

            _DQERepository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"Catalogue_ID",catalogue.ID },
                {"QueryString",queryString },
                {"Title",title },
                {"SeriesName",seriesName },
                {"ChartType",chartType }
            });
        }

        public UserDefinedChart(DQERepository repository, DbDataReader r): base(repository, r)
        {
            _DQERepository = repository;
            _catalogue = _DQERepository.CatalogueRepository.GetObjectByID<Catalogue>(int.Parse(r["Catalogue_ID"].ToString()));
            _queryString = r["QueryString"].ToString();
            _title = r["Title"].ToString();
            _seriesName = r["SeriesName"].ToString();
            _chartType = int.Parse(r["ChartType"].ToString());
        }

    }
}
