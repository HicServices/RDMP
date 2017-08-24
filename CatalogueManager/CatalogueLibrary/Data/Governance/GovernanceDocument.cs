using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;
using Microsoft.SqlServer.Management.Smo;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.Data.Governance
{

    /// <summary>
    /// Contains the path to a useful file which reflects either a request or a granting of governance e.g. a letter from your local healthboard authorising you to host/use 1 or more
    /// datasets for a given period of time.  Also includes a name (which should really match the file name) and a description which should be a plain summary of what is in the document
    /// such that lay users can appreciate what the document contains/means for the system.
    /// </summary>
    public class GovernanceDocument : DatabaseEntity
    {
        #region Database Properties

        private int _governancePeriodID;
        private string _name;
        private string _description;
        private string _url;

        public int GovernancePeriod_ID
        {
            get { return _governancePeriodID; }
            private set { SetField(ref  _governancePeriodID, value); }
        } //every document belongs to only one period of governance knoweldge (via fk relationship)

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

        [AdjustableLocation]
        public string URL
        {
            get { return _url; }
            set { SetField(ref  _url, value); }
        }

        #endregion

        public GovernanceDocument(ICatalogueRepository repository, GovernancePeriod parent, FileInfo file)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"GovernancePeriod_ID", parent.ID},
                {"URL", file.FullName},
                {"Name", file.Name}
            });
        }

        public GovernanceDocument(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            //cannot be null
            Name = r["Name"].ToString();
            URL = r["URL"].ToString();

            GovernancePeriod_ID = Convert.ToInt32(r["GovernancePeriod_ID"]);

            //can be null
            Description = r["Description"] as string;
        }


        public override string ToString()
        {
            return Name;
        }

        public void Check(ICheckNotifier notifier)
        {

            try
            {
                FileInfo fileInfo = new FileInfo(URL);

                if (fileInfo.Exists)
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "Found intact attachment file " + fileInfo + " with length " +
                            UsefulStuff.GetHumanReadableByteSize(fileInfo.Length), CheckResult.Success));
                else
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "File " + fileInfo.FullName + " does not exist (for GovernanceDocument '" + this + "' (ID=" +
                            ID + ")", CheckResult.Fail));
            }
            catch (Exception ex)
            {
                notifier.OnCheckPerformed(
                  new CheckEventArgs(
                      "Failed to check for existance of the file described by GovernanceDocument '" + this + "' (ID=" +
                      ID + ")", CheckResult.Fail,ex));
            }
        }
        public string GetFilenameOnly()
        {
            return Path.GetFileName(URL);
        }
    }
}
