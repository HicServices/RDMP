using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Rdmp.Core.Validation.Constraints;
using Rdmp.Core.Validation.Constraints.Primary;
using Rdmp.Core.Validation.Constraints.Secondary;

namespace Rdmp.Core.Validation;

/// <summary>
///     An ItemValidator is created for each item in the target object (row) you want to validate.
///     Each ItemValidator has a single PrimaryConstraint and zero or more SecondaryConstraint(s).
/// </summary>
public class ItemValidator
{
    //This is static because creating new ones with the Type[] causes memory leaks in unmanaged memory   https://blogs.msdn.microsoft.com/tess/2006/02/15/net-memory-leak-xmlserializing-your-way-to-a-memory-leak/
    private static XmlSerializer _serializer;

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

    public PrimaryConstraint PrimaryConstraint { get; set; }
    public string TargetProperty { get; set; }

    [XmlIgnore] public Type ExpectedType { get; set; }

    [XmlArray]
    [XmlArrayItem("SecondaryConstraint", typeof(SecondaryConstraint))]
    public List<SecondaryConstraint> SecondaryConstraints { get; set; }

    public ValidationFailure ValidateAll(object columnValue, object[] otherColumns, string[] otherColumnNames)
    {
        if (otherColumns == null)
            throw new Exception("we were not passed any otherColumns");

        if (otherColumns.Length != otherColumnNames.Length)
            throw new Exception(
                "there was a difference between the number of columns and the number of column names passed for validation");

        var result = ValidatePrimaryConstraint(columnValue);

        return result ?? ValidateSecondaryConstraints(columnValue, otherColumns, otherColumnNames);
    }

    private ValidationFailure ValidatePrimaryConstraint(object columnValue)
    {
        if (PrimaryConstraint == null)
            return null;
        try
        {
            return PrimaryConstraint.Validate(columnValue);
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"Error processing PrimaryConstraint validator of Type {PrimaryConstraint.GetType().Name} on column {TargetProperty}.  Value being validated was '{columnValue}'",
                ex);
        }
    }

    private ValidationFailure ValidateSecondaryConstraints(object columnValue, object[] otherColumns,
        string[] otherColumnNames)
    {
        foreach (ISecondaryConstraint secondaryConstraint in SecondaryConstraints)
            try
            {
                var result = secondaryConstraint.Validate(columnValue, otherColumns, otherColumnNames);

                if (result != null)
                    return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error processing Secondary Constraint validator of Type {secondaryConstraint.GetType().Name} on column {TargetProperty}.  Value being validated was '{columnValue}'",
                    ex);
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

    /// <summary>
    ///     Persist the current ItemValidator instance to a string containing XML.
    /// </summary>
    /// <returns>a String</returns>
    public string SaveToXml(bool indent = true)
    {
        _serializer ??= new XmlSerializer(typeof(ItemValidator), Validator.GetExtraTypes().ToArray());

        var sb = new StringBuilder();

        using (var sw = XmlWriter.Create(sb, new XmlWriterSettings { Indent = indent }))
        {
            _serializer.Serialize(sw, this);
        }

        return sb.ToString();
    }

    #region Fluent API experiment

    public void As(string constraintType)
    {
        PrimaryConstraint = (PrimaryConstraint)Validator.CreateConstraint(constraintType, Consequence.Wrong);
    }

    public ItemValidator OfType(Type type)
    {
        ExpectedType = type;
        return this;
    }

    public void OccursAfter(string comparatorFieldName)
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);
        b.Inclusive = false;
        b.LowerFieldName = comparatorFieldName;
        b.Upper = DateTime.MaxValue;

        AddSecondaryConstraint(b);
    }

    public void OccursBefore(string comparatorFieldName)
    {
        var b = (BoundDate)Validator.CreateConstraint("bounddate", Consequence.Wrong);
        b.Lower = DateTime.MinValue;
        b.UpperFieldName = comparatorFieldName;

        AddSecondaryConstraint(b);
    }

    public ItemValidator IsLessThan(string comparatorFieldName)
    {
        var b = (BoundDouble)Validator.CreateConstraint("bounddouble", Consequence.Wrong);
        b.UpperFieldName = comparatorFieldName;
        b.Lower = double.MinValue;
        AddSecondaryConstraint(b);

        return this;
    }

    public ItemValidator IsGreaterThan(string comparatorFieldName)
    {
        var b = (BoundDouble)Validator.CreateConstraint("bounddouble", Consequence.Wrong);
        b.LowerFieldName = comparatorFieldName;
        b.Upper = double.MaxValue;
        AddSecondaryConstraint(b);

        return this;
    }

    public BoundDouble IsBetween(int lower)
    {
        var b = (BoundDouble)Validator.CreateConstraint("bounddouble", Consequence.Wrong);
        b.Lower = lower;
        AddSecondaryConstraint(b);

        return b;
    }

    public ItemValidator And()
    {
        return this;
    }

    #endregion
}