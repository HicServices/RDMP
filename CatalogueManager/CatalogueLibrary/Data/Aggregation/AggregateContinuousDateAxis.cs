using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Aggregation;

namespace CatalogueLibrary.Data.Aggregation
{
    /// <summary>
    /// Each AggregateConfiguration can have a single AggregateDimension defined as a date axis, this specifies the start/end and increment of the aggregate e.g.
    /// PrescribedDate dimension may have an axis defining it as running from 2001-2009 in increments of 1 month.  
    /// 
    /// <para>For this to work the AggregateDimension output data should be of type a date also.</para>
    /// </summary>
    public class AggregateContinuousDateAxis: DatabaseEntity, IQueryAxis
    {
    

        #region Database Properties
        private int _aggregateDimensionID;
        private string _startDate;
        private string _endDate;
        private AxisIncrement _axisIncrement;

        /// <summary>
        /// The column (<see cref="AggregateDimension"/> in the AggregateConfiguration which this axis is defined on.  The AggregateContinuousDateAxis defines 
        /// the axis (e.g.  2001-01-01 to GetDate() by Month ) while the AggregateDimension_ID is the pointer to the column on which the axis applies within the
        /// query. 
        /// </summary>
        public int AggregateDimension_ID
        {
            get { return _aggregateDimensionID; }
            set { SetField(ref  _aggregateDimensionID, value); }
        }

        /// <summary>
        /// The date or scalar function determining what date the graph axis should start at.  This could be as simple as '2001-01-01' or complex like dateadd(yy, -1, GetDate())
        /// </summary>
        public string StartDate
        {
            get { return _startDate; }
            set { SetField(ref  _startDate, value); }
        }

        /// <summary>
        /// The date or scalar function determining what date the graph axis should end at.  This could be as simple as '2001-01-01' or complex GetDate()
        /// </summary>
        public string EndDate
        {
            get { return _endDate; }
            set { SetField(ref  _endDate, value); }
        }

        /// <summary>
        /// Defines the increment of the axis which will be continuous buckets between <see cref="StartDate"/> and <see cref="EndDate"/>.
        /// </summary>
        public AxisIncrement AxisIncrement
        {
            get { return _axisIncrement; }
            set { SetField(ref  _axisIncrement, value); }
        }

        #endregion

        #region Relationships
        /// <inheritdoc cref="AggregateDimension_ID"/>
        [NoMappingToDatabase]
        public AggregateDimension AggregateDimension { get{return Repository.GetObjectByID<AggregateDimension>(AggregateDimension_ID);}}
        #endregion

        /// <summary>
        /// Defines that the specified column (<see cref="AggregateDimension"/>) should function as the continuous axis of an <see cref="AggregateConfiguration"/> graph. 
        /// For example if you are graphing the number of prescriptions given out each month then the axis would be applied to the 'PrescribedDate' <see cref="AggregateDimension"/>
        ///  </summary>
        /// <remarks>To use this you will first have to create an AggregateConfiguration and setup the count(*)/sum(*) etc stuff and then add a new AggregateDimension <see cref="AggregateConfiguration.AddDimension"/> </remarks>
        /// <param name="repository"></param>
        /// <param name="dimension"></param>
        public AggregateContinuousDateAxis(ICatalogueRepository repository,AggregateDimension dimension)
        {
            repository.InsertAndHydrate(this, 
                new Dictionary<string, object>()
                {
                    {"AggregateDimension_ID",dimension.ID}
                });
        }
        
        internal AggregateContinuousDateAxis(ICatalogueRepository repository,DbDataReader r) : base(repository,r)
        {
            AggregateDimension_ID = int.Parse(r["AggregateDimension_ID"].ToString());
            StartDate = r["StartDate"].ToString();
            EndDate = r["EndDate"].ToString();
            AxisIncrement = (AxisIncrement) r["AxisIncrement"];
        }
    }
}
