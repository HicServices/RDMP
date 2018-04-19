using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Describes a period of time in which a given act can take place (e.g. only cache data from the MRI imaging web service during the hours of 11pm - 5am so as not to 
    /// disrupt routine hospital use).  Also serves as a Locking point for job control.  Once an IPermissionWindow is in use by a process (e.g. Caching Pipeline) then it
    /// is not available to other processes (e.g. loading or other caching pipelines that share the same IPermissionWindow).
    /// </summary>
    public class PermissionWindow : VersionedDatabaseEntity, IPermissionWindow, ILockable
    {
        #region Database Properties

        private string _name;
        private string _description;
        private bool _requiresSynchronousAccess;
        private bool _lockedBecauseRunning;
        private string _lockHeldBy;
        
        public string Name
        {
            get { return _name; }
            set { SetField(ref  _name, value); }
        }

        public string Description
        {
            get { return _description; }
            set { SetField(ref  _description, value); }
        }

        public bool RequiresSynchronousAccess
        {
            get { return _requiresSynchronousAccess; }
            set { SetField(ref  _requiresSynchronousAccess, value); }
        }

        public bool LockedBecauseRunning
        {
            get { return _lockedBecauseRunning; }
            set { SetField(ref  _lockedBecauseRunning, value); }
        }

        public string LockHeldBy
        {
            get { return _lockHeldBy; }
            set { SetField(ref  _lockHeldBy, value); }
        }

        public string PermissionPeriodConfig {
            get { return SerializePermissionWindowPeriods(); }
            set
            {
                DeserializePermissionWindowPeriods(value);
                OnPropertyChanged();
            }
        }

        #endregion

        #region Relationships

        [NoMappingToDatabase]
        public IEnumerable<CacheProgress> CacheProgresses {
            get { return Repository.GetAllObjectsWithParent<CacheProgress>(this); }
        }
        #endregion

        [NoMappingToDatabase]
        public List<PermissionWindowPeriod> PermissionWindowPeriods { get; private set; }

        private string SerializePermissionWindowPeriods()
        {
            var serializer = new XmlSerializer(typeof (List<PermissionWindowPeriod>));
            using (var output = new StringWriter())
            {
                serializer.Serialize(output, PermissionWindowPeriods);
                return output.ToString();
            }
        }

        private void DeserializePermissionWindowPeriods(string permissionPeriodConfig)
        {
            if (string.IsNullOrWhiteSpace(permissionPeriodConfig))
                PermissionWindowPeriods = new List<PermissionWindowPeriod>();
            else
            {
                var deserializer = new XmlSerializer(typeof (List<PermissionWindowPeriod>));
                PermissionWindowPeriods = deserializer.Deserialize(new StringReader(permissionPeriodConfig)) as List<PermissionWindowPeriod>;
            }
        }

        public bool CurrentlyWithinPermissionWindow()
        {
            if (!PermissionWindowPeriods.Any())
                return true;

            return TimeIsWithinPermissionWindow(DateTime.UtcNow);
        }
        
        public virtual bool TimeIsWithinPermissionWindow(DateTime dateTimeUTC)
        {
            if (!PermissionWindowPeriods.Any())
                return true;

            return PermissionWindowPeriods.Any(permissionPeriod => permissionPeriod.Contains(dateTimeUTC));
        }

        public PermissionWindow(ICatalogueRepository repository)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"PermissionPeriodConfig", DBNull.Value}
            });
        }

        internal PermissionWindow(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            Name = r["Name"].ToString();
            Description = r["Description"].ToString();
            RequiresSynchronousAccess = Convert.ToBoolean(r["RequiresSynchronousAccess"]);
            PermissionPeriodConfig = r["PermissionPeriodConfig"].ToString();

            LockedBecauseRunning = Convert.ToBoolean(r["LockedBecauseRunning"]);
            LockHeldBy = r["LockHeldBy"].ToString();
        }

        public PermissionWindow(List<PermissionWindowPeriod> permissionPeriods)
        {
            PermissionWindowPeriods = permissionPeriods;
            RequiresSynchronousAccess = true;
        }

        
        public override string ToString()
        {
            return (string.IsNullOrWhiteSpace(Name) ? "Unnamed" : Name) + "(ID = " + ID + ")";
        }
        
        public IEnumerable<ICacheProgress> GetAllCacheProgresses()
        {
            return CacheProgresses;
        }

        public void SetPermissionWindowPeriods(List<PermissionWindowPeriod> windowPeriods)
        {
            PermissionWindowPeriods = windowPeriods;
            PermissionPeriodConfig = SerializePermissionWindowPeriods();
        }

        public void Lock()
        {
            LockedBecauseRunning = true;
            LockHeldBy = Environment.UserName + " (" + Environment.MachineName + ")";
            SaveToDatabase();
        }

        public void Unlock()
        {
            LockedBecauseRunning = false;
            LockHeldBy = null;
            SaveToDatabase();
        }

        public void RefreshLockPropertiesFromDatabase()
        {
            ((CatalogueRepository)Repository).RefreshLockPropertiesFromDatabase(this);
        }
    }
}