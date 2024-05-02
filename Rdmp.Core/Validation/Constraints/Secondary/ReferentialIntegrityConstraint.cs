// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Xml.Serialization;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.Validation.UIAttributes;

namespace Rdmp.Core.Validation.Constraints.Secondary;

/// <summary>
///     Specifies that values in the column must (or must not - see InvertLogic) appear in another column.  This lets you
///     have a database table which contains a
///     always allowed (or forbidden) of expected values.  This is particularly useful if you want to create a Lookup table
///     but you don't want to create a constraint
///     at database level because you expect dirty data and don't want to crash the data load.
/// </summary>
public class ReferentialIntegrityConstraint : SecondaryConstraint, ICheckable
{
    private readonly IRepository _repository;

    [Description("When ticked, the current value MUST NOT appear in the OtherColumnInfo")]
    public bool InvertLogic { get; set; }

    private int _otherColumnInfoID;

    //this is the only value that actually needs to be serialized!
    [HideOnValidationUI]
    public int OtherColumnInfoID
    {
        get => _otherColumnInfoID;
        set
        {
            _otherColumnInfoID = value;

            if (value > 0)
                OtherColumnInfo = _repository.GetObjectByID<ColumnInfo>(value);
        }
    }

    [Description(
        "The current value MUST appear in the SET OF ALL VALUES stored in the this column.  The ColumnInfo you choose does not have to be stored on the same server/database as your dataset")]
    [XmlIgnore]
    public ColumnInfo OtherColumnInfo
    {
        get => _otherColumnInfo;
        set
        {
            _otherColumnInfo = value;

            if (OtherColumnInfoID != value.ID)
                OtherColumnInfoID = value.ID;
        }
    }

    /// <summary>
    ///     Only for XmlSerializer, do not use otherwise
    /// </summary>
    public ReferentialIntegrityConstraint()
    {
        if (Validator.LocatorForXMLDeserialization == null)
            throw new Exception(
                "Cannot deserialize/construct this class because the static LocatorForXMLDeserialization field has not been set");

        _repository = Validator.LocatorForXMLDeserialization.CatalogueRepository;
    }

    public ReferentialIntegrityConstraint(IRepository repository)
    {
        _repository = repository;
    }

    private HashSet<string> _uniqueValues;
    private ColumnInfo _otherColumnInfo;


    public override void RenameColumn(string originalName, string newName)
    {
    }

    public override string GetHumanReadableDescriptionOfValidation()
    {
        if (OtherColumnInfo == null)
            return
                "Fetches all the values held in OtherColumnInfo and confirms that the values in this field are also in that collection (use cases for this constraint include cross database/server referential integrity or any time when you don't want to explicitly declare a foreign key in your database due to data quality issues)";

        var tableInfo = OtherColumnInfo.TableInfo;

        return InvertLogic
            ? $"Fetches all the values held in {OtherColumnInfo} on server {tableInfo.Server} and confirms that the values in this field ARE NOT in that collection"
            : $"Fetches all the values held in {OtherColumnInfo} on server {tableInfo.Server} and confirms that the values in this field are also in that collection";
    }

    /// <summary>
    ///     The first call to this will load all values from the validation column, which may take an appreciable amount of
    ///     time for large datasets (such as when validating against CHI).
    /// </summary>
    /// <param name="value"></param>
    /// <param name="otherColumns"></param>
    /// <param name="otherColumnNames"></param>
    /// <returns></returns>
    public override ValidationFailure Validate(object value, object[] otherColumns, string[] otherColumnNames)
    {
        if (_uniqueValues == null)
        {
            _uniqueValues = new HashSet<string>();
            GetUniqueValues();
        }

        if (value == null || value == DBNull.Value)
            return null;

        var contained = _uniqueValues.Contains(value.ToString());

        //it is in the hashset
        if (contained)
            if (!InvertLogic) //the logic is not inverted (expected behaviour)
                return null;
            else
                //the logic is inverted!
                return new ValidationFailure(
                    $"Value '{value}' was found in row and also in the column {OtherColumnInfo} (InvertLogic was set to true meaning that OtherColumnInfo is an exclusion list)",
                    this);

        //it was not contained in the hashset

        //if invert logic then hash set is an exclusion list so this is expected behaviour
        if (InvertLogic)
            return null;

        //it is not contained and we have not inverted the logic so this is a validation failure, the value was not found in the referential integrity column OtherColumnInfo
        return
            new ValidationFailure(
                $"Value '{value}' was found in row but not in corresponding referential integrity column {OtherColumnInfo}",
                this);
    }


    public void Check(ICheckNotifier checker)
    {
        if (OtherColumnInfo == null)
        {
            checker.OnCheckPerformed(
                new CheckEventArgs("No ColumnInfo has been selected yet! unable to populate constraint HashSet",
                    CheckResult.Fail));

            return;
        }

        // Attempt to get a single value from the table and then validate it
        var tableInfo = OtherColumnInfo.TableInfo;
        object itemToValidate;
        using (var con = GetConnectionToOtherTable(tableInfo))
        {
            con.Open();
            try
            {
                using var cmd = DatabaseCommandHelper.GetCommand(
                    $"SELECT TOP 1 {OtherColumnInfo} FROM {tableInfo} WHERE {OtherColumnInfo} IS NOT NULL", con);
                cmd.CommandTimeout = 5;
                itemToValidate = cmd.ExecuteScalar();
            }
            catch (Exception e)
            {
                checker.OnCheckPerformed(
                    e.HResult == -2 // timeout
                        ? new CheckEventArgs($"Timeout when trying to access lookup table: {tableInfo}",
                            CheckResult.Warning, e)
                        : new CheckEventArgs($"Failed to query validation lookup table: {tableInfo}", CheckResult.Fail,
                            e));

                return;
            }
        }

        if (itemToValidate == null)
        {
            checker.OnCheckPerformed(new CheckEventArgs($"No validation items were found in {OtherColumnInfo}",
                CheckResult.Fail));
            return;
        }

        // Now attempt to validate the item just retrieved from the lookup table. By definition it should be valid.
        var failure =
            // If it is an exclusion list then pass a guid which shouldn't match any of the items in the lookup column
            // Call to Validate may cause performance issues as it loads the entire column contents which for, e.g. CHI, can take a long time
            Validate(InvertLogic ? Guid.NewGuid().ToString() : itemToValidate, null, null);

        checker.OnCheckPerformed(failure == null
            ? new CheckEventArgs("Test Read Code validation successful", CheckResult.Success)
            : new CheckEventArgs(failure.Message, CheckResult.Fail));
    }

    /// <summary>
    ///     Loads the entire (distinct) contents of the validation column into a hashset. This operation will take some time
    ///     for very large datasets, e.g. validating against CHI, so be careful when calling.
    /// </summary>
    private void GetUniqueValues()
    {
        if (OtherColumnInfo == null)
            throw new NotSupportedException(
                "No ColumnInfo has been selected yet! unable to populate constraint HashSet");


        //Get the values off the server
        var tableInfo = OtherColumnInfo.TableInfo;
        var sqlToFetchValues = $"Select distinct {OtherColumnInfo} from {tableInfo}";

        //open connection
        using var con = GetConnectionToOtherTable(tableInfo);
        con.Open();
        try
        {
            //send the select
            using var cmd = DatabaseCommandHelper.GetCommand(sqlToFetchValues, con);
            using var reader = cmd.ExecuteReader();
            var runtimeName = OtherColumnInfo.GetRuntimeName();

            //store the values in the HashSet
            while (reader.Read())
            {
                var obj = reader[runtimeName];
                if (obj != null && obj != DBNull.Value)
                {
                    var strValue = obj.ToString();
                    if (!string.IsNullOrWhiteSpace(strValue))
                        _uniqueValues.Add(strValue);
                }
            }
        }
        catch (Exception e)
        {
            throw new Exception(
                $"Failed to execute SQL '{sqlToFetchValues}' under context {DataAccessContext.InternalDataProcessing}",
                e);
        }
    }

    private static DbConnection GetConnectionToOtherTable(IDataAccessPoint tableInfo)
    {
        return DataAccessPortal
            .ExpectServer(tableInfo, DataAccessContext.InternalDataProcessing)
            .GetConnection();
    }
}