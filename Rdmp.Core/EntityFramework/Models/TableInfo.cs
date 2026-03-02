using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.EntityFramework.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("TableInfo")]
    public class TableInfo: DatabaseObject
    {
        [Key]
        public override int ID { get; set; }

        [Required]
        [MaxLength(500)]
        public string Name { get; set; }

        public string Database { get; set; }
        public string Server { get; set; }
        public string DatabaseType { get; set; }
        //public bool IsLookup { get; set; }

        public virtual ICollection<ColumnInfo> ColumnInfos { get; set; }

        public IQuerySyntaxHelper GetQuerySyntaxHelper() => QuerySyntaxHelperFactory.Create((FAnsi.DatabaseType)Enum.Parse(typeof(DatabaseTypeEnum),DatabaseType));
    }

    public enum DatabaseTypeEnum
    {
        /// <summary>
        /// Any Microsoft Sql Server database (e.g. Express etc).  Does not include Access.
        /// </summary>
        MicrosoftSQLServer,

        /// <summary>
        /// My Sql database engine.
        /// </summary>
        MySql,

        /// <summary>
        /// Oracle database engine
        /// </summary>
        Oracle,

        /// <summary>
        /// PostgreSql database engine
        /// </summary>
        PostgreSql
    }

}
