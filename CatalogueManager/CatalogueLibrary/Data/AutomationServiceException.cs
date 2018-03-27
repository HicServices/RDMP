using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// The automation windows service is an alternative to running DatasetLoaderUI /DataQualityEngine etc manually and handles routine automated loading of LoadMetadatas, caching of data and 
    /// running data quality engine runs.  Because the windows service does not have a visible UI it populates any errors it encounters into this table.  These errors are prominently displayed
    /// on Dashboard.exe user interface.
    /// </summary>
    public class AutomationServiceException : DatabaseEntity
    {
        #region Database Properties

        private string _machineName;
        private string _exception;
        private DateTime _eventDate;
        private string _explanation;

        public string MachineName
        {
            get { return _machineName; }
            set { SetField(ref  _machineName, value); }
        }

        public string Exception
        {
            get { return _exception; }
            set { SetField(ref  _exception, value); }
        }

        public DateTime EventDate
        {
            get { return _eventDate; }
            private set { SetField(ref  _eventDate, value); }
        }

        public string Explanation
        {
            get { return _explanation; }
            set { SetField(ref  _explanation, value); }
        }

        #endregion

        public AutomationServiceException(ICatalogueRepository repository, Exception e)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"MachineName", Environment.MachineName},
                {"Exception", ExceptionHelper.ExceptionToListOfInnerMessages(e, true)}
            });
        }

        internal AutomationServiceException(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            MachineName = r["MachineName"].ToString();
            Exception = r["Exception"].ToString();
            EventDate = Convert.ToDateTime(r["EventDate"]);
            Explanation = r["Explanation"] as string; //as string because it can be null
        }

        public override string ToString()
        {
            return Exception;
        }
    }
}