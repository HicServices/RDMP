using FAnsi;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Icons.IconProvision;
using System.Drawing;
using System.IO;
using System.Text;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    /// <summary>
    /// Script multiple tables optionally porting schema to a new server/DBMS type
    /// </summary>
    public class ExecuteCommandScriptTables : BasicCommandExecution
    {
        private readonly TableInfo[] _tableInfos;
        private readonly DatabaseType? _dbType;
        private readonly string _dbName;
        private readonly FileInfo _outFile;

        /// <summary>
        /// Scripts multiple tables schemas and optionally ports datatypes / constraints etc to alternate DBMS
        /// </summary>
        public ExecuteCommandScriptTables(IBasicActivateItems activator,
            [DemandsInitialization("Tables to script")]
            TableInfo[] tableInfos,
            [DemandsInitialization("Optional alternate DBMS Type to port the schema to")]
            DatabaseType? dbType,
            [DemandsInitialization("Optional alternate database name to use in the script generated.")]
            string dbName,
            [DemandsInitialization("Optional file to write the resulting script out to.  Pass Null to output to console.")]
            FileInfo outFile) : base(activator)
        {
            _tableInfos = tableInfos;
            this._dbType = dbType;
            this._dbName = dbName;
            this._outFile = outFile;
        }

        public override string GetCommandHelp()
        {
            return "Scripts multiple tables structure to Clipboard (without dependencies)";
        }

        public override void Execute()
        {
            var sbScript = new StringBuilder();

            foreach (var tableInfo in _tableInfos)
            {
                var tbl = tableInfo.Discover(DataAccessContext.InternalDataProcessing);

                var hypotheticalServer = new DiscoveredServer("localhost", _dbName ?? "None", _dbType ?? tableInfo.DatabaseType, null, null);
                var hypotheticalTable = hypotheticalServer.ExpectDatabase(_dbName ?? tbl.Database.GetRuntimeName()).ExpectTable(tbl.GetRuntimeName());

                var result = tbl.ScriptTableCreation(false, false, false, hypotheticalTable);
                sbScript.AppendLine(result);
                sbScript.AppendLine();
            }

            if(_outFile != null)
            {
                File.WriteAllText(_outFile.FullName, sbScript.ToString());
            }    
            else
            {
                Show($"Script for {_tableInfos.Length} tables", sbScript.ToString());
            }
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.SQL);
        }
    }
}
