using System;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.Data.Remoting;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Construction;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;

namespace CatalogueLibrary.Data.DataLoad
{
    /// <summary>
    /// Abstract base for all concrete IArgument objects.  An Argument is a stored value for a Property defined on a PipelineComponent or DLE component which has
    /// been decorated with [DemandsInitialization] and for which the user has picked a value.  The class includes both the Type of the argument (extracted from
    /// the class Property PropertyInfo via reflection) and the Value (stored in the database as a string).
    /// 
    /// <para>This allows simple UI driven population and persistence of configuration settings for plugin and system core components as they are used in all pipeline and
    /// dle activities.  See ArgumentCollection for UI logic.</para>
    /// </summary>
    public abstract class Argument : VersionedDatabaseEntity, IArgument
    {
        public static readonly Type[] PermissableTypes =
        {
            typeof(char?),typeof(char),
            typeof(int?), typeof(int),
            typeof(DateTime?), typeof(DateTime),
            typeof(double?), typeof(double),
            typeof(float?), typeof(float),

            typeof(bool), //no nullable bools please

            typeof(string), typeof(FileInfo),
            typeof(DirectoryInfo),
            typeof(Enum), typeof(Uri), typeof(Regex),
            
            typeof(Type),

            //IMapsDirectlyToDatabaseTable
            typeof(TableInfo), typeof(ColumnInfo), typeof(PreLoadDiscardedColumn), typeof(LoadProgress), 
            typeof(CacheProgress), typeof(ExternalDatabaseServer), typeof(StandardRegex),typeof(CohortIdentificationConfiguration),
            typeof(RemoteRDMP),
            typeof(DataAccessCredentials),
           
            //wierd special cases
            typeof(ICustomUIDrivenClass), typeof(EncryptedString),
            
            //special static argument type, always gets the same value never has a database persisted value
            typeof(CatalogueRepository), 
            
            //user must be IDemandToUseAPipeline<T>
            typeof(Pipeline)
        };

        #region Database Properties

        private string _name;
        private string _value;
        private string _type;
        private string _description;
        
        public string Name
        {
            get { return _name; }
            set { SetField(ref  _name, value); }
        }

        [AdjustableLocation]
        public string Value
        {
            get { return _value; }
            set { SetField(ref  _value, value); }
        }

        public string Type
        {
            get { return _type; }
            protected set { SetField(ref  _type, value); }
        }

        public string Description
        {
            get { return _description; }
            set { SetField(ref  _description, value); }
        }

        #endregion

        protected Argument()
        {
        }

        protected Argument(ICatalogueRepository repository, DbDataReader dataReader)
            : base(repository, dataReader)
        {
        }

        public object GetValueAsSystemType()
        {
            object customType;
            if (HandleIfCustom(out customType))
                return customType;

            //bool
            if (Type.Equals(typeof(bool).ToString()))
            {
                if (String.IsNullOrWhiteSpace(Value))
                    return false;

                return Convert.ToBoolean(Value);
            }

            if (Type.Equals(typeof (Type).ToString()))
                if (string.IsNullOrWhiteSpace(Value))
                    return null;
                else
                    return ((CatalogueRepository) Repository).MEF.GetTypeByNameFromAnyLoadedAssembly(Value);

            if (Type.Equals(typeof (CatalogueRepository).ToString()))
                return Repository;


            //float?
            if (Type.Equals(typeof(float?).ToString()) || Type.Equals(typeof(float).ToString()))
                if (string.IsNullOrWhiteSpace(Value))
                    return null;
                else
                    return float.Parse(Value);

            //double?
            if (Type.Equals(typeof(double?).ToString()) || Type.Equals(typeof(double).ToString()))
                if (string.IsNullOrWhiteSpace(Value))
                    return null;
                else
                    return double.Parse(Value);

            //int?
            if (Type.Equals(typeof(int?).ToString()) || Type.Equals(typeof(int).ToString()))
                if (string.IsNullOrWhiteSpace(Value))
                    return null;
                else
                    return int.Parse(Value);
            
            //char?
            if (Type.Equals(typeof(char?).ToString()) || Type.Equals(typeof(char).ToString()))
                if (string.IsNullOrWhiteSpace(Value))
                    return null;
                else
                    return char.Parse(Value);

            //DateTime?
            if (Type.Equals(typeof(DateTime?).ToString()) || Type.Equals(typeof(DateTime).ToString()))
                if (string.IsNullOrWhiteSpace(Value))
                    return null;
                else
                    return DateTime.Parse(Value);

            //null
            if (String.IsNullOrWhiteSpace(Value))
                return null;

            if (Type.Equals(typeof(Uri).ToString()))
                return new Uri(Value);

            if (Type.Equals(typeof(string).ToString()))
                return Value;

            if (Type.Equals(typeof(FileInfo).ToString()))
                return new FileInfo(Value);

            if (Type.Equals(typeof(DirectoryInfo).ToString()))
                return new DirectoryInfo(Value);

            if (Type.Equals(typeof(Regex).ToString()))
                return new Regex(Value);

            Type type = GetSystemType();

            if (typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(type))
            {
                //if it is interface e.g. ITableInfo fetch instead the TableInfo object
                if (type.IsInterface && type.Name.StartsWith("I"))
                    type = ((CatalogueRepository) Repository).MEF.GetTypeByNameFromAnyLoadedAssembly(type.Name.Substring(1));

                return Repository.GetObjectByID(type, Convert.ToInt32(Value));
            }

            if (Type.Equals(typeof (EncryptedString).ToString()))
                return new EncryptedString((CatalogueRepository) Repository) {Value = Value};
            
            throw new NotSupportedException("Custom arguments cannot be of type " + Type);

        }

        private bool HandleIfCustom(out object answer)
        {
            answer = null;

            //try to enum it 
            Type type = GetSystemType();

            if (typeof(Enum).IsAssignableFrom(type))
                if (string.IsNullOrWhiteSpace(Value))
                    return true;
                else
                {
                    answer = Enum.Parse(type, Value);
                    return true;
                }

            //if it is data driven
            if (typeof(ICustomUIDrivenClass).IsAssignableFrom(type))
            {
                ICustomUIDrivenClass result;

                try
                {
                    Type t = ((CatalogueRepository) Repository).MEF.GetTypeByNameFromAnyLoadedAssembly(type.FullName);

                    ObjectConstructor constructor = new ObjectConstructor();

                    result = (ICustomUIDrivenClass) constructor.Construct(t, (ICatalogueRepository) Repository);
                     
                }
                catch (Exception e)
                {
                    throw new Exception("Failed to create an ICustomUIDrivenClass of type " + type.FullName + " make sure that you mark your class as public, commit it to the catalogue and mark it with the export '[Export(typeof(ICustomUIDrivenClass))]'", e);
                }

                try
                {
                    result.RestoreStateFrom(Value);//, (CatalogueRepository)Repository);
                }
                catch (Exception e)
                {
                    throw new Exception("RestoreState failed on your ICustomUIDrivenClass called " + type.FullName + " the restore value was the string value '" + Value + "'", e);
                }

                answer =  result;
                return true;
            }

            //it is not a custom ui driven type
            return false;
        }

        public Type GetSystemType()
        {
            //if we know they type (it is exactly one we are expecting)
            foreach (Type knownType in PermissableTypes)
            {
                //return the type
                if (knownType.ToString().Equals(Type))
                    return knownType;
            }

            //it is an unknown Type e.g. Bob where Bob is an ICustomUIDrivenClass or something
            var anyType =  ((CatalogueRepository)Repository).MEF.GetTypeByNameFromAnyLoadedAssembly(Type);

            if(anyType == null)
                throw new Exception("Could not figure out what SystemType to use for Type = '" + Type +"'");

            return anyType;
        }

        public void SetType(Type t)
        {
            //anything that is a child of a permissable type
            //if (!PermissableTypes.Any(tp => tp.IsAssignableFrom(t)))
            //        throw new NotSupportedException("Type " + t + " is not a permissable type for ProcessTaskArguments");

            Type = t.ToString();
        }

        public void SetValue(object o)
        {
            //anything implementing this interface is permitted 
            if (o is ICustomUIDrivenClass)
            {
                Value = ((ICustomUIDrivenClass)o).SaveStateToString();
                return;
            }

            if (o == null)
            {
                Value = null;
                return;
            }

            if (o is Type)
            {
                Value = o.ToString();//We are being asked to store a Type e.g. MyPlugins.MyCustomSQLHacker instead of an instance so easy, we just store the Type as a full name
                return;
            }

            //get the system type
            Type type = GetSystemType();

            if (o is String)
            {
                if (typeof (IEncryptedString).IsAssignableFrom(type))
                {
                    var encryptor = new EncryptedString((CatalogueRepository) Repository);
                    encryptor.Value = o.ToString();
                    Value = encryptor.Value;
                    return;
                }
                else
                {
                    Value = o.ToString();
                    return;
                }
            }

            //if it's a nullable type find the underlying Type
            if (type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = Nullable.GetUnderlyingType(type);

            //if we already have a known type set on us
            if (!String.IsNullOrWhiteSpace(Type))
            {
                //if we are not being passed an Enum
                if (!typeof(Enum).IsAssignableFrom(type))
                {
                    //if we have been given an illegal typed object
                    if (!PermissableTypes.Contains(o.GetType()))
                        throw new NotSupportedException("Type " + o.GetType() + " is not one of the permissable types for ProcessTaskArgument, argument must be one of:" + PermissableTypes.Aggregate("", (s, n) => s + n + ",").TrimEnd(','));

                    //if we are passed something o of differing type to the known requested type then someone is lying to someone!
                    if (type != null && !type.IsInstanceOfType(o))
                        throw new Exception("Cannot set value " + o + " (of Type " + o.GetType().FullName + ") to on ProcessTaskArgument because it has an incompatible Type specified (" + type.FullName + ")");                        
                }
            }
            
            if (o is IMapsDirectlyToDatabaseTable)
                Value = ((IMapsDirectlyToDatabaseTable)o).ID.ToString();
            else
                Value = o.ToString();
        }
    }
}
