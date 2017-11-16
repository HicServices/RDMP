using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data.Pipelines
{
    /// <summary>
    /// Each Pipeline has 0 or more PipelineComponents.
    /// </summary>
    public class PipelineComponent : VersionedDatabaseEntity, IPipelineComponent
    {
        #region Database Properties

        private string _name;
        private int _order;
        private int _pipelineID;
        private string _class;

        public string Name
        {
            get { return _name; }
            set { SetField(ref  _name, value); }
        }

        public int Order
        {
            get { return _order; }
            set { SetField(ref  _order, value); }
        }

        public int Pipeline_ID
        {
            get { return _pipelineID; }
            set { SetField(ref  _pipelineID, value); }
        }

        public string Class
        {
            get { return _class; }
            set { SetField(ref  _class, value); }
        }

        #endregion

        #region Relationships

        [NoMappingToDatabase]
        public IEnumerable<IPipelineComponentArgument> PipelineComponentArguments {
            get { return Repository.GetAllObjectsWithParent<PipelineComponentArgument>(this); }
        }

        [NoMappingToDatabase]
        public IHasDependencies Pipeline {
            get { return Repository.GetObjectByID<Pipeline>(Pipeline_ID); }
        }

        #endregion

        public override string ToString()
        {
            string classLastBit = Class;

            if (Class.Contains("."))
                classLastBit = Class.Substring(Class.LastIndexOf(".") + 1);

            return Name + "(" + classLastBit + ")";
        }

        public PipelineComponent(ICatalogueRepository repository, IPipeline parent, Type componentType, int order,
            string name = null)
        {
         repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"Name", name ?? "Run " + componentType.Name},
                {"Pipeline_ID", parent.ID},
                {"Class", componentType.ToString()},
                {"Order", order}
            });   
        }

        public PipelineComponent(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            Order = int.Parse(r["Order"].ToString());
            Pipeline_ID = int.Parse(r["Pipeline_ID"].ToString());
            Class = r["Class"].ToString();
            Name = r["Name"].ToString();
        }
        
        public IEnumerable<IArgument> GetAllArguments()
        {
            return PipelineComponentArguments;
        }

        public IArgument CreateNewArgument()
        {
            return new PipelineComponentArgument((ICatalogueRepository)Repository,this);
        }

        public string GetClassNameWhoArgumentsAreFor()
        {
            return Class;
        }


        public Type GetClassAsSystemType()
        {
            return ((CatalogueRepository)Repository).MEF.GetTypeByNameFromAnyLoadedAssembly(Class);
        }

        public string GetClassNameLastPart()
        {
            if (string.IsNullOrWhiteSpace(Class))
                return Class;

            return Class.Substring(Class.LastIndexOf('.') + 1);
        }

        /// <summary>
        /// Creates new ProcessTaskArguments for the supplied class T (based on what DemandsInitialization fields it has).  Parent is the ProcessTask that hosts the class T e.g. IAttacher
        /// </summary>
        /// <typeparam name="T">A class that has some DemandsInitialization fields</typeparam>
        /// <param name="parent"></param>
        public IEnumerable<PipelineComponentArgument> CreateArgumentsForClassIfNotExists<T>()
        {
           return CreateArgumentsForClassIfNotExists(typeof(T));
        }

        public PipelineComponent Clone(Pipeline intoTargetPipeline)
        {
            var cataRepo = (ICatalogueRepository) intoTargetPipeline.Repository;

            var clone = new PipelineComponent(cataRepo, intoTargetPipeline, GetClassAsSystemType(), Order);
            foreach (IPipelineComponentArgument argument in PipelineComponentArguments)
            {
                var cloneArg = new PipelineComponentArgument(cataRepo, clone);

                cloneArg.Name = argument.Name;
                cloneArg.Value = argument.Value;
                cloneArg.SetType(argument.GetSystemType());
                cloneArg.Description = argument.Description;
                cloneArg.SaveToDatabase();
            }

            clone.Name = Name;
            clone.SaveToDatabase();
            
            return clone;
        }

        public PipelineComponentArgument[] CreateArgumentsForClassIfNotExists(Type underlyingComponentType)
        {
            var argFactory = new ArgumentFactory();
            return argFactory.CreateArgumentsForClassIfNotExistsGeneric(underlyingComponentType,

                //tell it how to create new instances of us related to parent
                this,

                //what arguments already exist
                PipelineComponentArguments.ToArray())

                //convert the result back from generic to specific (us)
                .Cast<PipelineComponentArgument>().ToArray();
        }

        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            return new IHasDependencies[] {Pipeline};
        }
        
        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            return PipelineComponentArguments.Cast<IHasDependencies>().ToArray();
        }
    }
}
