using System.Data.Common;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// See DatabaseEntity
    /// </summary>
    public abstract class VersionedDatabaseEntity : DatabaseEntity
    {
        #region Database Properties

        private string _softwareVersion;

        /// <summary>
        /// The version of RDMP that was running when the object was created
        /// </summary>
        [DoNotExtractProperty]
        public string SoftwareVersion
        {
            get { return _softwareVersion; }
            set { SetField(ref  _softwareVersion, value); }
        }

        #endregion
        
        protected VersionedDatabaseEntity()
        {

        }
        protected VersionedDatabaseEntity(IRepository repository, DbDataReader r):base(repository,r)
        {
            SoftwareVersion = r["SoftwareVersion"].ToString();
        }

    }
}