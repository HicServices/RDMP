// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;

namespace Rdmp.Core.Tests.Validation;

public class TestConstants
{
    public const string _VALID_CHI = "0809821176";
    public const string _INVALID_CHI_SEX = "0204450363";
    public const string _INVALID_CHI_CHECKSUM = "0204450361";

    public static readonly Dictionary<string, object> ValidChiAndConsistentSex = new()
    {
        { "chi", _VALID_CHI },
        { "gender", "M" }
    };

    public static readonly Dictionary<string, object> ValidChiAndInconsistentSex = new()
    {
        { "chi", _VALID_CHI },
        { "gender", "F" }
    };

    public static readonly Dictionary<string, object> ValidChiAndNullSex = new()
    {
        { "chi", _VALID_CHI },
        { "gender", null }
    };

    public static readonly Dictionary<string, object> InvalidChiAndNullSex = new()
    {
        { "chi", _INVALID_CHI_CHECKSUM },
        { "gender", null }
    };

    public static readonly Dictionary<string, object> InvalidChiAndValidSex = new()
    {
        { "chi", _INVALID_CHI_CHECKSUM },
        { "gender", "F" }
    };

    public static readonly Dictionary<string, object> NullChiAndValidSex = new()
    {
        { "chi", null },
        { "gender", "F" }
    };

    public static readonly Dictionary<string, object> NullChiAndNullSex = new()
    {
        { "chi", null },
        { "gender", null }
    };

    #region For testing date bounds
    public static readonly Dictionary<string, object> AdmissionDateOccursAfterDob = new()
    {
        { "dob", new DateTime(1977, 3, 3) },
        { "admission_date", new DateTime(1987, 3, 3) }
    };

    public static readonly Dictionary<string, object> AdmissionDateOccursBeforeDob = new()
    {
        { "dob", new DateTime(1977, 3, 3) },
        { "admission_date", new DateTime(1947, 3, 3) }
    };

    public static readonly Dictionary<string, object> AdmissionDateOccursOnDob = new()
    {
        { "dob", new DateTime(1977, 3, 3) },
        { "admission_date", new DateTime(1977, 3, 3) }
    };

    public static readonly Dictionary<string, object> ParentDobOccursBeforeDob = new()
    {
        { "parent_dob", new DateTime(1917, 3, 3) },
        { "dob", new DateTime(1947, 3, 3) }
    };

    public static readonly Dictionary<string, object> ParentDobOccursAfterDob = new()
    {
        { "parent_dob", new DateTime(2000, 3, 3) },
        { "dob", new DateTime(1947, 3, 3) }
    };

    public static readonly Dictionary<string, object> ParentDobOccursOnDob = new()
    {
        { "parent_dob", new DateTime(2000, 3, 3) },
        { "dob", new DateTime(2000, 3, 2) }
    };

    public static readonly Dictionary<string, object> OperationOccursDuringStay = new()
    {
        { "admission_date", new DateTime(2012, 1, 1) },
        { "discharge_date", new DateTime(2012, 1, 5) },
        { "operation_date", new DateTime(2012, 1, 3) }
    };

    public static readonly Dictionary<string, object> OperationOccursBeforeStay = new()
    {
        { "admission_date", new DateTime(2012, 1, 1) },
        { "discharge_date", new DateTime(2012, 1, 5) },
        { "operation_date", new DateTime(2011, 1, 3) }
    };

    public static readonly Dictionary<string, object> OperationOccursAfterStay = new()
    {
        { "admission_date", new DateTime(2012, 1, 1) },
        { "discharge_date", new DateTime(2012, 1, 5) },
        { "operation_date", new DateTime(2013, 1, 3) }
    };

    public static readonly Dictionary<string, object> OperationOccursOnStartOfStay = new()
    {
        { "admission_date", new DateTime(2012, 1, 1) },
        { "discharge_date", new DateTime(2012, 1, 5) },
        { "operation_date", new DateTime(2012, 1, 1) }
    };

    public static readonly Dictionary<string, object> OperationOccursOnEndOfStay = new()
    {
        { "admission_date", new DateTime(2012, 1, 1) },
        { "discharge_date", new DateTime(2012, 1, 5) },
        { "operation_date", new DateTime(2012, 1, 5) }
    };

    #endregion
}