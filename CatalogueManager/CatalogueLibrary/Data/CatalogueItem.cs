using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;


namespace CatalogueLibrary.Data
{
    /// <summary>
    /// A virtual column that is made available to researchers.  Each Catalogue has 1 or more CatalogueItems, these contain the descriptions of what is contained
    /// in the column as well as any outstanding/resolved issues with the column (see CatalogueItemIssue).
    /// 
    /// It is important to note that CatalogueItems are not tied to underlying database tables/columns except via an ExtractionInformation.  This means that you can
    /// for example have multiple different versions of the same underlying ColumnInfo 
    /// 
    /// e.g.
    /// CatalogueItem: PatientDateOfBirth (ExtractionInformation verbatim but 'Special Approval Required')
    /// CatalogueItem: PatientDateOfBirthApprox (ExtractionInformation rounds to nearest quarter but governance is 'Core')
    /// 
    /// Both the above would extract from the same ColumnInfo DateOfBirth
    /// </summary>
    public class CatalogueItem : VersionedDatabaseEntity, IDeleteable, IComparable, IHasDependencies, IRevertable, INamed
    {
        #region Database Properties
        public static int Name_MaxLength = -1;
        public static int Statistical_cons_MaxLength = -1;
        public static int Research_relevance_MaxLength = -1;
        public static int Description_MaxLength = -1;
        public static int Topic_MaxLength = -1;
        public static int Agg_method_MaxLength = -1;
        public static int Limitations_MaxLength = -1;
        public static int Comments_MaxLength = -1;

        public string _Name;
        public string _Statistical_cons;
        public string _Research_relevance;
        public string _Description;
        public string _Topic;
        public string _Agg_method;
        public string _Limitations;
        public string _Comments;
        private int _catalogueID;
        private int? _columnInfoID;
        private Catalogue.CataloguePeriodicity _periodicity;

        [DoNotExtractProperty]
        public int Catalogue_ID
        {
            get { return _catalogueID; }
            set { SetField(ref _catalogueID , value); }
        }
        
        public string Name {
            get { return _Name;}
            set {SetField(ref _Name,value);} 
        }

        public string Statistical_cons {
            get { return _Statistical_cons; }
            set
            {
                SetField(ref _Statistical_cons, value);
            } 
        }

        public string Research_relevance
        {
            get { return _Research_relevance; }
            set { SetField(ref _Research_relevance , value);}
        }

        public string Description
        {
            get { return _Description; }
            set { SetField(ref _Description , value);}
        }
        public string Topic
        {
            get { return _Topic; }
            set { SetField(ref _Topic , value);}
        }

        public string Agg_method
        {
            get { return _Agg_method; }
            set { SetField(ref _Agg_method , value);}
        }

        public string Limitations
        {
            get { return _Limitations; }
            set{ SetField(ref _Limitations , value);}
        }

        public string Comments
        {
            get { return _Comments; }
            set { SetField( ref _Comments , value);}
        }


        public int? ColumnInfo_ID
        {
            get { return _columnInfoID; }
            set { SetField(ref _columnInfoID , value); }
        }

        public Catalogue.CataloguePeriodicity Periodicity
        {
            get { return _periodicity; }
            set { SetField(ref _periodicity , value); }
        }

        #endregion


        #region Relationships
        [NoMappingToDatabase]
        public Catalogue Catalogue {
            get { return Repository.GetObjectByID<Catalogue>(Catalogue_ID); }
        }

        [NoMappingToDatabase]
        public ExtractionInformation ExtractionInformation
        {
            get
            {
                return Repository.GetAllObjectsWithParent<ExtractionInformation>(this).SingleOrDefault();
            }
        }

        [NoMappingToDatabase]
        public ColumnInfo ColumnInfo
        {
            get
            {
                try
                {
                    return ColumnInfo_ID == null ? null : Repository.GetObjectByID<ColumnInfo>(ColumnInfo_ID.Value);
                }
                catch (KeyNotFoundException) //The ColumnInfo has been deleted elsewhere in the program? but this local object in memory doesn't know that
                {
                    //Let's make sure that's definetly the case
                    if(HasLocalChanges().Differences.Any(d=>d.Property.Name.Equals("ColumnInfo_ID") && d.DatabaseValue == null))
                    {
                        ColumnInfo_ID = null;
                        return null;
                    }

                    throw;
                }
            }
        }

        [NoMappingToDatabase]
        public IEnumerable<CatalogueItemIssue> CatalogueItemIssues
        {
            get { return Repository.GetAllObjectsWithParent<CatalogueItemIssue>(this); }
        }

        #endregion

        public CatalogueItem(ICatalogueRepository repository, Catalogue parent, string name)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"Name", name},
                {"Catalogue_ID", parent.ID}
            });
        }

        internal CatalogueItem(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            Catalogue_ID = int.Parse(r["Catalogue_ID"].ToString()); //gets around decimals and other random crud number field types that sql returns
            Name = (string)r["Name"];
            Statistical_cons = r["Statistical_cons"].ToString();
            Research_relevance = r["Research_relevance"].ToString();
            Description = r["Description"].ToString();
            Topic = r["Topic"].ToString();
            Agg_method = r["Agg_method"].ToString();
            Limitations = r["Limitations"].ToString();
            Comments = r["Comments"].ToString();
            ColumnInfo_ID = ObjectToNullableInt(r["ColumnInfo_ID"]);

            //Periodicity - with handling for invalid enum values listed in database
            object periodicity = r["Periodicity"];
            if (periodicity == null || periodicity == DBNull.Value)
                Periodicity = Catalogue.CataloguePeriodicity.Unknown;
            else
            {
                Catalogue.CataloguePeriodicity periodicityAsEnum;

                if(Catalogue.CataloguePeriodicity.TryParse(periodicity.ToString(), true, out periodicityAsEnum))
                    Periodicity = periodicityAsEnum;
                else
                     Periodicity = Catalogue.CataloguePeriodicity.Unknown;

            }
        }

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(object obj)
        {
            if (obj is CatalogueItem)
            {
                return -(obj.ToString().CompareTo(this.ToString())); //sort alphabetically (reverse)
            }

            throw new Exception("Cannot compare " + this.GetType().Name + " to " + obj.GetType().Name);
        }
        
        
        public CatalogueItem CloneCatalogueItemWithIDIntoCatalogue(Catalogue cataToImportTo)
        {
            if(this.Catalogue_ID == cataToImportTo.ID)
                throw new ArgumentException("Cannot clone a CatalogueItem into it's own parent, specify a different catalogue to clone into");

            var clone = new CatalogueItem((ICatalogueRepository)cataToImportTo.Repository, cataToImportTo, this.Name);
            
            //Get all the properties           
            PropertyInfo[] propertyInfo = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            //Assign all source property to taget object 's properties
            foreach (PropertyInfo property in propertyInfo)
            {
                //Check whether property can be written to
                if (property.CanWrite && !property.Name.Equals("ID") && !property.Name.Equals("Catalogue_ID"))
                    if (property.PropertyType.IsValueType || property.PropertyType.IsEnum || property.PropertyType.Equals(typeof(System.String)))
                        property.SetValue(clone, property.GetValue(this, null), null);
            }

            clone.SaveToDatabase();
            
            return clone;
        }


        public IEnumerable<ColumnInfo> GuessAssociatedColumn(ColumnInfo[] guessPool)
        {
            //exact matches exist so return those
            ColumnInfo[] Guess = guessPool.Where(col => col.GetRuntimeName().Equals(this.Name)).ToArray();
            if (Guess.Any())
                return Guess;

            //ignore caps match instead
            Guess = guessPool.Where(col => col.GetRuntimeName().ToLower().Equals(this.Name.ToLower())).ToArray();
            if (Guess.Any())
                return Guess;

            //ignore caps and remove spaces match instead
            Guess =
                guessPool.Where(col => col.GetRuntimeName().ToLower().Replace(" ", "").Equals(this.Name.ToLower().Replace(" ", ""))).ToArray();
            if (Guess.Any())
                return Guess;

            //contains match is final last resort
            return guessPool.Where(col =>
                col.GetRuntimeName().ToLower().Contains(this.Name.ToLower())
                ||
                Name.ToLower().Contains(col.GetRuntimeName().ToLower()));
            
        }

        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            return null;
        }

        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            List<IHasDependencies> dependantObjects = new List<IHasDependencies>();

            var exInfo = ExtractionInformation;

            if(exInfo != null)
                dependantObjects.Add(exInfo);

            if(ColumnInfo_ID != null)
                dependantObjects.Add(ColumnInfo);

            dependantObjects.Add(Catalogue);
            return dependantObjects.ToArray();
        }

        public void SetColumnInfo(ColumnInfo columnInfo)
        {
            ((CatalogueRepository)Repository).SaveSpecificPropertyOnlyToDatabase(this,"ColumnInfo_ID",columnInfo != null? (object) columnInfo.ID:null);
        }
    }
}
