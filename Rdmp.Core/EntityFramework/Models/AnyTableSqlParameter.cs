using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("AnyTableSqlParameter")]
    public class AnyTableSqlParameter : ReferenceOtherObjectDatabaseObject, ISqlParameter, IHasDependencies
    {
        /// <summary>
        /// Names that are not allowed for user custom parameters because they
        /// are used internally by RMDP query building engines
        /// </summary>
        public static readonly string[] ProhibitedParameterNames =
        {
        "@CohortDefinitionID",
        "@ProjectNumber",
        "@dateAxis",
        "@currentDate",
        "@dbName",
        "@sql",
        "@isPrimaryKeyChange",
        "@Query",
        "@Columns",
        "@value",
        "@pos",
        "@len",
        "@startDate",
        "@endDate"
    };

        [NotMapped]
        public string ParameterName => throw new NotImplementedException();
        /// <summary>
        /// The default value to give to parameters when creating new blank/unknown role
        /// </summary>
        public const string DefaultValue = "'todo'";
        public string ParameterSQL { get; set; }
        public string Value { get; set; }
        public string Comment { get; set; }

        public void Check(ICheckNotifier notifier)
        {
            throw new NotImplementedException();
        }

        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            throw new NotImplementedException();
        }

        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            throw new NotImplementedException();
        }

        public IMapsDirectlyToDatabaseTable GetOwnerIfAny()
        {
            throw new NotImplementedException();
        }

        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            throw new NotImplementedException();
        }
    }
}
