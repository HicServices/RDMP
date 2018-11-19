using System;
using System.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;

namespace CatalogueManager.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls
{
    public class ArgumentValueUIArgs
    {
        public object InitialValue { get; set; }
        public Type Type { get; set; }
        public RequiredPropertyInfo Required { get; set; }
        public DataTable PreviewIfAny { get; set; }
        public CatalogueRepository CatalogueRepository { get; set; }

        /// <summary>
        /// Call this when the value populated in the user interface is changed
        /// </summary>
        public Action<object> Setter { get; set; }

        /// <summary>
        /// Call this when the value populated in the user interface is illegal
        /// </summary>
        public Action<Exception> Fatal { get; set; }
    }
}
