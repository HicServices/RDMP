// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Validation.UIAttributes;

namespace Rdmp.Core.Validation.Constraints.Secondary;

/// <summary>
///     Values being validated are expected to pass the Regex pattern.  The pattern itself is a reference to a
///     StandardRegex which is a central curated definition
///     pattern in the Catalogue database.  This allows you to have multiple columns/validation rules in multiple datasets
///     share the same regex without having to
///     create copies (and allows you to update the definition in one place).
/// </summary>
public class StandardRegexConstraint : SecondaryConstraint
{
    private readonly IRepository _repository;
    private Regex _regex;
    private StandardRegex _standardRegex;
    private int _standardRegexID;

    /// <summary>
    ///     Only for XmlSerializer, do not use otherwise
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
        get => _standardRegexID;
        set
        {
            _standardRegexID = value;

            CatalogueStandardRegex = value == 0 ? null : _repository.GetObjectByID<StandardRegex>(value);
        }
    }

    [Description(
        "The Regular Expression pattern that MUST match the value being validated.  This is a centralised definition of a Concept stored in the Catalogue (Click the RegEx button to edit these)")]
    [XmlIgnore]
    public StandardRegex CatalogueStandardRegex
    {
        get => _standardRegex;
        set
        {
            _standardRegex = value;

            if (value == null)
            {
                _regex = null;
                return;
            }

            _regex = new Regex(value.Regex);

            //check is not redundant because assigning the field has repercusions and would result in circular reference! (Blame XMLSerialization for this cluster F*)
            if (StandardRegexID != value.ID)
                StandardRegexID = value.ID;
        }
    }

    public override void RenameColumn(string originalName, string newName)
    {
    }

    public override string GetHumanReadableDescriptionOfValidation()
    {
        return CatalogueStandardRegex != null
            ? $"Checks that the value conforms to the agency specific StandardRegex concept '{CatalogueStandardRegex.ConceptName}' defined in the Catalogue"
            : "Checks that values match the supplied agency specific StandardRegex defined in the Catalogue for core concepts (e.g. Gender)";
    }

    public override ValidationFailure Validate(object value, object[] otherColumns, string[] otherColumnNames)
    {
        if (value == null || value == DBNull.Value)
            return null;

        if (string.IsNullOrWhiteSpace(value.ToString()))
            return null;

        return _regex.IsMatch(value.ToString())
            ? null
            : new ValidationFailure(
                $"Value {value} did not match pattern for StandardRegex concept '{CatalogueStandardRegex.ConceptName}'",
                this);
    }
}