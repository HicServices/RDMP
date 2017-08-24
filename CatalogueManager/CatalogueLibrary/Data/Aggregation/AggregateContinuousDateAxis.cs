using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using Microsoft.Office.Interop.Excel;
using MySql.Data.MySqlClient;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Aggregation;

namespace CatalogueLibrary.Data.Aggregation
{
    /// <summary>
    /// Each AggregateDimension in an AggregateConfiguration can have a defined date axis, this specifies the start/end and increment of the aggregate e.g.
    /// PrescribedDate dimension may have an axis defining it as running from 2001-2009 in increments of 1 month.  
    /// 
    /// For this to work the AggregateDimension output data should be of type a date also.
    /// </summary>
    public class AggregateContinuousDateAxis: DatabaseEntity, IQueryAxis
    {
    

        #region Database Properties
        private int _aggregateDimensionID;
        private string _startDate;
        private string _endDate;
        private AxisIncrement _axisIncrement;

        public int AggregateDimension_ID
        {
            get { return _aggregateDimensionID; }
            set { SetField(ref  _aggregateDimensionID, value); }
        }

        public string StartDate
        {
            get { return _startDate; }
            set { SetField(ref  _startDate, value); }
        }

        public string EndDate
        {
            get { return _endDate; }
            set { SetField(ref  _endDate, value); }
        }

        public AxisIncrement AxisIncrement
        {
            get { return _axisIncrement; }
            set { SetField(ref  _axisIncrement, value); }
        }

        #endregion

        #region Relationships
        [NoMappingToDatabase]
        public AggregateDimension AggregateDimension { get{return Repository.GetObjectByID<AggregateDimension>(AggregateDimension_ID);}}
        #endregion


        public AggregateContinuousDateAxis(ICatalogueRepository repository,AggregateDimension dimension)
        {
            repository.InsertAndHydrate(this, 
                new Dictionary<string, object>()
                {
                    {"AggregateDimension_ID",dimension.ID}
                });
        }


        public AggregateContinuousDateAxis(ICatalogueRepository repository,DbDataReader r) : base(repository,r)
        {
            AggregateDimension_ID = int.Parse(r["AggregateDimension_ID"].ToString());
            StartDate = r["StartDate"].ToString();
            EndDate = r["EndDate"].ToString();
            AxisIncrement = (AxisIncrement) r["AxisIncrement"];
        }
    }
}
