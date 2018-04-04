using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data.DataLoad
{
    /// <summary>
    /// Contains a strongly typed value which will be provided to an instantiated data class of ProcessTask at runtime.  These usually correspond
    /// 1 to 1 with [DemandsInitialization] flagged properties of a data class e.g. 'LoadModules.Generic.Attachers.AnySeparatorFileAttacher' would have
    /// a ProcessTaskArgument record for the property UnderReadBehaviour and one for IgnoreBlankLines and one for IgnoreQuotes etc. 
    /// 
    /// <para>This all happens transparently by reflection and is handled at design time through PluginProcessTaskUI seamlessly</para>
    /// </summary>
    public class ProcessTaskArgument : Argument
    {
        #region Database Properties
        private int _processTask_ID;

        public int ProcessTask_ID
        {
            get { return _processTask_ID; }
            set { SetField(ref _processTask_ID, value); }
        }

        #endregion

        public ProcessTaskArgument(ICatalogueRepository repository, ProcessTask parent)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"ProcessTask_ID", parent.ID},
                {"Name", "Parameter" + Guid.NewGuid()},
                {"Type", typeof (string).ToString()}
            });
        }

        internal ProcessTaskArgument(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            ProcessTask_ID = int.Parse(r["ProcessTask_ID"].ToString());
            Type = r["Type"].ToString();
            Name = r["Name"].ToString();
            Value = r["Value"] as string;
            Description = r["Description"] as string;
        }
        
        public override string ToString()
        {
            return Name;
        }
        
        /// <summary>
        /// Creates new ProcessTaskArguments for the supplied class T (based on what DemandsInitialization fields it has).  Parent is the ProcessTask that hosts the class T e.g. IAttacher
        /// </summary>
        /// <typeparam name="T">A class that has some DemandsInitializations</typeparam>
        /// <param name="parent"></param>
        public static IArgument[] CreateArgumentsForClassIfNotExists<T>(ProcessTask parent)
        {
            var argFactory = new ArgumentFactory();
            return argFactory.CreateArgumentsForClassIfNotExistsGeneric<T>(
                
                //tell it how to create new instances of us related to parent
                parent,

                //what arguments already exist
                parent.GetAllArguments().ToArray())

                //convert the result back from generic to specific (us)
                .ToArray();
        }
    }
}
