using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.Dashboarding
{
    /// <summary>
    /// Records position and Type of an IDashboardableControl on a DashboardLayout.  The lifecycle goes:
    /// 1. Control instance created (must have a blank constructor)
    /// 2. ConstructEmptyCollection called on instance of control
    /// 3. Step2 collection Hydrated witht he PersistenceString (which can be null/empty)
    /// 4. Step2 collection given the Objects referenced by ObjectsUsed
    /// 5. Control instance given the Hydrated collection with SetCollection method
    /// </summary>
    public class DashboardControl:DatabaseEntity
    {
        #region Database Properties
        private int _dashboardLayout_ID;
        private int _x;
        private int _y;
        private int _width;
        private int _height;
        private string _controlType;
        private string _persistenceString;

        public int DashboardLayout_ID
        {
            get { return _dashboardLayout_ID; }
            set { SetField(ref _dashboardLayout_ID, value); }
        }
        public int X
        {
            get { return _x; }
            set { SetField(ref _x, value); }
        }
        public int Y
        {
            get { return _y; }
            set { SetField(ref _y, value); }
        }
        public int Width
        {
            get { return _width; }
            set { SetField(ref _width, value); }
        }
        public int Height
        {
            get { return _height; }
            set { SetField(ref _height, value); }
        }
        public string ControlType
        {
            get { return _controlType; }
            set { SetField(ref _controlType, value); }
        }
        public string PersistenceString
        {
            get { return _persistenceString; }
            set { SetField(ref _persistenceString, value); }
        }

        #endregion

        #region Relationships
        [NoMappingToDatabase]
        public DashboardObjectUse[] ObjectsUsed{ get { return Repository.GetAllObjectsWithParent<DashboardObjectUse>(this); }}

        [NoMappingToDatabase]
        public DashboardLayout ParentLayout { get { return Repository.GetObjectByID<DashboardLayout>(DashboardLayout_ID); } }
        #endregion

        public DashboardControl(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            DashboardLayout_ID = Convert.ToInt32(r["DashboardLayout_ID"]);
            X = Convert.ToInt32(r["X"]);
            Y = Convert.ToInt32(r["Y"]);
            Height = Convert.ToInt32(r["Height"]);
            Width = Convert.ToInt32(r["Width"]);

            ControlType = r["ControlType"].ToString();//cannot be null
            PersistenceString = r["PersistenceString"] as string;//can be null
        }

        public DashboardControl(ICatalogueRepository repository, DashboardLayout parent, Type controlType, int x, int y, int w, int h, string persistenceString)
        {
            Repository = repository;

            Repository.InsertAndHydrate(this,new Dictionary<string, object>()
            {
                {"DashboardLayout_ID",parent.ID},
                {"X",x},
                {"Y",y},
                {"Width",w},
                {"Height",h},
                {"ControlType",controlType.Name},
                {"PersistenceString",persistenceString}
            });   
        }
        public override string ToString()
        {
            return ControlType + "( " + ID + " )";
        }

        public void SaveCollectionState(IPersistableObjectCollection collection)
        {
            //save ourselves
            PersistenceString = collection.SaveExtraText();
            SaveToDatabase();

            //save our objects
            foreach (var o in ObjectsUsed)
                o.DeleteInDatabase();

            foreach (IMapsDirectlyToDatabaseTable objectToSave in collection.DatabaseObjects)
                new DashboardObjectUse((ICatalogueRepository) Repository, this, objectToSave);
            
            
        }
    }
}
