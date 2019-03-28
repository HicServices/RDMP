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