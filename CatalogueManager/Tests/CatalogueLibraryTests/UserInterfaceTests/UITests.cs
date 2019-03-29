// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Reflection;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Repositories;
using FAnsi.Implementation;

namespace CatalogueLibraryTests.UserInterfaceTests
{
    public class UITests
    {
        protected MemoryDataExportRepository Repository = new MemoryDataExportRepository();
        protected TestActivateItems ItemActivator;
        
        protected T WhenIHaveA<T>() where T:DatabaseEntity
        {
            T toReturn = null;

            if (typeof (T) == typeof (Catalogue))
                toReturn = (T)(object) new Catalogue(Repository, "Mycata");

            if (typeof(T) == typeof(CatalogueItem))
            {
                var cata = new Catalogue(Repository, "Mycata");
                toReturn = (T)(object)new CatalogueItem(Repository, cata, "MyCataItem");
            }

            if (typeof (T) == typeof (TableInfo))
            {
                var table = new TableInfo(Repository, "My_Table");
                toReturn = (T)(object)table;
            }

            if (typeof (T) == typeof (ColumnInfo))
            {
                var ti = WhenIHaveA<TableInfo>();
                var col = new ColumnInfo(Repository,"My_Col","varchar(10)",ti);
                toReturn = (T) (object) col;
            }

            if (typeof (T) == typeof (AggregateConfiguration))
            {
                var ti = WhenIHaveA<TableInfo>();
                var dateCol = new ColumnInfo(Repository, "MyDateCol", "datetime2", ti);
                var otherCol = new ColumnInfo(Repository, "MyOtherCol", "varchar(10)", ti);

                var cata = WhenIHaveA<Catalogue>();
                var dateCi = new CatalogueItem(Repository, cata, dateCol.Name);
                var dateEi = new ExtractionInformation(Repository, dateCi, dateCol, dateCol.Name);
                var otherCi = new CatalogueItem(Repository, cata, otherCol.Name);
                var otherEi = new ExtractionInformation(Repository, otherCi, otherCol, otherCol.Name);
                
                toReturn = (T)(object)new AggregateConfiguration(Repository, cata, "My graph");
            }

            if (toReturn == null)
                throw new NotSupportedException();

            toReturn.SaveToDatabase();
            return toReturn;
            //create catalogue
        }


        protected T AndLaunch<T>(DatabaseEntity o) where T : Control, IRDMPSingleDatabaseObjectControl, new()
        {
            if (ItemActivator == null)
                ItemActivator = new TestActivateItems(Repository);

            Form f = new Form();
            T ui = new T();
            f.Controls.Add(ui);
            ui.SetDatabaseObject(ItemActivator, o);
            return ui;
        }

        /// <summary>
        /// Loads FAnsi implementations for all supported DBMS platforms into memory
        /// </summary>
        protected void LoadDatabaseImplementations()
        {
            ImplementationManager.Load(
                typeof(FAnsi.Implementations.MicrosoftSQL.MicrosoftSQLImplementation).Assembly,
                typeof(FAnsi.Implementations.MySql.MySqlImplementation).Assembly,
                typeof(FAnsi.Implementations.Oracle.OracleImplementation).Assembly);
        }
    }
}