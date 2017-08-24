using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using HIC.Common.Validation.Constraints;
using HIC.Common.Validation.Constraints.Primary;
using HIC.Common.Validation.Constraints.Secondary;

namespace HIC.Common.Validation
{
    /// <summary>
    /// An ItemValidator is created for each item in the target object (row) you want to validate.
    /// Each ItemValidator has a single PrimaryConstraint and zero or more SecondaryConstraint(s).
    /// </summary>
    public class ItemValidator
    {
        public PrimaryConstraint PrimaryConstraint { get; set; }
        public string TargetProperty { get; set; }

        [XmlIgnore]
        public Type ExpectedType { get; set; }

        [XmlArray]
        [XmlArrayItem("SecondaryConstraint", typeof(SecondaryConstraint))]
        public List<SecondaryConstraint> SecondaryConstraints { get; set; }

        public ItemValidator()
        {
            PrimaryConstraint = null;
            SecondaryConstraints = new List<SecondaryConstraint>();
        }

        public ItemValidator(string targetProperty)
        {
            TargetProperty = targetProperty;
            PrimaryConstraint = null;
            SecondaryConstraints = new List<SecondaryConstraint>();

        }

        public ValidationFailure ValidateAll(object columnValue, object[] otherColumns, string[] otherColumnNames)
        {
            if(otherColumns == null)
                throw new Exception("we were not passed any otherColumns");

            if (otherColumns.Length != otherColumnNames.Length)
                throw new Exception(
                    "there was a difference between the number of columns and the number of column names passed for validation");

            ValidationFailure result = ValidatePrimaryConstraint(columnValue);

            if (result != null)
                return result;

            return ValidateSecondayConstraints(columnValue, otherColumns, otherColumnNames);
        }

        private ValidationFailure ValidatePrimaryConstraint(object columnValue)
        {
            if (PrimaryConstraint != null)
                return PrimaryConstraint.Validate(columnValue);

            return null;
        }

        private ValidationFailure ValidateSecondayConstraints(object columnValue, object[] otherColumns, string[] otherColumnNames)
        {
            foreach (ISecondaryConstraint secondaryConstraint in SecondaryConstraints)
            {
                ValidationFailure result = secondaryConstraint.Validate(columnValue, otherColumns, otherColumnNames);

                if (result != null)
                    return result;
            }

            return null;
        }

        public void AddSecondaryConstraint(SecondaryConstraint c)
        {
            SecondaryConstraints.Add(c);
        }

        public override string ToString()
        {
            return TargetProperty;
        }

        #region Fluent API experiment

        public void As(string constraintType)
        {
            this.PrimaryConstraint = (PrimaryConstraint)Validator.CreateConstraint(constraintType,Consequence.Wrong);
        }

        public ItemValidator OfType(Type type)
        {
            this.ExpectedType = type;
            return this;
        }

        public void OccursAfter(string comparatorFieldName)
        {
            var b = (BoundDate)Validator.CreateConstraint("bounddate",Consequence.Wrong);
            b.Inclusive = false;
            b.LowerFieldName = comparatorFieldName;
            b.Upper = DateTime.MaxValue;

            this.AddSecondaryConstraint(b);
        }
        
        public void OccursBefore(string comparatorFieldName)
        {
            var b = (BoundDate)Validator.CreateConstraint("bounddate",Consequence.Wrong);
            b.Lower = DateTime.MinValue;
            b.UpperFieldName = comparatorFieldName;

            this.AddSecondaryConstraint(b);
        }

        public ItemValidator IsLessThan(string comparatorFieldName)
        {
            var b = (BoundDouble)Validator.CreateConstraint("bounddouble",Consequence.Wrong);
            b.UpperFieldName = comparatorFieldName;
            b.Lower = double.MinValue;
            this.AddSecondaryConstraint(b);

            return this;
        }

        public ItemValidator IsGreaterThan(string comparatorFieldName)
        {
            var b = (BoundDouble)Validator.CreateConstraint("bounddouble",Consequence.Wrong);
            b.LowerFieldName = comparatorFieldName;
            b.Upper = double.MaxValue;
            this.AddSecondaryConstraint(b);

            return this;
        }

        public BoundDouble IsBetween(int lower)
        {
            var b = (BoundDouble)Validator.CreateConstraint("bounddouble",Consequence.Wrong);
            b.Lower = lower;
            this.AddSecondaryConstraint(b);

            return b;
        }

        public ItemValidator And()
        {
            return this;
        }

        #endregion

        //This is static because creating new ones with the Type[] causes memory leaks in unmanaged memory   https://blogs.msdn.microsoft.com/tess/2006/02/15/net-memory-leak-xmlserializing-your-way-to-a-memory-leak/
        private static XmlSerializer _serializer;

        /// <summary>
        /// Persist the current ItemValidator instance to a string containing XML.
        /// </summary>
        /// <returns>a String</returns>
        public string SaveToXml(bool indent = true)
        {
            if(_serializer == null)
                _serializer = new XmlSerializer(typeof(ItemValidator), Validator.GetExtraTypes());

            var sb = new StringBuilder();

            using (var sw = XmlWriter.Create(sb, new XmlWriterSettings { Indent = indent }))
                _serializer.Serialize(sw, this);

            return sb.ToString();
        
        }
    }
}
