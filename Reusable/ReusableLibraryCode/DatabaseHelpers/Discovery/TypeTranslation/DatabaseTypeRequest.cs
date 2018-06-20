using System;
using System.Collections.ObjectModel;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation.TypeDeciders;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation
{
    /// <summary>
    /// Describes a cross platform database field type you want created including maximum width for string based columns and precision/scale for decimals.
    /// 
    /// <para>See ITypeTranslater to see how a DatabaseTypeRequest is turned into the proprietary string e.g. A DatabaseTypeRequest with CSharpType = typeof(DateTime)
    /// is translated into 'datetime2' in Microsoft SQL Server but 'datetime' in MySql server.</para>
    /// </summary>
    public class DatabaseTypeRequest
    {
        public static readonly ReadOnlyCollection<Type> PreferenceOrder = new ReadOnlyCollection<Type>(new Type[]
        {
            typeof(bool),
            typeof(int),
            typeof(decimal),

            typeof(TimeSpan),
            typeof(DateTime), //ironically Convert.ToDateTime likes int and floats as valid dates -- nuts
            
            typeof(string)
        });

        public Type CSharpType { get; private set; }
        public int? MaxWidthForStrings { get; private set; }
        public DecimalSize DecimalPlacesBeforeAndAfter { get; private set; }

        public DatabaseTypeRequest(Type cSharpType, int? maxWidthForStrings = null,
            DecimalSize decimalPlacesBeforeAndAfter = null)
        {
            CSharpType = cSharpType;
            MaxWidthForStrings = maxWidthForStrings;
            DecimalPlacesBeforeAndAfter = decimalPlacesBeforeAndAfter;
        }

        #region Equality
        protected bool Equals(DatabaseTypeRequest other)
        {
            return Equals(CSharpType, other.CSharpType) && MaxWidthForStrings == other.MaxWidthForStrings && Equals(DecimalPlacesBeforeAndAfter, other.DecimalPlacesBeforeAndAfter);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DatabaseTypeRequest) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (CSharpType != null ? CSharpType.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ MaxWidthForStrings.GetHashCode();
                hashCode = (hashCode*397) ^ (DecimalPlacesBeforeAndAfter != null ? DecimalPlacesBeforeAndAfter.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(DatabaseTypeRequest left, DatabaseTypeRequest right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DatabaseTypeRequest left, DatabaseTypeRequest right)
        {
            return !Equals(left, right);
        }
        #endregion

        public static DatabaseTypeRequest Max(DatabaseTypeRequest first, DatabaseTypeRequest second)
        {
            //if types differ
            if (PreferenceOrder.IndexOf(first.CSharpType) < PreferenceOrder.IndexOf(second.CSharpType))
                return second;

            if (PreferenceOrder.IndexOf(first.CSharpType) > PreferenceOrder.IndexOf(second.CSharpType))
                return first;
            
            if(!(first.CSharpType == second.CSharpType))
                throw new NotSupportedException("Cannot Max DatabaseTypeRequests because they were of differing Types and neither Type appeared in the PreferenceOrder (Types were '" + first.CSharpType +"' and '" + second.CSharpType + "')");

            int? newMaxWidthIfStrings = first.MaxWidthForStrings;

            //if first doesn't have a max string width
            if (newMaxWidthIfStrings == null)
                newMaxWidthIfStrings = second.MaxWidthForStrings; //use the second
            else if (second.MaxWidthForStrings != null)
                newMaxWidthIfStrings = Math.Max(newMaxWidthIfStrings.Value, second.MaxWidthForStrings.Value); //else use the max of the two

            //types are the same
            return new DatabaseTypeRequest(
                first.CSharpType,
                newMaxWidthIfStrings,
                DecimalSize.Combine(first.DecimalPlacesBeforeAndAfter, second.DecimalPlacesBeforeAndAfter)
                );

        }
    }
}
