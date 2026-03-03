using FAnsi;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.DataLoad.Extensions;
using Rdmp.Core.Curation.Data.EntityNaming;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.EntityFramework.Models;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.Providers;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("TableInfo")]
    public class TableInfo : DatabaseObject, ITableInfo
    {
        [Key]
        public override int ID { get; set; }

        [Required]
        [MaxLength(500)]
        public string Name { get; set; }

        public string Database { get; set; } = "0";
        public string Server { get; set; }
        public string DatabaseType { get; set; }

        public string Schema { get; set; }
        public bool IsPrimaryExtractionTable { get; set; }
        //public bool IsLookup { get; set; }

        public virtual ICollection<ColumnInfo> ColumnInfos { get; set; }
        public bool IsTableValuedFunction { get; set; }
        public int? IdentifierDumpServer_ID { get; set; }

        [ForeignKey("IdentifierDumpServer_ID")]
        public virtual ExternalDatabaseServer IdentifierDumpServer { get; set; }

        public PreLoadDiscardedColumn[] PreLoadDiscardedColumns => Array.Empty<PreLoadDiscardedColumn>();

        public bool IsView { get; set; }

        [NotMapped]
        DatabaseType IDataAccessPoint.DatabaseType { get; set; }//todo want to gety rid of this

        ColumnInfo[] ITableInfo.ColumnInfos => this.ColumnInfos.ToArray();

        public IQuerySyntaxHelper GetQuerySyntaxHelper() => QuerySyntaxHelperFactory.Create(GetDatabaseType());

        public FAnsi.DatabaseType GetDatabaseType() => FAnsi.DatabaseType.MicrosoftSQLServer;//(FAnsi.DatabaseType)Enum.Parse(typeof(DatabaseTypeEnum), DatabaseType);
        public override string ToString() => Name;

        public string GetRuntimeName() => GetQuerySyntaxHelper().GetRuntimeName(Name);

        public string GetFullyQualifiedName() =>
    GetQuerySyntaxHelper().EnsureFullyQualified(Database, Schema, GetRuntimeName());

        public string GetRuntimeName(LoadBubble bubble, INameDatabasesAndTablesDuringLoads tableNamingScheme = null)
        {
            tableNamingScheme ??= new FixedStagingDatabaseNamer(Database);

            var baseName = GetQuerySyntaxHelper().GetRuntimeName(Name);

            return tableNamingScheme.GetName(baseName, bubble);
        }

        public bool IsLookupTable()
        {
            throw new NotImplementedException();
        }

        public bool IsLookupTable(ICoreChildProvider childProvider)
        {
            throw new NotImplementedException();
        }

        public string GetDatabaseRuntimeName(LoadStage loadStage, INameDatabasesAndTablesDuringLoads namer = null)
        {
            var baseName = GetDatabaseRuntimeName();

            namer ??= new FixedStagingDatabaseNamer(baseName);

            return namer.GetDatabaseName(baseName, loadStage.ToLoadBubble());
        }

        public IEnumerable<IHasStageSpecificRuntimeName> GetColumnsAtStage(LoadStage loadStage)
        {
            throw new NotImplementedException();
        }

        public DiscoveredTable Discover(DataAccessContext context)
        {
            throw new NotImplementedException();
        }

        public string GetDatabaseRuntimeName()
        {
            throw new NotImplementedException();
        }

        public Curation.Data.Catalogue[] GetAllRelatedCatalogues()
        {
            throw new NotImplementedException();
        }

        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }

        public IDataAccessCredentials GetCredentialsIfExists(DataAccessContext context)
        {
            throw new NotImplementedException();
        }

        public bool DiscoverExistence(DataAccessContext context, out string reason)
        {
            throw new NotImplementedException();
        }

        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            throw new NotImplementedException();
        }

        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            throw new NotImplementedException();
        }

        public ISqlParameter[] GetAllParameters()
        {
            throw new NotImplementedException();
        }

        public void RevertToDatabaseState()
        {
            throw new NotImplementedException();
        }

        public RevertableObjectReport HasLocalChanges()
        {
            throw new NotImplementedException();
        }

        public bool Exists()
        {
            throw new NotImplementedException();
        }

        public void SaveToDatabase()
        {
            throw new NotImplementedException();
        }

        public void ClearAllInjections()
        {
            throw new NotImplementedException();
        }

        public void Check(ICheckNotifier notifier)
        {
            throw new NotImplementedException();
        }

        public string GetRuntimeName(LoadStage stage, INameDatabasesAndTablesDuringLoads tableNamingScheme = null)
        {
            throw new NotImplementedException();
        }

        Catalogue[] ITableInfo.GetAllRelatedCatalogues()
        {
            throw new NotImplementedException();
        }
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
