using System;
using System.Collections.Generic;
using System.Text;
using FAnsi;
using NUnit.Framework;

namespace Tests.Common
{
    public class All
    {
        /// <summary>
        /// <see cref="TestCaseSourceAttribute"/> for tests that should run on all DBMS
        /// </summary>
        public static DatabaseType[] DatabaseTypes = new[]
        {
            DatabaseType.MicrosoftSQLServer,
            DatabaseType.MySql,
            DatabaseType.Oracle,
            DatabaseType.PostgreSql
        };

        /// <summary>
        /// <see cref="TestCaseSourceAttribute"/> for tests that should run on all DBMS
        /// with both permutations of true/false.  Matches exhaustively method signature (DatabaseType,bool)
        /// </summary>
        public static object[] DatabaseTypesWithBoolFlags = new[]
        {
            new object[] {DatabaseType.MicrosoftSQLServer,true},
            new object[] {DatabaseType.MySql,true},
            new object[] {DatabaseType.Oracle,true},
            new object[] {DatabaseType.PostgreSql,true},
            new object[] {DatabaseType.MicrosoftSQLServer,false},
            new object[] {DatabaseType.MySql,false},
            new object[] {DatabaseType.Oracle,false},
            new object[] {DatabaseType.PostgreSql,false}
        };
    }
}
