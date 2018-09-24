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
    /// <inheritdoc cref="IPipelineComponent"/>
    public class PipelineComponent : VersionedDatabaseEntity, IPipelineComponent
    {
        #region Database Properties

        private string _name;
        private int _order;
        private int _pipelineID;
        private string _class;

        /// <inheritdoc/>
        public string Name
        {
            get { return _name; }
            set { SetField(ref  _name, value); }
        }

        /// <inheritdoc/>
        public int Order
        {
            get { return _order; }
            set { SetField(ref  _order, value); }
        }
        /// <inheritdoc/>
        public int Pipeline_ID
        {
            get { return _pipelineID; }
            set { SetField(ref  _pipelineID, value); }
        }
        /// <inheritdoc/>
        public string Class
        {
            get { return _class; }
            set { SetField(ref  _class, value); }
        }

        #endregion

        #region Relationships

        /// <inheritdoc/>
        [NoMappingToDatabase]
        public IEnumerable<IPipelineComponentArgument> PipelineComponentArguments {
            get { return Repository.GetAllObjectsWithParent<PipelineComponentArgument>(this); }
        }

        /// <inheritdoc cref="Pipeline_ID"/>
        [NoMappingToDatabase]
        public IHasDependencies Pipeline {
            get { return Repository.GetObjectByID<Pipeline>(Pipeline_ID); }
        }

        #endregion

        /// <inheritdoc/>
        public override string ToString()
        {
            string classLastBit = Class;

            if (Class.Contains("."))
                classLastBit = Class.Substring(Class.LastIndexOf(".") + 1);

            return Name + "(" + classLastBit + ")";
        }

        /// <summary>
        /// Creates a new component in the <paramref name="parent"/> <see cref="Pipeline"/>.  This will mean that when run the <see cref="Pipeline"/>
        /// will instantiate and run the given <paramref name="componentType"/>.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="parent"></param>
        /// <param name="componentType"></param>
        /// <param name="order"></param>
        /// <param name="name"></param>
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

        internal PipelineComponent(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            Order = int.Parse(r["Order"].ToString());
            Pipeline_ID = int.Parse(r["Pipeline_ID"].ToString());
            Class = r["Class"].ToString();
            Name = r["Name"].ToString();
        }
        
        /// <inheritdoc/>
        public IEnumerable<IArgument> GetAllArguments()
        {
            return PipelineComponentArguments;
        }
        /// <inheritdoc/>
        public IArgument CreateNewArgument()
        {
            return new PipelineComponentArgument((ICatalogueRepository)Repository,this);
        }
        /// <inheritdoc/>
        public string GetClassNameWhoArgumentsAreFor()
        {
            return Class;
        }

        /// <inheritdoc/>
        public Type GetClassAsSystemType()
        {
            return ((CatalogueRepository)Repository).MEF.GetTypeByNameFromAnyLoadedAssembly(Class);
        }

        /// <inheritdoc/>
        public string GetClassNameLastPart()
        {
            if (string.IsNullOrWhiteSpace(Class))
                return Class;

            return Class.Substring(Class.LastIndexOf('.') + 1);
        }
        
        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public IArgument[] CreateArgumentsForClassIfNotExists<T>()
        {
            return CreateArgumentsForClassIfNotExists(typeof(T));
        }

        /// <inheritdoc/>
        public IArgument[] CreateArgumentsForClassIfNotExists(Type underlyingComponentType)
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

        /// <inheritdoc/>
        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            return new IHasDependencies[] {Pipeline};
        }

        /// <inheritdoc/>
        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            return PipelineComponentArguments.Cast<IHasDependencies>().ToArray();
        }
    }
}
