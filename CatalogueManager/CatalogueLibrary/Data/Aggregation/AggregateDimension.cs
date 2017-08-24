using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data.Aggregation
{
    /// <summary>
    /// This class allows you to associate a specific extractioninformation for use in aggregate generation.  For example a dataset might have a date field AdmissionDate which you
    /// want to create an aggregate configuration (when patients were admitted) over time.  However the class also allows you to specify new SelectSQL which can change how the field
    /// is extracted e.g. you might want to change "[MyDatabase].[MyTable].[AdmissionDate]" into "YEAR([MyDatabase].[MyTable].[AdmissionDate]) as AdmissionDate" 
    /// </summary>
    public class AggregateDimension : VersionedDatabaseEntity, ISaveable, IDeleteable, IColumn,IHasDependencies
    {
        #region Database Properties

        private int _aggregateConfigurationID;
        private int _extractionInformationID;
        private string _alias;
        private string _selectSQL;
        private int _order;

        public int AggregateConfiguration_ID
        {
            get { return _aggregateConfigurationID; }
            set { SetField(ref  _aggregateConfigurationID, value); }
        }

        public int ExtractionInformation_ID
        {
            get { return _extractionInformationID; }
            set { SetField(ref  _extractionInformationID, value); }
        }

        public string Alias
        {
            get { return _alias; }
            set { SetField(ref  _alias, value); }
        }

        public string SelectSQL
        {
            get { return _selectSQL; }
            set { SetField(ref  _selectSQL, value); }
        }

        public int Order
        {
            get { return _order; }
            set { SetField(ref  _order, value); }
        }
        #endregion

        //IExtractableColumn stuff (which references the underlying extractionInformation - does not appear in table but fetches it from the other objects table)
        private ExtractionInformation _extractionInformation;
        

        #region Relationships

        [NoMappingToDatabase]
        public bool HashOnDataRelease { get{CacheExtractionInformation(); return _extractionInformation.HashOnDataRelease; } }

        [NoMappingToDatabase]
        public bool IsExtractionIdentifier { get { CacheExtractionInformation(); return _extractionInformation.IsExtractionIdentifier; } }
        [NoMappingToDatabase]
        public bool IsPrimaryKey { get { CacheExtractionInformation(); return _extractionInformation.IsPrimaryKey; } }
        [NoMappingToDatabase]
        public ColumnInfo ColumnInfo { get { CacheExtractionInformation(); return _extractionInformation.ColumnInfo; } }

        [NoMappingToDatabase]
        public AggregateContinuousDateAxis AggregateContinuousDateAxis
        {
            get
            {
                return Repository.GetAllObjectsWithParent<AggregateContinuousDateAxis>(this).SingleOrDefault();
            }
        }

        [NoMappingToDatabase]
        public ExtractionInformation ExtractionInformation {get { return Repository.GetObjectByID<ExtractionInformation>(ExtractionInformation_ID); } }

        [NoMappingToDatabase]
        public AggregateConfiguration AggregateConfiguration { get { return Repository.GetObjectByID<AggregateConfiguration>(AggregateConfiguration_ID); } }

        #endregion

        public AggregateDimension(ICatalogueRepository repository, ExtractionInformation basedOnColumn, AggregateConfiguration configuration)
        {
            object alias = DBNull.Value;
            if (basedOnColumn.Alias != null) alias = basedOnColumn.Alias;

            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"AggregateConfiguration_ID", configuration.ID},
                {"ExtractionInformation_ID", basedOnColumn.ID},
                {"SelectSQL", basedOnColumn.SelectSQL},
                {"Alias", alias},
                {"Order", basedOnColumn.Order}
            });
        }

        public AggregateDimension(ICatalogueRepository repository,DbDataReader r) : base(repository,r)
        {
            AggregateConfiguration_ID = int.Parse(r["AggregateConfiguration_ID"].ToString());
            ExtractionInformation_ID = int.Parse(r["ExtractionInformation_ID"].ToString());
            
            SelectSQL = r["SelectSQL"] as string;
            Alias = r["Alias"] as string;

            Order = int.Parse(r["Order"].ToString());
        }
    
        public string GetRuntimeName()
        {
            return RDMPQuerySyntaxHelper.GetRuntimeName(this);
        }

        public override string ToString()
        {
            try
            {
                return GetRuntimeName();
            }
            catch (Exception)
            {
                return "Unamed AggregateDimension ID " + ID;
            }
        }
        
        private void CacheExtractionInformation()
        {
            if (_extractionInformation == null)
                //there is a cascade delete on the relationship between extraction informations down into dimensions that should prevent the user deleting the extraction information and it leaving an orphans defined in an aggregate.
                _extractionInformation = Repository.GetObjectByID<ExtractionInformation>(ExtractionInformation_ID);
        }

        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            return new[] {ExtractionInformation};
        }

        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            return new[] { AggregateConfiguration };
        }

        
    }
}
