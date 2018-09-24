using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;

namespace CatalogueLibrary.Data
{

    /// <summary>
    /// Describes a document (e.g. PDF / Excel file etc) which is useful for understanding a given dataset (Catalogue).  This can be marked as Extractable in which case 
    /// every time the dataset is extracted the file will also be bundled along with it (so that researchers can also benefit from the file).
    /// 
    /// <para>You can also mark SupportingDocuments as Global in which case they will be provided (if Extractable) to researchers regardless of which datasets they have selected
    /// e.g. a PDF on data governance or a copy of an empty 'data use contract document'</para>
    /// 
    /// <para>Finally you can tie in the Ticketing system so that you can audit time spent curating the document etc.</para>
    /// </summary>
    public class SupportingDocument : VersionedDatabaseEntity,INamed
    {
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Name_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int Description_MaxLength = -1;
        ///<inheritdoc cref="IRepository.FigureOutMaxLengths"/>
        public static int URL_MaxLength = -1;

        #region Database Properties
        private string _name;
        private Uri _uRL;
        private string _description;
        private bool _extractable;
        private string _ticket;
        private bool _isGlobal;
        private int _catalogueID;

        /// <inheritdoc/>
        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value); }
        }

        /// <summary>
        /// Path to the document on disk
        /// </summary>
        [AdjustableLocation]
        public Uri URL
        {
            get { return _uRL; }
            set { SetField(ref _uRL, value); }
        }

        /// <summary>
        /// Human readable description of what the document contains and why it is in the system
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { SetField(ref _description, value); }
        }

        /// <summary>
        /// If true then the file will be copied to the output directory of project extractions that include the <see cref="Catalogue_ID"/>.
        /// </summary>
        public bool Extractable
        {
            get { return _extractable; }
            set { SetField(ref _extractable, value); }
        }

        /// <summary>
        /// <see cref="CatalogueLibrary.Ticketing.ITicketingSystem"/> identifier of a ticket for logging time curating / updating etc the document
        /// </summary>
        public string Ticket
        {
            get { return _ticket; }
            set { SetField(ref _ticket, value); }
        }

        /// <summary>
        /// If <see cref="Extractable"/>  and <see cref="IsGlobal"/> then the document will be copied to the ouptut directory of all project extractions
        /// regardless of whether or not the <see cref="Catalogue_ID"/> dataset is included in the extraction
        /// </summary>
        public bool IsGlobal
        {
            get { return _isGlobal; }
            set { SetField(ref _isGlobal, value); }
        }

        /// <summary>
        /// The dataset the document relates to
        /// </summary>
        public int Catalogue_ID
        {
            get { return _catalogueID; }
            set { SetField(ref _catalogueID , value); }
        }

        #endregion

        /// <summary>
        /// Creates a new supporting document for helping understand the dataset <paramref name="parent"/>
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        public SupportingDocument(ICatalogueRepository repository, Catalogue parent, string name)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"Name", name},
                {"Catalogue_ID", parent.ID}
            });
        }

        internal SupportingDocument(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            Catalogue_ID = int.Parse(r["Catalogue_ID"].ToString()); //gets around decimals and other random crud number field types that sql returns
            Name = r["Name"].ToString();
            URL = ParseUrl(r, "URL");
            Description = r["Description"].ToString();
            Ticket = r["Ticket"] as string;
            Extractable = (bool)r["Extractable"];
            IsGlobal = Convert.ToBoolean(r["IsGlobal"]);
        }
        
        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Returns true if <see cref="Extractable"/> and has a valid <see cref="URL"/>
        /// </summary>
        /// <returns></returns>
        public bool IsReleasable()
        {
            if (!Extractable)
                return false;
            
            //if it has no url or the url is blank or the url is to something that isn't a file
            if (URL == null || string.IsNullOrWhiteSpace(URL.AbsoluteUri) || !URL.IsFile)
                return false;

            //ok let the user download it through the website <- Yes that's right, this method when returns true lets anyone grab the file via the website CatalogueWebService.cs
            return true;
        }

        /// <summary>
        /// Returns the name of the file referenced by <see cref="URL"/>
        /// </summary>
        /// <returns></returns>
        public string GetFileName()
        {
            if (URL == null || string.IsNullOrWhiteSpace(URL.AbsoluteUri) || !URL.IsFile)
                return null;

            return Path.GetFileName(URL.AbsoluteUri);
        }
    }
}
