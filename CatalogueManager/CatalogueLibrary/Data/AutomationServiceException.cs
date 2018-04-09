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

        /// <summary>
        /// The computer name on which the automation service was running on when it crashed
        /// </summary>
        public string MachineName
        {
            get { return _machineName; }
            set { SetField(ref  _machineName, value); }
        }

        /// <summary>
        /// The Exception that triggered the automation service to crash
        /// </summary>
        public string Exception
        {
            get { return _exception; }
            set { SetField(ref  _exception, value); }
        }

        /// <summary>
        /// The time that the automation service to crashed
        /// </summary>
        public DateTime EventDate
        {
            get { return _eventDate; }
            private set { SetField(ref  _eventDate, value); }
        }

        /// <summary>
        /// The user provided description of how the event was handled once the Exception's consequences (if any) have been resolved.  This
        /// property is used to indicate whether an Exception is considered Outstanding (unobservered) or Handled (at which point it exists only
        /// for Archival/Audit purposes.
        /// </summary>
        public string Explanation
        {
            get { return _explanation; }
            set { SetField(ref  _explanation, value); }
        }

        #endregion

        /// <summary>
        /// Audits the fact that an instance of the RDMPAutomationService (or sub-part of it) has crashed.  This will create a persistent database record
        /// which must be resolved (See <see cref="Explanation"/>).
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="e"></param>
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

        /// <inheritdoc/>
        public override string ToString()
        {
            return Exception;
        }
    }
}