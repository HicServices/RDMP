// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using CatalogueLibrary.Data;
using FAnsi.Naming;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using Sharing.Refactoring.Exceptions;

namespace Sharing.Refactoring
{
    /// <summary>
    /// Handles making changes to SelectSQL properties that revolve around changing which underlying table/column drives the SQL.  For example when a user
    /// renames a TableInfo and wants to refactor the changes into all the ColumnInfos that underly it and all the ExtractionInformations that come from
    /// those ColumnInfos and then all the CohortIdentificationConfigurations, AggregateConfigurations etc etc.
    /// </summary>
    public class SelectSQLRefactorer
    {
        /// <summary>
        /// Replaces all references to the given table with the new table name in a columns SelectSQL.  This will also save the column.  Ensure
        /// that new tableName is in fact fully qualified e.g. '[db]..[tbl]'
        /// </summary>
        /// <param name="column"></param>
        /// <param name="tableName"></param>
        /// <param name="newFullySpecifiedTableName"></param>
        public void RefactorTableName(IColumn column,IHasFullyQualifiedNameToo tableName,string newFullySpecifiedTableName)
        {
            var ci = column.ColumnInfo;

            if(ci == null)
                throw new RefactoringException("Cannot refactor '"+column+"' because it's ColumnInfo was null");

            string fullyQualifiedName = tableName.GetFullyQualifiedName();
            
            if(!column.SelectSQL.Contains(fullyQualifiedName))
                throw new RefactoringException("IColumn '" + column + "' did not contain the fully specified table name during refactoring ('" + fullyQualifiedName +"'");

            if(!newFullySpecifiedTableName.Contains("."))
                throw new RefactoringException("Replacement table name was not fully specified, value passed was '" + newFullySpecifiedTableName +"' which did not contain any dots");

            column.SelectSQL = column.SelectSQL.Replace(fullyQualifiedName, newFullySpecifiedTableName);
            Save(column);
        }

        /// <summary>
        /// Replaces all references to the given table with the new table name in a ColumnInfo.  This will also save the column.    Ensure
        /// that new tableName is in fact fully qualified e.g. '[db]..[tbl]'
        /// </summary>
        /// <param name="column"></param>
        /// <param name="tableName"></param>
        /// <param name="newFullySpecifiedTableName"></param>
        public void RefactorTableName(ColumnInfo column, IHasFullyQualifiedNameToo tableName, string newFullySpecifiedTableName)
        {
            string fullyQualifiedName = tableName.GetFullyQualifiedName();

            if (!column.Name.StartsWith(fullyQualifiedName))
                throw new RefactoringException("ColumnInfo '" + column + "' did not start with the fully specified table name during refactoring ('" + fullyQualifiedName + "'");

            if (!newFullySpecifiedTableName.Contains("."))
                throw new RefactoringException("Replacement table name was not fully specified, value passed was '" + newFullySpecifiedTableName + "' which did not contain any dots");

            column.Name = column.Name.Replace(fullyQualifiedName, newFullySpecifiedTableName);
            column.SaveToDatabase();
        }

        protected void Save(object o)
        {
            var s = o as ISaveable;
            if (s != null)
                s.SaveToDatabase();
        }

        /// <summary>
        /// Replaces all references to the given table with the new table name in a columns SelectSQL.  This will also save the column.  Ensure
        /// that newFullySpecifiedColumnName is in fact fully qualified too e.g. [mydb]..[mytable].[mycol]
        /// </summary>
        /// <param name="column"></param>
        /// <param name="columnName"></param>
        /// <param name="newFullySpecifiedColumnName"></param>
        /// <param name="strict">Determines behaviour when column SelectSQL does not contain a reference to columnName.  True will throw a RefactoringException, false will return without making any changes</param>
        public void RefactorColumnName(IColumn column, IHasFullyQualifiedNameToo columnName, string newFullySpecifiedColumnName,bool strict = true)
        {
            string fullyQualifiedName = columnName.GetFullyQualifiedName();

            if (!column.SelectSQL.Contains(fullyQualifiedName))
                if(strict)
                    throw new RefactoringException("IColumn '" + column + "' did not contain the fully specified column name during refactoring ('" + fullyQualifiedName + "'");
                else
                    return;
                
            if (newFullySpecifiedColumnName.Count(c=>c == '.')<2)
                throw new RefactoringException("Replacement column name was not fully specified, value passed was '" + newFullySpecifiedColumnName + "' which should have had at least 2 dots");

            column.SelectSQL = column.SelectSQL.Replace(fullyQualifiedName, newFullySpecifiedColumnName);

            Save(column);
        }

        /// <summary>
        /// Determines whether the SelectSQL of the specified IColumn includes fully specified refactorable references.  If the SelectSQL is properly
        /// formed then the underlying column should appear fully specified at least once in the SelectSQL e.g. UPPER([db]..[tbl].[col])
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public bool IsRefactorable(IColumn column)
        {
            return GetReasonNotRefactorable(column) == null;
        }

        /// <summary>
        /// Determines whether the SelectSQL of the specified IColumn includes fully specified refactorable references.  Returns the reason why
        /// the IColumn is not IsRefactorable (or null if it is).
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public string GetReasonNotRefactorable(IColumn column)
        {
            var ci = column.ColumnInfo;

            if (ci == null)
                return "Cannot refactor '" + column + "' because it's ColumnInfo was null";

            if (!column.SelectSQL.Contains(ci.Name))
                return "IColumn '" + column + "' did not contain the fully specified column name of it's underlying ColumnInfo ('"+ci.Name+"') during refactoring";
            
            string fullyQualifiedName = ci.TableInfo.GetFullyQualifiedName();

            if (!column.SelectSQL.Contains(fullyQualifiedName))
                return "IColumn '" + column + "' did not contain the fully specified table name ('" + fullyQualifiedName + "') during refactoring";

            return null;
        }
    }
}
