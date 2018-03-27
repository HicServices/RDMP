using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data.DataTables;
using MapsDirectlyToDatabaseTable;

namespace DataExportLibrary.Data.DataTables
{
    /// <summary>
    /// Each Project can have 0 or more DataUser entities associated with it.  Each DataUser represents a single person who will have access to the extracted data.  This record is not
    /// currently used for anything beyond writting into the ReleaseDocument (See WordDataReleaseFileGenerator) and helping track which researchers are responsible/dependent on
    /// each projects data.
    /// </summary>
    public class DataUser : VersionedDatabaseEntity, IDataUser
    {
        #region Database Properties
        private string _forename;
        private string _surname;
        private string _email;

        public string Forename
        {
            get { return _forename; }
            set { SetField(ref _forename, value); }
        }
        public string Surname
        {
            get { return _surname; }
            set { SetField(ref _surname, value); }
        }
        public string Email
        {
            get { return _email; }
            set { SetField(ref _email, value); }
        }

        #endregion
        
        public static int Forename_MaxLength = -1;
        public static int Surname_MaxLength = -1;
        public static int Email_MaxLength = -1;

        public DataUser(IDataExportRepository repository, string forename, string surname)
        {
            Repository = repository;
            Repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"Forename", forename},
                {"Surname", surname}
            });
        }

        internal DataUser(IDataExportRepository repository, DbDataReader r)
            : base(repository, r)
        {
            Forename = r["Forename"] as string;
            Surname = r["Surname"] as string;
            Email = r["Email"] as string;
        }

        public override string ToString()
        {
            return Forename + " " + Surname;
        }

        public void RegisterAsDataUserOnProject(Project project)
        {
            Repository.Insert("INSERT INTO Project_DataUser(Project_ID,DataUser_ID) VALUES (@Project_ID,@DataUser_ID);",new Dictionary<string, object>
            {
                {"Project_ID", project.ID},
                {"DataUser_ID", ID}
            });
        }

        public void UnRegisterAsDataUserOnProject(Project project)
        {
            Repository.Delete("DELETE FROM Project_DataUser WHERE Project_ID=@Project_ID AND DataUser_ID=@DataUser_ID;", 
                new Dictionary<string, object>
                {
                    {"Project_ID", project.ID},
                    {"DataUser_ID", ID}
                });
        }
    }
}
