using System.Collections.Generic;
using System.Data;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.TableCreation
{
    public class CreateTableArgs
    {
        /// <summary>
        /// The destination database in which to create the table
        /// </summary>
        public DiscoveredDatabase Database { get; set; }

        /// <summary>
        /// Name you want the table to have once created
        /// </summary>
        public string TableName { get; private set; }

        /// <summary>
        /// Schema of the <see cref="Database"/> to create the table in
        /// </summary>
        public string Schema { get; private set; }

        /// <summary>
        /// Optional - Columns are normally created based on supplied DataTable data rows.  If this is set then the Type specified here will
        /// be used instead.
        /// </summary>
        public DatabaseColumnRequest[] ExplicitColumnDefinitions { get; set; }

        /// <summary>
        /// Set this to make last minute changes to column datatypes before table creation
        /// </summary>
        public IDatabaseColumnRequestAdjuster Adjuster { get; set; }

        /// <summary>
        /// Link between columns that you want to create in your table <see cref="DatabaseColumnRequest"/> and existing columns (<see cref="DiscoveredColumn"/>) that
        /// should be paired with a foreign key constraint
        /// </summary>
        public Dictionary<DatabaseColumnRequest, DiscoveredColumn> ForeignKeyPairs { get; private set; }

        /// <summary>
        /// When creating a foreign key constraint (See <see cref="ForeignKeyPairs"/>) determines whether ON DELETE CASCADE should be set.
        /// </summary>
        public bool CascadeDelete { get; private set; }

        /// <summary>
        /// The data to use to determine table schema and load into the newly created table (unless <see cref="CreateEmpty"/> is set).
        /// </summary>
        public DataTable DataTable { get; private set; }

        /// <summary>
        /// When creating the table, do not upload any rows supplied in <see cref="DataTable"/>
        /// </summary>
        public bool CreateEmpty { get; private set; }

        /// <summary>
        /// True if the table has been created
        /// </summary>
        public bool TableCreated { get; private set; }

        /// <summary>
        /// Populated after the table has been created (See <see cref="TableCreated"/>), list of the <see cref="DataTypeComputer"/> used to create the columns in the table.
        /// <para>This will be null if no <see cref="DataTable"/> was provided when creating the table</para>
        /// </summary>
        public Dictionary<string, DataTypeComputer> ColumnCreationLogic { get; private set; }

        /// <summary>
        /// Create a table with the given name.  Set your columns in <see cref="ExplicitColumnDefinitions"/>
        /// </summary>
        public CreateTableArgs(DiscoveredDatabase database, string tableName, string schema)
        {
            Database = database;
            TableName = tableName;
            Schema = schema;
        }
        
        /// <summary>
        /// Create a table with the given name.  Set your columns in <see cref="ExplicitColumnDefinitions"/>
        /// </summary>
        public CreateTableArgs(DiscoveredDatabase database,string tableName,string schema,Dictionary<DatabaseColumnRequest, DiscoveredColumn> foreignKeyPairs,bool cascadeDelete)
            :this(database,tableName,schema)
        {
            ForeignKeyPairs = foreignKeyPairs;
            CascadeDelete = cascadeDelete;
        }

        /// <summary>
        /// Create a table with the given name based on the columns and data in the provided <paramref name="dataTable"/>.  If you want to override the 
        /// data type of a given column set <see cref="ExplicitColumnDefinitions"/>
        /// </summary>
        public CreateTableArgs(DiscoveredDatabase database, string tableName, string schema,DataTable dataTable, bool createEmpty)
            :this(database,tableName,schema)
        {
            DataTable = dataTable;
            CreateEmpty = createEmpty;
        }
        
        /// <summary>
        /// Create a table with the given name based on the columns and data in the provided <paramref name="dataTable"/>.  If you want to override the 
        /// data type of a given column set <see cref="ExplicitColumnDefinitions"/>
        /// </summary>
        public CreateTableArgs(DiscoveredDatabase database, string tableName, string schema,DataTable dataTable, bool createEmpty, Dictionary<DatabaseColumnRequest, DiscoveredColumn> foreignKeyPairs, bool cascadeDelete)
            : this(database, tableName, schema,dataTable,createEmpty)
        {
            ForeignKeyPairs = foreignKeyPairs;
            CascadeDelete = cascadeDelete;
        }

        /// <summary>
        /// Declare that the table has been created and the provided <paramref name="columnsCreated"/> were used to determine the column schema
        /// </summary>
        /// <param name="columnsCreated"></param>
        public void OnTableCreated(Dictionary<string, DataTypeComputer> columnsCreated)
        {
            ColumnCreationLogic = columnsCreated;
            TableCreated = true;
        }
    }
}
