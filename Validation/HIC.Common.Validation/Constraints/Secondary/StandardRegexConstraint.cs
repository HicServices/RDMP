using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using CatalogueLibrary.Data;
using HIC.Common.Validation.UIAttributes;
using MapsDirectlyToDatabaseTable;

namespace HIC.Common.Validation.Constraints.Secondary
{
    public class StandardRegexConstraint : SecondaryConstraint
    {
        private readonly IRepository _repository;
        private Regex _regex;
        private StandardRegex _standardRegex;
        private int _standardRegexID;

        /// <summary>
        /// Only for XMLSerializer, do not use otherwise
        /// </summary>
        public StandardRegexConstraint()
        {
            _repository = Validator.LocatorForXMLDeserialization.CatalogueRepository;
        }

        public StandardRegexConstraint(IRepository repository)
        {
            _repository = repository;
        }

        //this is the only value that actually needs to be serialized!
        [HideOnValidationUI]
        public int StandardRegexID
        {
            get { return _standardRegexID; }
            set
            {
                _standardRegexID = value;

                if (value == 0)
                    CatalogueStandardRegex = null;
                else
                    CatalogueStandardRegex = _repository.GetObjectByID<StandardRegex>(value);
            }
        }

        [Description("The Regular Expression pattern that MUST match the value being validated.  This is a centralised definition of a Concept stored in the Catalogue (Click the RegEx button to edit these)")]
        [XmlIgnore]
        public StandardRegex CatalogueStandardRegex
        {
            get { return _standardRegex; }
            set
            {
                _standardRegex = value;

                if (value == null)
                {
                    _regex = null;
                    return;
                }
                else
                {
                    _regex = new Regex(value.Regex);

                    //check is not redundant because assigning the field has repercusions and would result in circular reference! (Blame XMLSerialization for this cluster F*)
                    if(StandardRegexID != value.ID)
                        StandardRegexID = value.ID;
                }
            }
        }

        public override void RenameColumn(string originalName, string newName)
        {
            
        }

        public override string GetHumanReadableDescriptionOfValidation()
        {
            if (CatalogueStandardRegex != null)
                return "Checks that the value conforms to the agency specific StandardRegex concept '" + CatalogueStandardRegex.ConceptName + "' defined in the Catalogue";

            return "Checks that values match the supplied agency specific StandardRegex defined in the Catalogue for core concepts (e.g. Gender)";
        }
        
        public override ValidationFailure Validate(object value, object[] otherColumns, string[] otherColumnNames)
        {
            if (value == null || value == DBNull.Value)
                return null;
            
            if(string.IsNullOrWhiteSpace(value.ToString()))
                return null;

            if (_regex.IsMatch(value.ToString()))
                return null;

            return new ValidationFailure("Value " + value + " did not match pattern for StandardRegex concept '" + CatalogueStandardRegex.ConceptName + "'", this);
            
        }

    }
}
