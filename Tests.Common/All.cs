// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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
