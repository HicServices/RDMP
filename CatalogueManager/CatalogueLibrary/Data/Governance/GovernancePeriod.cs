using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.Data.Governance
{
    /// <summary>
    /// A GovernancePeriod is used to track the fact that a given set of datasets requires external approval for your agency to hold.  This is not the same as releasing data
    /// to researchers or researcher approval to get specific extracts from you.  Governance Periods are concerned only with your agency and it's ability to hold datasets.  A 
    /// GovernancePeriod starts at a specific date and can optionally expire.  A GovernancePeriod relates to one or more Catalogues but Catalogues can have multiple GovernancePeriods
    /// e.g. if you require to get approval from 2 different external agencies to hold a specific dataset.  
    /// 
    /// <para>GovernancePeriods are entirely optional, you can happily get by without configuring any for any of your Catalogues.  However once you have configured a GovernancePeriod for a
    /// specific Catalogue once then it will always require governance and be reported as Governance Expired in the Dashboard once it's GovernancePeriod has expired.</para>
    /// 
    /// <para>The correct usage of GovernancePeriods is to never delete them e.g. your dataset
    /// MyDataset1 would have Governacne 2001-2002 (with attachment letters of approval) and another one for 2003-2004 and another from 2005 onwards etc.</para>
    /// </summary>
    public class GovernancePeriod : DatabaseEntity, ICheckable
    {
        #region Database Properties

        private DateTime _startDate;
        private DateTime? _endDate;
        private string _name;
        private string _description;
        private string _ticket;

        public DateTime StartDate
        {
            get { return _startDate; }
            set { SetField(ref  _startDate, value); }
        }

        public DateTime? EndDate
        {
            get { return _endDate; }
            set { SetField(ref  _endDate, value); }
        }

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

        public string Ticket
        {
            get { return _ticket; }
            set { SetField(ref  _ticket, value); }
        }

        #endregion

        #region Relationships
        [NoMappingToDatabase]
        public IEnumerable<GovernanceDocument> GovernanceDocuments {
            get { return Repository.GetAllObjectsWithParent<GovernanceDocument>(this); }
        }

        [NoMappingToDatabase]
        public IEnumerable<Catalogue> GovernedCatalogues
        { get{ return
                Repository.SelectAll<Catalogue>(
                    @"SELECT Catalogue_ID FROM GovernancePeriod_Catalogue where GovernancePeriod_ID=" + ID,
                    "Catalogue_ID");}
        }

        #endregion

        public GovernancePeriod(IRepository repository)
        {
            repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"Name", "GovernancePeriod"+Guid.NewGuid()},
                {"StartDate",DateTime.Now}
            });
        }

        internal GovernancePeriod(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            //cannot be null
            Name = r["Name"].ToString();
            StartDate = Convert.ToDateTime(r["StartDate"]);

            //can be null
            Ticket = r["Ticket"] as string;
            EndDate = ObjectToNullableDateTime(r["EndDate"]);
            Description = r["Description"] as string;
        }
        
        public override string ToString()
        {
            return Name;
        }
        
        public void Check(ICheckNotifier notifier)
        {
            if (EndDate == null)
                notifier.OnCheckPerformed(new CheckEventArgs("There is no end date for GovernancePeriod " + Name,CheckResult.Warning));
            else
                if (EndDate <= StartDate)
                    notifier.OnCheckPerformed(new CheckEventArgs("GovernancePeriod " + Name + " expires before it begins!", CheckResult.Fail));
                else
                    notifier.OnCheckPerformed(new CheckEventArgs("GovernancePeriod " + Name + " expiry date is after the start date", CheckResult.Success));
            
            foreach (var doc in GovernanceDocuments)
                doc.Check(notifier);
        }
        
        public void DeleteGovernanceRelationshipTo(Catalogue c)
        {
            var affectedRows = Repository.Delete(string.Format(@"DELETE FROM GovernancePeriod_Catalogue WHERE Catalogue_ID={0} AND GovernancePeriod_ID={1}",c.ID, ID));
                
            if(affectedRows >1)
                throw new Exception("somehow we deleted more than 1 row when trying to erase a specific governance relationship to Catalogue " + c + ", affected rows was " + affectedRows);
        }

        public void CreateGovernanceRelationshipTo(Catalogue c)
        {
            Repository.Insert(string.Format(
                @"INSERT INTO GovernancePeriod_Catalogue (Catalogue_ID,GovernancePeriod_ID) VALUES ({0},{1})",
                c.ID, ID), null);
        }

        
        public bool IsExpired()
        {
            if (EndDate == null)
                return false;

            return DateTime.Now.Date > EndDate.Value.Date;
        }
    }
}
