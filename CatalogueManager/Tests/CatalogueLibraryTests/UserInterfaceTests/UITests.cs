using System;
using System.Reflection;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Repositories;

namespace CatalogueLibraryTests.UserInterfaceTests
{
    public class UITests
    {
        protected MemoryDataExportRepository Repository = new MemoryDataExportRepository();
        protected TestActivateItems ItemActivator;
        
        protected T GetPrivateField<T>(Control c, string field)
        {
            return (T)c.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(c);
        }
        
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

            if (typeof (T) == typeof (AggregateConfiguration))
            {
                var cata = new Catalogue(Repository, "My Cata");
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
    }
}