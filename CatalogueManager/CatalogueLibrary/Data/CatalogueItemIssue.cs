// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text.RegularExpressions;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using ReusableLibraryCode;
using ReusableLibraryCode.Annotations;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// A lightweight record of a problem you have encountered with a specific column/transform in a dataset.  Must be tied to a CatalogueItem 
    /// which is the column as it is made available to researchers.  Note that depending on settings these Issues may be written out by DataExportManager
    /// into the metadata report so be careful what you write in the name/description fields - i.e. don't put in sensitive information that could upset 
    /// governancers.
    /// 
    /// <para>Note also the Ticket property, this lets you use your own agencies Ticketing system (e.g. JIRA) to track time/descriptions whilst also making the 
    /// RDMP aware of the issues.  See TicketingSystemConfiguration for how to setup a ticketing system plugin for your RDMP deployment.</para>
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
        
        /// <summary>
        /// User provided description of the issue.  This will be externally visible in extractions metadata documents
        /// </summary>
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

        /// <summary>
        /// The specific column or transform that this issue affects (CatalogueItem)
        /// </summary>
        [DoNotExtractProperty]
        public int CatalogueItem_ID
        {
            get { return _catalogueItemID; }
            set { SetField(ref  _catalogueItemID, value); }
        }

        /// <summary>
        /// A name for refering to the current issue 
        /// </summary>
        [Unique]
        [NotNull]
        public string Name
        {
            get { return _name; }
            set { SetField(ref  _name, value); }
        }

        /// <summary>
        /// The action underway to resolve the issue
        /// </summary>
        [DoNotExtractProperty]
        public string Action
        {
            get { return _action; }
            set { SetField(ref  _action, value); }
        }

        /// <summary>
        /// Guidlines for how to handle the issue e.g. which records are likely to be affected, whether they should be ignored or if there is a workaround etc
        /// </summary>
        public string NotesToResearcher
        {
            get { return _notesToResearcher; }
            set { SetField(ref  _notesToResearcher, value); }
        }


        //do not remove this attribute because SQL often contains ANOCHI or PROCHIs
        /// <summary>
        /// Sql you can run to reproduce the issue e.g. select * from mytable where LEN(chi) = 0
        /// </summary>
        [DoNotExtractProperty]
        public string SQL
        {
            get { return _sql; }
            set { SetField(ref  _sql, value); }
        }

        /// <summary>
        /// A ticketing system issue number for where this issue is documented more fully in a tracking system (if any)
        /// </summary>
        [DoNotExtractProperty]
        public string Ticket
        {
            get { return _ticket; }
            set { SetField(ref  _ticket, value); }
        }

        /// <summary>
        /// The current status of the issue (e.g. outstanding or resolved)
        /// </summary>
        [DoNotExtractProperty]
        public IssueStatus Status
        {
            get { return _status; }
            set { SetField(ref  _status, value); }
        }

        /// <summary>
        /// Automatically populated date at which the issue was created
        /// </summary>
        [DoNotExtractProperty]
        public DateTime DateCreated
        {
            get { return _dateCreated; }
            set { SetField(ref  _dateCreated, value); }
        }

        /// <summary>
        /// The date the ticket entered it's current Status
        /// </summary>
        public DateTime? DateOfLastStatusChange
        {
            get { return _dateOfLastStatusChange; }
            set { SetField(ref  _dateOfLastStatusChange, value); }
        }

        /// <summary>
        /// The windows username of the person who created the issue
        /// </summary>
        [DoNotExtractProperty]
        public string UserWhoCreated
        {
            get { return _userWhoCreated; }
            set { SetField(ref  _userWhoCreated, value); }
        }


        /// <summary>
        /// Windows username of the person who changed the issue to it's current status
        /// </summary>
        [DoNotExtractProperty]
        public string UserWhoLastChangedStatus
        {
            get { return _userWhoLastChangedStatus; }
            set { SetField(ref  _userWhoLastChangedStatus, value); }
        }


        /// <summary>
        /// How serious is the issue (red amber or green)
        /// </summary>
        public IssueSeverity Severity
        {
            get { return _severity; }
            set { SetField(ref  _severity, value); }
        }

        /// <summary>
        /// ID of the IssueSystemUser who reported the issue
        /// </summary>
        [Obsolete("This should be merged with DataUser")]
        [DoNotExtractProperty]
        public int? ReportedBy_ID
        {
            get { return _reportedByID; }
            set { SetField(ref  _reportedByID, value); }
        }


        /// <summary>
        /// Date the ReportedBy_ID reported the issue (could be different from DateCreated if the report was provided by email and there was a delay
        /// entering it into the system).
        /// </summary>
        [DoNotExtractProperty]
        public DateTime? ReportedOnDate
        {
            get { return _reportedOnDate; }
            set { SetField(ref  _reportedOnDate, value); }
        }

        /// <summary>
        /// ID of the IssueSystemUser who is responsible for resolving this issue
        /// </summary>
        [Obsolete("This should be merged with DataUser")]
        [DoNotExtractProperty]
        public int? Owner_ID
        {
            get { return _ownerID; }
            set { SetField(ref  _ownerID, value); }
        }

        /// <summary>
        /// Optional file path to an excel spreadsheet which provides more information e.g. graphs etc for the issue
        /// </summary>
        [DoNotExtractProperty]
        [AdjustableLocation]
        public string PathToExcelSheetWithAdditionalInformation
        {
            get { return _pathToExcelSheetWithAdditionalInformation; }
            set { SetField(ref  _pathToExcelSheetWithAdditionalInformation, value); }
        }

        #endregion

        #region Relationships
        /// <inheritdoc cref="CatalogueItem_ID"/>
        [NoMappingToDatabase]
        public CatalogueItem CatalogueItem
        {
            get
            {
                return Repository.GetObjectByID<CatalogueItem>(CatalogueItem_ID);
            }
        }
        /// <inheritdoc cref="ReportedBy_ID"/>
        [NoMappingToDatabase]
        public IssueSystemUser ReportedBy {
            get { return ReportedBy_ID == null? null: Repository.GetObjectByID<IssueSystemUser>((int) ReportedBy_ID); }
        }

        /// <inheritdoc cref="Owner_ID"/>
        [NoMappingToDatabase]
        public IssueSystemUser Owner {
            get { return Owner_ID == null ? null : Repository.GetObjectByID<IssueSystemUser>((int) Owner_ID); }
        }
        #endregion

        /// <summary>
        /// Defines a new problematic issue associated with the specified column/transform (CatalogueItem).
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="item"></param>
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

        internal CatalogueItemIssue(ICatalogueRepository repository, DbDataReader r)
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
        
        /// <summary>
        /// Returns the Name of the IssueSystemUser referenced by <see cref="ReportedBy_ID"/> or Unknown if there isn't one.
        /// </summary>
        /// <returns></returns>
        public string GetReportedByName()
        {
            if (this.ReportedBy_ID == null)
                return "Unknown";

            return ReportedBy.Name;
        }

        /// <summary>
        /// Returns the Name of the IssueSystemUser referenced by <see cref="Owner_ID"/> or Unknown if there isn't one.
        /// </summary>
        /// <returns></returns>
        public string GetOwnerByName()
        {
            if (this.Owner_ID == null)
                return "Unknown";

            return Owner.Name;
        }

        /// <inheritdoc cref="ToString"/>
        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// The severity of a CatalogueItemIssue
    /// </summary>
    public enum IssueSeverity
    {
        /// <summary>
        /// Very serious
        /// </summary>
        Red,

        /// <summary>
        /// Not so serious
        /// </summary>
        Amber,

        /// <summary>
        /// Not serious at all
        /// </summary>
        Green
    }

    /// <summary>
    /// The current development stage a CatalogueItemIssue is at
    /// </summary>
    public enum IssueStatus
    {
        /// <summary>
        /// Issue has not been evaluated/no fix is being prepared
        /// </summary>
        Outstanding,

        /// <summary>
        /// A fix is in the works
        /// </summary>
        InDevelopment,

        /// <summary>
        /// A fix/evaluation cannot be applied due to external factors
        /// </summary>
        Blocked,

        /// <summary>
        /// The issue has been fully resolved and exists only for recording purposes
        /// </summary>
        Resolved
    }
}
