// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Cohort.Joinables;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.Defaults;
using CatalogueLibrary.Data.Governance;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.Data.Referencing;
using CatalogueLibrary.Data.Remoting;
using CatalogueLibrary.Data.Serialization;
using CatalogueLibrary.Properties;
using CatalogueLibrary.Repositories.Construction;
using CatalogueLibrary.Repositories.Managers;
using HIC.Logging;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using ReusableLibraryCode;
using ReusableLibraryCode.Comments;
using ReusableLibraryCode.Extensions;

namespace CatalogueLibrary.Repositories
{
    /// <summary>
    /// Pointer to the Catalogue Repository database in which all DatabaseEntities declared in CatalogueLibrary.dll are stored.  Ever DatabaseEntity class must exist in a
    /// Microsoft Sql Server Database (See DatabaseEntity) and each object is compatible only with a specific type of TableRepository (i.e. the database that contains the
    /// table matching their name).  CatalogueLibrary.dll objects in CatalogueRepository, DataExportLibrary.dll objects in DataExportRepository, DataQualityEngine.dll objects
    /// in DQERepository etc.
    /// 
    /// <para>This class allows you to fetch objects and should be passed into constructors of classes you want to construct in the Catalogue database.  </para>
    /// 
    /// <para>It also includes helper properties for setting up relationships and controling records in the non DatabaseEntity tables in the database e.g. <see cref="AggregateForcedJoinManager"/></para>
    /// </summary>
    public class CatalogueRepository : TableRepository, ICatalogueRepository
    {
        /// <inheritdoc/>
        public IAggregateForcedJoinManager AggregateForcedJoinManager { get; private set; }

        /// <inheritdoc/>
        public IGovernanceManager GovernanceManager { get; private set; }

        /// <inheritdoc/>
        public ITableInfoCredentialsManager TableInfoCredentialsManager { get; private set; }

        
        /// <inheritdoc/>
        public IJoinManager JoinManager { get; set; }

        /// <inheritdoc/>
        public MEF MEF { get; set; }
        
        readonly ObjectConstructor _constructor = new ObjectConstructor();

        /// <inheritdoc/>
        public CommentStore CommentStore { get; set; }

        /// <inheritdoc/>
        public ICohortContainerManager CohortContainerManager { get; private set; }

        /// <summary>
        /// By default CatalogueRepository will execute DocumentationReportMapsDirectlyToDatabase which will load all the Types and find documentation in the source code for 
        /// them obviously this affects test performance so set this to true if you want it to skip this process.  Note where this is turned on, it's in the static constructor
        /// of DatabaseTests which means if you stick a static constructor in your test you can override it if you need access to the help text somehow in your test
        /// </summary>
        public static bool SuppressHelpLoading;

        /// <inheritdoc/>
        public IFilterManager FilterManager { get; private set; }

        /// <summary>
        /// Sets up an <see cref="IRepository"/> which connects to the database <paramref name="catalogueConnectionString"/> to fetch/create <see cref="DatabaseEntity"/> objects.
        /// </summary>
        /// <param name="catalogueConnectionString"></param>
        public CatalogueRepository(DbConnectionStringBuilder catalogueConnectionString): base(null,catalogueConnectionString)
        {
            AggregateForcedJoinManager = new AggregateForcedJoin(this);
            GovernanceManager = new GovernanceManager(this);
            TableInfoCredentialsManager = new TableInfoCredentialsManager(this);
            JoinManager = new JoinManager(this);
            CohortContainerManager = new CohortContainerManager(this);
            MEF = new MEF();
            FilterManager = new AggregateFilterManager(this);
            
            ObscureDependencyFinder = new CatalogueObscureDependencyFinder(this);
            
            //Shortcuts to improve performance of ConstructEntity (avoids reflection)
            Constructors.Add(typeof(Catalogue),(rep, r) => new Catalogue((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(CohortAggregateContainer),(rep,r)=>new CohortAggregateContainer((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(CohortIdentificationConfiguration),(rep,r)=>new CohortIdentificationConfiguration((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(GovernanceDocument),(rep,r)=>new GovernanceDocument((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(GovernancePeriod),(rep,r)=>new GovernancePeriod((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(StandardRegex),(rep,r)=>new StandardRegex((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(AnyTableSqlParameter),(rep,r)=>new AnyTableSqlParameter((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(Plugin),(rep,r)=>new Plugin((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(ANOTable),(rep,r)=>new ANOTable((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(AggregateConfiguration),(rep,r)=>new AggregateConfiguration((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(AggregateContinuousDateAxis),(rep,r)=>new AggregateContinuousDateAxis((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(AggregateDimension),(rep,r)=>new AggregateDimension((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(AggregateFilter),(rep,r)=>new AggregateFilter((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(AggregateFilterContainer),(rep,r)=>new AggregateFilterContainer((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(AggregateFilterParameter),(rep,r)=>new AggregateFilterParameter((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(CatalogueItem),(rep,r)=>new CatalogueItem((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(ColumnInfo),(rep,r)=>new ColumnInfo((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(JoinableCohortAggregateConfiguration),(rep,r)=>new JoinableCohortAggregateConfiguration((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(JoinableCohortAggregateConfigurationUse),(rep,r)=>new JoinableCohortAggregateConfigurationUse((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(ExternalDatabaseServer),(rep,r)=>new ExternalDatabaseServer((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(ExtractionFilter),(rep,r)=>new ExtractionFilter((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(ExtractionFilterParameter),(rep,r)=>new ExtractionFilterParameter((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(ExtractionInformation),(rep,r)=>new ExtractionInformation((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(ExtractionFilterParameterSet),(rep,r)=>new ExtractionFilterParameterSet((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(LoadMetadata),(rep,r)=>new LoadMetadata((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(ExtractionFilterParameterSetValue),(rep,r)=>new ExtractionFilterParameterSetValue((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(LoadModuleAssembly),(rep,r)=>new LoadModuleAssembly((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(LoadProgress),(rep,r)=>new LoadProgress((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(Favourite),(rep,r)=>new Favourite((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(Pipeline),(rep,r)=>new Pipeline((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(Lookup),(rep,r)=>new Lookup((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(AggregateTopX),(rep,r)=>new AggregateTopX((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(PipelineComponent),(rep,r)=>new PipelineComponent((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(LookupCompositeJoinInfo),(rep,r)=>new LookupCompositeJoinInfo((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(PipelineComponentArgument),(rep,r)=>new PipelineComponentArgument((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(PreLoadDiscardedColumn),(rep,r)=>new PreLoadDiscardedColumn((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(ProcessTask),(rep,r)=>new ProcessTask((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(DashboardLayout),(rep,r)=>new DashboardLayout((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(ProcessTaskArgument),(rep,r)=>new ProcessTaskArgument((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(DashboardControl),(rep,r)=>new DashboardControl((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(DataAccessCredentials),(rep,r)=>new DataAccessCredentials((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(SupportingDocument),(rep,r)=>new SupportingDocument((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(DashboardObjectUse),(rep,r)=>new DashboardObjectUse((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(SupportingSQLTable),(rep,r)=>new SupportingSQLTable((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(TableInfo),(rep,r)=>new TableInfo((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(RemoteRDMP),(rep,r)=>new RemoteRDMP((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(ObjectImport),(rep,r)=>new ObjectImport((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(ObjectExport),(rep,r)=>new ObjectExport((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(CacheProgress),(rep,r)=>new CacheProgress((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(ConnectionStringKeyword),(rep,r)=>new ConnectionStringKeyword((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(WindowLayout),(rep,r)=>new WindowLayout((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(PermissionWindow),(rep,r)=>new PermissionWindow((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(TicketingSystemConfiguration),(rep,r)=>new TicketingSystemConfiguration((ICatalogueRepository)rep, r));
            Constructors.Add(typeof(CacheFetchFailure), (rep, r) => new CacheFetchFailure((ICatalogueRepository)rep, r));

        }
        public IEncryptStrings GetEncrypter()
        {
            return new SimpleStringValueEncryption(new PasswordEncryptionKeyLocation(this).OpenKeyFile());
        }

        

        /// <summary>
        /// Initializes and loads <see cref="CommentStore"/> with all the xml doc/dll files found in the provided <paramref name="directories"/> 
        /// </summary>
        /// <param name="directories"></param>
        public void LoadHelp(params string[] directories)
        {
            if (!SuppressHelpLoading)
            {
                CommentStore = new CommentStore();
                CommentStore.ReadComments(directories);
                AddToHelp(Resources.KeywordHelp);
            }
        }

        private void AddToHelp(string keywordHelpFileContents)
        {
            //null is true for us loading help
            if (SuppressHelpLoading)
                return;

            var lines = keywordHelpFileContents.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var split = line.Split(':');

                if (split.Length != 2)
                    throw new Exception("Malformed line in Resources.KeywordHelp, line is:" + Environment.NewLine + line + Environment.NewLine + "We expected it to have exactly one colon in it");

                if (!CommentStore.ContainsKey(split[0]))
                    CommentStore.Add(split[0], split[1]);
            }
        }
        
        
        /// <inheritdoc/>
        public LogManager GetDefaultLogManager()
        {
            ServerDefaults defaults = new ServerDefaults(this);
            return new LogManager(defaults.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID));
        }

        /// <inheritdoc/>
        public Catalogue[] GetAllCatalogues(bool includeDeprecatedCatalogues = false)
        {
            return GetAllObjects<Catalogue>().Where(cata => (!cata.IsDeprecated) || includeDeprecatedCatalogues).ToArray();
        }

        /// <inheritdoc/>
        public Catalogue[] GetAllCataloguesWithAtLeastOneExtractableItem()
        {
            return
                GetAllObjects<Catalogue>(
                    @"WHERE exists (select 1 from CatalogueItem ci where Catalogue_ID = Catalogue.ID AND exists (select 1 from ExtractionInformation where CatalogueItem_ID = ci.ID)) ")
                    .ToArray();
        }

        /// <inheritdoc/>
        public IEnumerable<AnyTableSqlParameter> GetAllParametersForParentTable(IMapsDirectlyToDatabaseTable parent)
        {
            var type = parent.GetType();

            if (!AnyTableSqlParameter.IsSupportedType(type))
                throw new NotSupportedException("This table does not support parents of type " + type.Name);

            return GetReferencesTo<AnyTableSqlParameter>(parent);
        }

        /// <inheritdoc/>
        public TicketingSystemConfiguration GetTicketingSystem()
        {
            var configuration = GetAllObjects<TicketingSystemConfiguration>().Where(t => t.IsActive).ToArray();

            if (configuration.Length == 0)
                return null;

            if (configuration.Length == 1)
                return configuration[0];

            throw new NotSupportedException("There should only ever be one active ticketing system, something has gone very wrong, there are currently " + configuration.Length);
        }
        
        protected override IMapsDirectlyToDatabaseTable ConstructEntity(Type t, DbDataReader reader)
        {
            if (Constructors.ContainsKey(t))
                return Constructors[t](this, reader);

            return _constructor.ConstructIMapsDirectlyToDatabaseObject<ICatalogueRepository>(t, this, reader);
        }

        protected override bool IsCompatibleType(Type type)
        {
            return typeof (DatabaseEntity).IsAssignableFrom(type);
        }

        public ExternalDatabaseServer[] GetAllTier2Databases(Tier2DatabaseType type)
        {
            var servers = GetAllObjects<ExternalDatabaseServer>();
            string assembly;

            switch (type)
            {
                case Tier2DatabaseType.Logging:
                    assembly = "HIC.Logging.Database";
                    break;
                case Tier2DatabaseType.DataQuality:
                    assembly = "DataQualityEngine.Database";
                    break;
                case Tier2DatabaseType.QueryCaching:
                    assembly = "QueryCaching.Database";
                    break;
                case Tier2DatabaseType.ANOStore:
                    assembly = "ANOStore.Database";
                    break;
                case Tier2DatabaseType.IdentifierDump:
                    assembly = "IdentifierDump.Database";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }

            return servers.Where(s => s.CreatedByAssembly == assembly).ToArray();
        }
        
        public void UpsertAndHydrate<T>(T toCreate, ShareManager shareManager, ShareDefinition shareDefinition) where T : class,IMapsDirectlyToDatabaseTable
        {
            //Make a dictionary of the normal properties we are supposed to be importing
            Dictionary<string,object> propertiesDictionary = shareDefinition.GetDictionaryForImport();

            //for finding properties decorated with [Relationship]
            var finder = new AttributePropertyFinder<RelationshipAttribute>(toCreate);
            
            //If we have already got a local copy of this shared object?
            //either as an import or as an export
            T actual = (T)shareManager.GetExistingImportObject(shareDefinition.SharingGuid) ?? (T)shareManager.GetExistingExportObject(shareDefinition.SharingGuid);
            
            //we already have a copy imported of the shared object
            if (actual != null)
            {
                //It's an UPDATE i.e. take the new shared properties and apply them to the database copy / memory copy

                //copy all the values out of the share definition / database copy
                foreach (PropertyInfo prop in GetPropertyInfos(typeof(T)))
                {
                    //don't update any ID columns or any with relationships on UPDATE
                    if (propertiesDictionary.ContainsKey(prop.Name) && finder.GetAttribute(prop) == null)
                    {
                        SetValue(prop, propertiesDictionary[prop.Name], toCreate);
                    }
                    else
                        prop.SetValue(toCreate, prop.GetValue(actual)); //or use the database one if it isn't shared (e.g. ID, MyParent_ID etc)

                }

                toCreate.Repository = actual.Repository;
                
                //commit the updated values to the database
                SaveToDatabase(toCreate);
            }
            else
            {
                //It's an INSERT i.e. create a new database copy with the correct foreign key values and update the memory copy
                
                //for each relationship property on the class we are trying to hydrate
                foreach (PropertyInfo property in GetPropertyInfos(typeof(T)))
                {
                    RelationshipAttribute relationshipAttribute = finder.GetAttribute(property);

                    //if it has a relationship attribute then we would expect the ShareDefinition to include a dependency relationship with the sharing UID of the parent
                    //and also that we had already imported it since dependencies must be imported in order
                    if(relationshipAttribute != null)
                    {
                        int? newValue;

                        switch (relationshipAttribute.Type)
                        {
                            case RelationshipType.SharedObject:
                                //Confirm that the share definition includes the knowledge that theres a parent class to this object
                                if (!shareDefinition.RelationshipProperties.ContainsKey(relationshipAttribute))
                                    throw new Exception("Share Definition for object of Type " + typeof(T) + " is missing an expected RelationshipProperty called " + property.Name);

                                //Get the SharingUID of the parent for this property
                                Guid importGuidOfParent = shareDefinition.RelationshipProperties[relationshipAttribute];

                                //Confirm that we have a local import of the parent
                                var parentImport = shareManager.GetExistingImport(importGuidOfParent);

                                if (parentImport == null)
                                    throw new Exception("Cannot import an object of type " + typeof(T) + " because the ShareDefinition specifies a relationship to an object that has not yet been imported (A " + relationshipAttribute.Cref + " with a SharingUID of " + importGuidOfParent);

                                newValue = parentImport.ReferencedObjectID;
                                break;
                            case RelationshipType.LocalReference:
                                newValue = shareManager.GetLocalReference(property, relationshipAttribute, shareDefinition);
                                break;
                            case RelationshipType.IgnoreableLocalReference:
                                newValue = null;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        
                        //get the ID of the local import of the parent
                        if (propertiesDictionary.ContainsKey(property.Name))
                            propertiesDictionary[property.Name] = newValue;
                        else
                            propertiesDictionary.Add(property.Name,newValue);
                    }
                }

                //insert the full dictionary into the database under the Type
                InsertAndHydrate(toCreate,propertiesDictionary);

                //document that a local import of the share now exists and should be updated/reused from now on when that same GUID comes in / gets used by child objects
                shareManager.GetImportAs(shareDefinition.SharingGuid.ToString(), toCreate);
            }
        }

        public Plugin[] GetCompatiblePlugins()
        {
            var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            var plugins = GetAllObjects<Plugin>().Where(p => p.PluginVersion.IsCompatibleWith(new Version(version), 3));
            var uniquePlugins = plugins.GroupBy(p => new { name = p.Name, ver = new Version(p.PluginVersion.Major, p.PluginVersion.Minor, p.PluginVersion.Build) })
                                       .ToDictionary(g => g.Key, p => p.OrderByDescending(pv => pv.PluginVersion).First());
            return uniquePlugins.Values.ToArray();
        }

        public void SetValue(PropertyInfo prop, object value, IMapsDirectlyToDatabaseTable onObject)
        {
            //sometimes json decided to swap types on you e.g. int64 for int32
            var propertyType = prop.PropertyType;

            //if it is a nullable int etc
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof (Nullable<>))
                propertyType = propertyType.GetGenericArguments()[0]; //lets pretend it's just int / whatever

            if (value != null && value != DBNull.Value && !propertyType.IsInstanceOfType(value))
                if (propertyType == typeof(CatalogueFolder))
                {
                    //will be passed as a string
                    value = value is string ? new CatalogueFolder((Catalogue)onObject, (string)value):(CatalogueFolder) value;
                }
                else
                    if (typeof(Enum).IsAssignableFrom(propertyType))
                        value = Enum.ToObject(propertyType, value);//if the property is an enum
                    else
                        value = Convert.ChangeType(value, propertyType); //the property is not an enum

            prop.SetValue(onObject, value); //if it's a shared property (most properties) use the new shared value being imported
        }


        /// <summary>
        /// Returns all objects of Type T which reference the supplied object <paramref name="o"/>
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public T[] GetReferencesTo<T>(IMapsDirectlyToDatabaseTable o) where T : ReferenceOtherObjectDatabaseEntity
        {
            return GetAllObjects<T>("WHERE ReferencedObjectID = " + o.ID + " AND ReferencedObjectType = '" + o.GetType().Name + "' AND ReferencedObjectRepositoryType = '" + o.Repository.GetType().Name + "'");
        }

        public IServerDefaults GetServerDefaults()
        {
            return new ServerDefaults(this);
        }

        public bool IsLookupTable(ITableInfo tableInfo)
        {
            using (var con = GetConnection())
            {
                DbCommand cmd = DatabaseCommandHelper.GetCommand(
@"if exists (select 1 from Lookup join ColumnInfo on Lookup.Description_ID = ColumnInfo.ID where TableInfo_ID = @tableInfoID)
select 1
else
select 0", con.Connection, con.Transaction);

                DatabaseCommandHelper.AddParameterWithValueToCommand("@tableInfoID", cmd, tableInfo.ID);
                return Convert.ToBoolean(cmd.ExecuteScalar());
            }
        }
    }

    public enum Tier2DatabaseType
    {
        Logging,
        DataQuality,
        QueryCaching,
        ANOStore,
        IdentifierDump
    }
}
