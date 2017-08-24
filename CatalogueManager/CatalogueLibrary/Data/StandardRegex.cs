using System;
using System.Collections.Generic;
using System.Data.Common;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// A reusable regular expression which is available system wide.  Use these to record important standardised concepts which you need to use in RDMP.  For example if you have a
    /// blacklist for forbidden column names instead of copying and pasting the definition everywhere and into plugins etc you can define it once in the catalogue database as a 
    /// StandardRegex with a description and then everyone can link against it and have access to a centralised description.  This prevents you having multiple arguments getting out
    /// of sync in Pipeline components for example.
    /// </summary>
    public class StandardRegex : DatabaseEntity
    {
        #region Database Properties
        private string _conceptName;
        private string _regex;
        private string _description;

        public string ConceptName
        {
            get { return _conceptName; }
            set { SetField(ref _conceptName, value); }
        }

        /// <summary>
        /// The string that is the Pattern of the Regex, the user can happily type in invalid stuff and it will not break until it is used at runtime (so that we don't bust up at Design Time)
        /// </summary>
        public string Regex
        {
            get { return _regex; }
            set { SetField(ref _regex, value); }
        }
        public string Description
        {
            get { return _description; }
            set { SetField(ref _description, value); }
        }

        #endregion

        public StandardRegex(ICatalogueRepository repository)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"ConceptName", "New StandardRegex" + Guid.NewGuid()},
                {"Regex", ".*"}
            });
        }

        public StandardRegex(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            ConceptName = r["ConceptName"].ToString();
            Regex = r["Regex"].ToString();
            Description = r["Description"] as string;
        }

        public override string ToString()
        {
            return ConceptName;
        }
    }
}