using System;
using System.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;

namespace CatalogueManager.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls
{
    public class ArgumentValueUIArgs
    {
        public IArgumentHost Parent { get; set; }

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

        public ArgumentValueUIArgs Clone()
        {
            var newInstance = new ArgumentValueUIArgs();
            newInstance.Parent = Parent;
            newInstance.InitialValue = InitialValue;
            newInstance.Type = Type;
            newInstance.Required = Required;
            newInstance.PreviewIfAny = PreviewIfAny;
            newInstance.CatalogueRepository = CatalogueRepository;
            newInstance.Setter = Setter;
            newInstance.Fatal = Fatal;
            
            return newInstance;
        }
    }
}
