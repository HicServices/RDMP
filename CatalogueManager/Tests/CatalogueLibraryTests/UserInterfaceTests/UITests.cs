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
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Repositories;
using FAnsi.Implementation;
using LoadModules.Generic.Mutilators.Dilution.Operations;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using ReusableLibraryCode.CommandExecution.AtomicCommands;

namespace CatalogueLibraryTests.UserInterfaceTests
{
    public class UITests
    {
        protected MemoryDataExportRepository Repository = new MemoryDataExportRepository();
        protected TestActivateItems ItemActivator;

        protected MEF MEF;

        /// <summary>
        /// Call if your test needs to access classes via MEF.  Loads all dlls in the test directory.
        /// </summary>
        protected void SetupMEF()
        {
            MEF = new MEF();
            MEF.Setup(new SafeDirectoryCatalog(TestContext.CurrentContext.TestDirectory));
        }
        
        protected T WhenIHaveA<T>() where T:DatabaseEntity
        {
            T toReturn = null;

            if (typeof (T) == typeof (Catalogue))
                return (T)(object) Save(new Catalogue(Repository, "Mycata"));

            if (typeof(T) == typeof(CatalogueItem))
            {
                var cata = new Catalogue(Repository, "Mycata");
                return (T)(object)Save((T)(object)new CatalogueItem(Repository, cata, "MyCataItem"));
            }

            if (typeof (T) == typeof (TableInfo))
            {
                var table = new TableInfo(Repository, "My_Table");
                return  (T)(object)Save(table);
            }

            if (typeof (T) == typeof (ColumnInfo))
            {
                var ti = WhenIHaveA<TableInfo>();
                var col = new ColumnInfo(Repository,"My_Col","varchar(10)",ti);
                return (T)(object)Save(col);
            }

            if (typeof (T) == typeof (AggregateConfiguration))
            {
                ExtractionInformation dateEi;
                ExtractionInformation otherEi;
                return (T)(object)WhenIHaveA<AggregateConfiguration>(out dateEi, out otherEi);
            }

            if (typeof (T) == typeof (ANOTable))
            {
                ExternalDatabaseServer server;
                return (T)(object)WhenIHaveA<ANOTable>(out server);
            }

            throw new NotSupportedException();

        }

        protected AggregateConfiguration WhenIHaveA<T>(out ExtractionInformation dateEi, out ExtractionInformation otherEi) where T : AggregateConfiguration
        {
            var ti = WhenIHaveA<TableInfo>();
            var dateCol = new ColumnInfo(Repository, "MyDateCol", "datetime2", ti);
            var otherCol = new ColumnInfo(Repository, "MyOtherCol", "varchar(10)", ti);

            var cata = WhenIHaveA<Catalogue>();
            var dateCi = new CatalogueItem(Repository, cata, dateCol.Name);
            dateEi = new ExtractionInformation(Repository, dateCi, dateCol, dateCol.Name);
            var otherCi = new CatalogueItem(Repository, cata, otherCol.Name);
            otherEi = new ExtractionInformation(Repository, otherCi, otherCol, otherCol.Name);
            return Save(new AggregateConfiguration(Repository, cata, "My graph"));
        }

        protected DatabaseEntity WhenIHaveA<T>(out ExternalDatabaseServer server) where T:ANOTable
        {
            server = new ExternalDatabaseServer(Repository, "ANO Server", typeof(ANOStore.Database.Class1).Assembly);
            var anoTable = new ANOTable(Repository, server, "ANOFish", "F");
            return anoTable;
        }

        private T Save<T>(T s) where T:ISaveable
        {
            s.SaveToDatabase();
            return s;
        }

        protected T AndLaunch<T>(DatabaseEntity o) where T : Control, IRDMPSingleDatabaseObjectControl, new()
        {
            if (ItemActivator == null)
            {
                ItemActivator = new TestActivateItems(Repository);
                ItemActivator.RepositoryLocator.CatalogueRepository.MEF = MEF;
            }

            Form f = new Form();
            T ui = new T();
            f.Controls.Add(ui);
            ui.SetDatabaseObject(ItemActivator, o);
            return ui;
        }

        
        /// <summary>
        /// Loads FAnsi implementations for all supported DBMS platforms into memory
        /// </summary>
        [SetUp]
        protected void LoadDatabaseImplementations()
        {
            ImplementationManager.Load(
                typeof(FAnsi.Implementations.MicrosoftSQL.MicrosoftSQLImplementation).Assembly,
                typeof(FAnsi.Implementations.MySql.MySqlImplementation).Assembly,
                typeof(FAnsi.Implementations.Oracle.OracleImplementation).Assembly);
        }

        /// <summary>
        /// Asserts that the given command is impossible for the <paramref name="expectedReason"/>
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="expectedReason">The reason it should be impossible - uses StringAssert.Contains</param>
        protected void AssertImpossibleBecause(IAtomicCommand cmd, string expectedReason)
        {
            Assert.IsTrue(cmd.IsImpossible);
            StringAssert.Contains(expectedReason, cmd.ReasonCommandImpossible);
        }

        /// <summary>
        /// Asserts that no calls have been made to KillForm (the last resort termination of a UI).
        /// </summary>
        protected void AssertNoCrash()
        {
            Assert.AreEqual(0,ItemActivator.Results.KilledForms.Count);
        }
    }
}