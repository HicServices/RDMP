using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FAnsi;
using FAnsi.Discovery;

namespace Rdmp.Core.CommandLine.Interactive.Picking
{
    /// <summary>
    /// Determines if a command line argument provided was a reference to a <see cref="DiscoveredDatabase"/>
    /// </summary>
    public class PickDatabase : PickObjectBase
    {
        public override string Format => "DatabaseType:{DatabaseType}:[Name:{DatabaseName}:]{ConnectionString}";
        public override string Help => 
@"DatabaseType (Required):
    MicrosoftSQLServer
    MySql
    Oracle

Name: (Optional if in connection string)
ConnectionString (Required)";
        public override IEnumerable<string> Examples => new[]
        {
            "DatabaseType:MicrosoftSQLServer:Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;",
             
            //see https://stackoverflow.com/questions/4950897/odp-net-integrated-security-invalid-connection-string-argument
            "DatabaseType:Oracle:Name:Bob:Data Source=MyOracleDB;User Id=/;"
            
        };

        public PickDatabase() : base(null,
            new Regex("^DatabaseType:([A-Za-z]+):(Name:[^:]+:)?(.*)$",RegexOptions.IgnoreCase)) 
        {
            
        }

        public override CommandLineObjectPickerArgumentValue Parse(string arg, int idx)
        {
            var m = MatchOrThrow(arg, idx);

            var dbType = (DatabaseType)Enum.Parse(typeof(DatabaseType),m.Groups[1].Value,true);
            var dbName = Trim("Name:",m.Groups[2].Value);
            var connectionString = m.Groups[3].Value;

            var server = new DiscoveredServer(connectionString, dbType);

            DiscoveredDatabase db;

            if (string.IsNullOrWhiteSpace(dbName))
                db = server.GetCurrentDatabase();
            else
                db = server.ExpectDatabase(dbName);

            if(db == null)
                throw new CommandLineObjectPickerParseException("Missing database name parameter, it was not in connection string or specified explicitly",idx,arg);
            
            return new CommandLineObjectPickerArgumentValue(arg,idx,db);
        }

        public override IEnumerable<string> GetAutoCompleteIfAny()
        {
            yield return "DatabaseType:";
            yield return "Oracle";
        }
    }
}