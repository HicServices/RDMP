using System;
using System.Text.RegularExpressions;
using FAnsi;
using FAnsi.Discovery;

namespace Rdmp.Core.CommandLine.Interactive.Picking
{
    internal class PickDatabase : PickObjectBase
    {
        public const string Format = "Format \"Database:{DatabaseType}:Name:{DatabaseName}:{ConnectionString}\"";
        public const string Example = "";

        public PickDatabase() : base(null,
            new Regex("^Database:([A-Za-z]+):Name:([^:]+):(.*)$"))
        {
        }

        public override CommandLineObjectPickerArgumentValue Parse(string arg, int idx)
        {
            var m = MatchOrThrow(arg, idx);

            var dbType = (DatabaseType)Enum.Parse(typeof(DatabaseType),m.Groups[1].Value);
            var dbName = m.Groups[2].Value;
            var connectionString = m.Groups[3].Value;

            var server = new DiscoveredServer(connectionString, dbType);
            

            return new CommandLineObjectPickerArgumentValue(arg,idx,server.ExpectDatabase(dbName));
        }
    }
}