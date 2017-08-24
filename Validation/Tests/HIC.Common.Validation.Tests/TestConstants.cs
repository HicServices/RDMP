using System;
using System.Collections.Generic;

namespace HIC.Common.Validation.Tests
{
    public class TestConstants
    {
        public const string _VALID_CHI = "0809821176";
        public const string _INVALID_CHI_SEX = "0204450363";
        public const string _INVALID_CHI_CHECKSUM = "0204450361";

        public static readonly Dictionary<string, object> ValidChiAndConsistentSex = new Dictionary<string, object>()
            {
                {"chi", _VALID_CHI},
                {"gender", "M"}
            };

        public static readonly Dictionary<string, object> ValidChiAndInconsistentSex = new Dictionary<string, object>()
            {
                {"chi", _VALID_CHI},
                {"gender", "F"}
            };

        public static readonly Dictionary<string, object> ValidChiAndNullSex = new Dictionary<string, object>()
            {
                {"chi", _VALID_CHI},
                {"gender", null}
            };

        public static readonly Dictionary<string, object> InvalidChiAndNullSex = new Dictionary<string, object>()
            {
                {"chi", _INVALID_CHI_CHECKSUM},
                {"gender", null}
            };

        public static readonly Dictionary<string, object> InvalidChiAndValidSex = new Dictionary<string, object>()
            {
                {"chi", _INVALID_CHI_CHECKSUM},
                {"gender", "F"}
            };

        public static readonly Dictionary<string, object> NullChiAndValidSex = new Dictionary<string, object>()
            {
                {"chi", null},
                {"gender", "F"}
            };

        public static readonly Dictionary<string, object> NullChiAndNullSex = new Dictionary<string, object>()
            {
                {"chi", null},
                {"gender", null}
            };

        #region For testing date bounds
        public static readonly Dictionary<string, object> AdmissionDateOccursAfterDob = new Dictionary<string, object>
                {
                    {"dob", new DateTime(1977, 3, 3)},
                    {"admission_date", new DateTime(1987, 3, 3)}
                };

        public static readonly Dictionary<string, object> AdmissionDateOccursBeforeDob = new Dictionary<string, object>
                {
                    {"dob", new DateTime(1977, 3, 3)},
                    {"admission_date", new DateTime(1947, 3, 3)}
                };

        public static readonly Dictionary<string, object> AdmissionDateOccursOnDob = new Dictionary<string, object>
                {
                    {"dob", new DateTime(1977, 3, 3)},
                    {"admission_date", new DateTime(1977, 3, 3)}
                };

        public static readonly Dictionary<string, object> ParentDobOccursBeforeDob = new Dictionary<string, object>
                {
                    {"parent_dob", new DateTime(1917, 3, 3)},
                    {"dob", new DateTime(1947, 3, 3)}
                };

        public static readonly Dictionary<string, object> ParentDobOccursAfterDob = new Dictionary<string, object>
                {
                    {"parent_dob", new DateTime(2000, 3, 3)},
                    {"dob", new DateTime(1947, 3, 3)}
                };

        public static readonly Dictionary<string, object> ParentDobOccursOnDob = new Dictionary<string, object>
                {
                    {"parent_dob", new DateTime(2000, 3, 3)},
                    {"dob", new DateTime(2000, 3, 2)}
                };

        public static readonly Dictionary<string, object> OperationOccursDuringStay = new Dictionary<string, object>
                {
                    {"admission_date", new DateTime(2012, 1, 1)},
                    {"discharge_date", new DateTime(2012, 1, 5)},
                    {"operation_date", new DateTime(2012, 1, 3)}
                };

        public static readonly Dictionary<string, object> OperationOccursBeforeStay = new Dictionary<string, object>
                {
                    {"admission_date", new DateTime(2012, 1, 1)},
                    {"discharge_date", new DateTime(2012, 1, 5)},
                    {"operation_date", new DateTime(2011, 1, 3)}
                };

        public static readonly Dictionary<string, object> OperationOccursAfterStay = new Dictionary<string, object>
                {
                    {"admission_date", new DateTime(2012, 1, 1)},
                    {"discharge_date", new DateTime(2012, 1, 5)},
                    {"operation_date", new DateTime(2013, 1, 3)}
                };

        public static readonly Dictionary<string, object> OperationOccursOnStartOfStay = new Dictionary<string, object>
                {
                    {"admission_date", new DateTime(2012, 1, 1)},
                    {"discharge_date", new DateTime(2012, 1, 5)},
                    {"operation_date", new DateTime(2012, 1, 1)}
                };

        public static readonly Dictionary<string, object> OperationOccursOnEndOfStay = new Dictionary<string, object>
                {
                    {"admission_date", new DateTime(2012, 1, 1)},
                    {"discharge_date", new DateTime(2012, 1, 5)},
                    {"operation_date", new DateTime(2012, 1, 5)}
                };
        #endregion

    }
}