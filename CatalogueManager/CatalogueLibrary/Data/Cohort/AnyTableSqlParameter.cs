using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data.Cohort
{
    /// <summary>
    /// Allows you to override ALL instances of a given named parameter e.g. @studyStartDate in ALL AggregateFilterParameters in a given CohortIdentificationConfiguration
    /// with a single value.  This allows you to have multiple filters in different datasets that all use @studyStartDate parameter but override it globally for the configuration
    /// so that you don't have to manually update every parameter when you want to change your study criteria.  For this to work all AggregateFilterParameters must have the same name
    /// and datatype AND comment! as the study filters (see CohortQueryBuilder).
    /// </summary>
    public class AnyTableSqlParameter : VersionedDatabaseEntity, ISqlParameter,IHasDependencies
    {
        #region Database Properties

        private int _parentID;
        private string _parentTable;
        private string _parameterSQL;
        private string _value;
        private string _comment;

        public int Parent_ID
        {
            get { return _parentID; }
            set { SetField(ref  _parentID, value); }
        }

        public string ParentTable
        {
            get { return _parentTable; }
            set { SetField(ref  _parentTable, value); }
        }

        public string ParameterSQL
        {
            get { return _parameterSQL; }
            set { SetField(ref  _parameterSQL, value); }
        }

        public string Value
        {
            get { return _value; }
            set { SetField(ref  _value, value); }
        }

        public string Comment
        {
            get { return _comment; }
            set { SetField(ref  _comment, value); }
        }


        #endregion


        [NoMappingToDatabase]
        public string ParameterName
        {
            get { return RDMPQuerySyntaxHelper.GetParameterNameFromDeclarationSQL(ParameterSQL); }
        }

        public AnyTableSqlParameter(ICatalogueRepository repository, IMapsDirectlyToDatabaseTable parent, string parameterSQL)
        {
            if (!RDMPQuerySyntaxHelper.IsValidParameterName(parameterSQL))
                throw new ArgumentException("parameterSQL is not valid \"" + parameterSQL + "\"");

            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"ParameterSQL", parameterSQL},
                {"Parent_ID", parent.ID},
                {"ParentTable", parent.GetType().Name}
            });
        }

        public AnyTableSqlParameter(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            Value = r["Value"] as string;
            Parent_ID = (int)r["Parent_ID"];
            ParentTable = r["ParentTable"].ToString();
            ParameterSQL = r["ParameterSQL"] as string;
            Comment = r["Comment"] as string;
        }

        public override string ToString()
        {
            return ParameterName;
        }

        public static bool IsSupportedType(Type type)
        {
            return DescribeUseCaseForParent(type) != null;
        }

        public static string DescribeUseCaseForParent(Type type)
        {
            if (type == typeof(CohortIdentificationConfiguration))
                return "SQLParameters at this level are global for a given cohort identification configuration task e.g. @StudyWindowStartDate which could then be used by 10 datasets within that configuration";

            if (type == typeof (AggregateConfiguration))
                return "SQLParameters at this level are intended for fulfilling table valued function parameters and centralising parameter declarations across multiple AggregateFilter(s) within a single AggregateConfiguration (note that while these are 'global' with respect to the filters, if the AggregateConfiguration is part of a multiple configuration CohortIdentificationConfiguration then this is less 'global' than those declared at that level)";

            if (type == typeof(TableInfo))
                return "SQLParameters at this level are intended for fulfilling table valued function parameters, note that these should/can be overridden later on e.g. in Extraction/Cohort generation.  This value is intended to give a baseline result which can be run through DataQualityEngine and Checking etc";

            return null;
        }

        public IMapsDirectlyToDatabaseTable GetOwnerIfAny()
        {
            var type = typeof (Catalogue).Assembly.GetTypes().Single(t=>t.Name.Equals(ParentTable));

            return ((CatalogueRepository)Repository).GetObjectByID(type,Parent_ID);
        }

        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            return new IHasDependencies[0];
        }

        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            var parent = GetOwnerIfAny() as IHasDependencies;

            if (parent != null)
                return new[] {parent};

            return new IHasDependencies[0];
        }
    }
}
