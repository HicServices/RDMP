using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text.RegularExpressions;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// A lightweight record of a problem you have encountered with a specific column/transform in a dataset.  Must be tied to a CatalogueItem 
    /// which is the column as it is made available to researchers.  Note that depending on settings these Issues may be written out by DataExportManager
    /// into the metadata report so be careful what you write in the name/description fields - i.e. don't put in sensitive information that could upset 
    /// governancers.
    /// 
    /// Note also the Ticket property, this lets you use your own agencies Ticketing system (e.g. JIRA) to track time/descriptions whilst also making the 
    /// RDMP aware of the issues.  See TicketingSystemConfiguration for how to setup a ticketing system plugin for your RDMP deployment.
    /// </summary>
    public class CatalogueItemIssue : VersionedDatabaseEntity,INamed
    {
        #region Database Properties
        private string _description;
        private int _catalogueItemID;
        private string _name;
        private string _action;
        private string _notesToResearcher;
        private string _sql;
        private string _ticket;
        private IssueStatus _status;
        private DateTime _dateCreated;
        private DateTime? _dateOfLastStatusChange;
        private string _userWhoCreated;
        private string _userWhoLastChangedStatus;
        private IssueSeverity _severity;
        private int? _reportedByID;
        private DateTime? _reportedOnDate;
        private int? _ownerID;
        private string _pathToExcelSheetWithAdditionalInformation;

        public string Description
        {
            get { return _description; }
            set
            {

                if(value != null)
                {
                    Regex regCHI = new Regex(@"\b[0-9]{10}\b");
                    Regex regPROCHI = new Regex(@"\b[a-zA-Z]{3}[0-9]{7}\b");

                    if(regCHI.IsMatch(value))
                        throw new ArgumentException("Identifiers should not appear in issue descriptions (Issue ID=" + ID + ")" );

                    if (regPROCHI.IsMatch(value))
                        throw new ArgumentException("Identifiers should not appear in issue descriptions (Issue ID=" + ID + ")");
                        
                }

                SetField(ref _description ,value); 
                
            }
        }

        [DoNotExtractProperty]
        public int CatalogueItem_ID
        {
            get { return _catalogueItemID; }
            set { SetField(ref  _catalogueItemID, value); }
        }

        public string Name
        {
            get { return _name; }
            set { SetField(ref  _name, value); }
        }

        [DoNotExtractProperty]
        public string Action
        {
            get { return _action; }
            set { SetField(ref  _action, value); }
        }

        public string NotesToResearcher
        {
            get { return _notesToResearcher; }
            set { SetField(ref  _notesToResearcher, value); }
        }


        //do not remove this attribute because SQL often contains ANOCHI or PROCHIs
        [DoNotExtractProperty]
        public string SQL
        {
            get { return _sql; }
            set { SetField(ref  _sql, value); }
        }

        [DoNotExtractProperty]
        public string Ticket
        {
            get { return _ticket; }
            set { SetField(ref  _ticket, value); }
        }


        [DoNotExtractProperty]
        public IssueStatus Status
        {
            get { return _status; }
            set { SetField(ref  _status, value); }
        }


        [DoNotExtractProperty]
        public DateTime DateCreated
        {
            get { return _dateCreated; }
            set { SetField(ref  _dateCreated, value); }
        }

        public DateTime? DateOfLastStatusChange
        {
            get { return _dateOfLastStatusChange; }
            set { SetField(ref  _dateOfLastStatusChange, value); }
        }

        [DoNotExtractProperty]
        public string UserWhoCreated
        {
            get { return _userWhoCreated; }
            set { SetField(ref  _userWhoCreated, value); }
        }

        [DoNotExtractProperty]
        public string UserWhoLastChangedStatus
        {
            get { return _userWhoLastChangedStatus; }
            set { SetField(ref  _userWhoLastChangedStatus, value); }
        }

        public IssueSeverity Severity
        {
            get { return _severity; }
            set { SetField(ref  _severity, value); }
        }

        [DoNotExtractProperty]
        public int? ReportedBy_ID
        {
            get { return _reportedByID; }
            set { SetField(ref  _reportedByID, value); }
        }

        [DoNotExtractProperty]
        public DateTime? ReportedOnDate
        {
            get { return _reportedOnDate; }
            set { SetField(ref  _reportedOnDate, value); }
        }

        [DoNotExtractProperty]
        public int? Owner_ID
        {
            get { return _ownerID; }
            set { SetField(ref  _ownerID, value); }
        }

        [DoNotExtractProperty]
        [AdjustableLocation]
        public string PathToExcelSheetWithAdditionalInformation
        {
            get { return _pathToExcelSheetWithAdditionalInformation; }
            set { SetField(ref  _pathToExcelSheetWithAdditionalInformation, value); }
        }

        #endregion

        #region Relationships
        [NoMappingToDatabase]
        public CatalogueItem CatalogueItem
        {
            get
            {
                return Repository.GetObjectByID<CatalogueItem>(CatalogueItem_ID);
            }
        }
        [NoMappingToDatabase]
        public IssueSystemUser ReportedBy {
            get { return ReportedBy_ID == null? null: Repository.GetObjectByID<IssueSystemUser>((int) ReportedBy_ID); }
        }

        [NoMappingToDatabase]
        public IssueSystemUser Owner {
            get { return Owner_ID == null ? null : Repository.GetObjectByID<IssueSystemUser>((int) Owner_ID); }
        }
        #endregion

        public CatalogueItemIssue(ICatalogueRepository repository, CatalogueItem item)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"CatalogueItem_ID", item.ID},
                {"Status", IssueStatus.Outstanding.ToString()},
                {"Name", "NewIssue" + Guid.NewGuid()},
                {"UserWhoCreated", Environment.UserName}
            });
        }

        public CatalogueItemIssue(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            CatalogueItem_ID = int.Parse(r["CatalogueItem_ID"].ToString());
            Name = r["Name"] as string;
            Description = r["Description"] as string;
            Action = r["Action"] as string;
            NotesToResearcher = r["NotesToResearcher"] as string;

            SQL = r["SQL"] as string;
            Ticket = r["Ticket"] as string;

            DateCreated = DateTime.Parse(r["DateCreated"].ToString());

            if(r["DateOfLastStatusChange"] != DBNull.Value)
                DateOfLastStatusChange = DateTime.Parse(r["DateOfLastStatusChange"].ToString());

            if (r["ReportedBy_ID"] != DBNull.Value)
                ReportedBy_ID = int.Parse(r["ReportedBy_ID"].ToString());

            if (r["Owner_ID"] != DBNull.Value)
                Owner_ID = int.Parse(r["Owner_ID"].ToString());

            if (r["ReportedOnDate"] != DBNull.Value)
                ReportedOnDate = DateTime.Parse(r["ReportedOnDate"].ToString());

            UserWhoCreated = r["UserWhoCreated"] as string;
            UserWhoLastChangedStatus = r["UserWhoLastChangedStatus"] as string;

            IssueStatus status;

            if(!Enum.TryParse(r["Status"].ToString(), out status))
                throw new Exception("Did not recognise status \""+r["Status"]+"\"");

            Status = status;
            IssueSeverity severity;

            if (!Enum.TryParse(r["Severity"].ToString(), out severity))
                throw new Exception("Did not recognise IssueSeverity \"" + r["Severity"] + "\"");
            else
                Severity = severity;

            PathToExcelSheetWithAdditionalInformation = r["PathToExcelSheetWithAdditionalInformation"] as string;
        }
        
        
        public string GetNameIncludingTicketIfExists()
        {

            string issueBit =
                "(" +
                (string.IsNullOrWhiteSpace(Ticket) ? "No JIRA Ticket" : Ticket)
                +
                " ID=" + ID
                 + ")";
           return Name + issueBit;
        }

        public string GetReportedByName()
        {
            if (this.ReportedBy_ID == null)
                return "Unknown";

            return ReportedBy.Name;
        }

        public string GetOwnerByName()
        {
            if (this.Owner_ID == null)
                return "Unknown";

            return Owner.Name;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public enum IssueSeverity
    {
        Red,
        Amber,
        Green
    }

    public enum IssueStatus
    {
        Outstanding,
        InDevelopment,
        Blocked,
        Resolved
    }
}
