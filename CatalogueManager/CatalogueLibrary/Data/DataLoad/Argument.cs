using System;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Construction;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.DataLoad
{
    public abstract class Argument : VersionedDatabaseEntity, IArgument
    {
        public static readonly Type[] PermissableTypes = { typeof(char?), typeof(int?), typeof(int), typeof(string), typeof(DateTime), typeof(FileInfo), typeof(DirectoryInfo), typeof(Enum), typeof(Uri), typeof(Regex), typeof(TableInfo), typeof(ColumnInfo), typeof(PreLoadDiscardedColumn), typeof(LoadProgress), typeof(CacheProgress), typeof(ExternalDatabaseServer), typeof(bool), typeof(ICustomUIDrivenClass), typeof(EncryptedString), typeof(CatalogueRepository), typeof(Pipeline), typeof(StandardRegex), typeof(Type),typeof(CohortIdentificationConfiguration)};

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

            //null
            if (String.IsNullOrWhiteSpace(Value))
                return null;

            if (Type.Equals(typeof(Uri).ToString()))
                return new Uri(Value);

            if (Type.Equals(typeof(string).ToString()))
                return Value;

            if (Type.Equals(typeof(DateTime).ToString()))
                return DateTime.Parse(Value);

            if (Type.Equals(typeof(FileInfo).ToString()))
                return new FileInfo(Value);

            if (Type.Equals(typeof(DirectoryInfo).ToString()))
                return new DirectoryInfo(Value);

            if (Type.Equals(typeof(Regex).ToString()))
                return new Regex(Value);

            Type type = GetSystemType();

            if (typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(type))
                return Repository.GetObjectByID(GetSystemType(), Convert.ToInt32(Value));

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
            return ((CatalogueRepository)Repository).MEF.GetTypeByNameFromAnyLoadedAssembly(Type);
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
                    if (o.GetType() != type)
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