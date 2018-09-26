using System;
using System.ComponentModel;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.Spontaneous;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Nodes.PipelineNodes
{
    /// <summary>
    /// This class is a wrapper for a <see cref="Pipeline"/> that has been found to be compatible with a given <see cref="PipelineUseCase"/> (in terms of the source / 
    /// destination components and flow type etc).
    /// 
    /// <para>It is <see cref="SpontaneousObject"/> only so it appears under Ctrl+F window... not a pattern we want to repeat.</para>
    /// </summary>
    public class PipelineCompatibleWithUseCaseNode : SpontaneousObject,IMasqueradeAs
    {
        public Pipeline Pipeline { get; set; }
        public PipelineUseCase UseCase { get; set; }
        private Type _useCaseType;

        public PipelineCompatibleWithUseCaseNode(Pipeline pipeline, PipelineUseCase useCase)
        {
            Pipeline = pipeline;
            UseCase = useCase;
            _useCaseType = UseCase.GetType();
        }

        public object MasqueradingAs()
        {
            return Pipeline;
        }

        public override string ToString()
        {
            return Pipeline.Name;
        }

        #region Equality
        protected bool Equals(PipelineCompatibleWithUseCaseNode other)
        {
            return _useCaseType.Equals(other._useCaseType) && Pipeline.Equals(other.Pipeline);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PipelineCompatibleWithUseCaseNode) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_useCaseType.GetHashCode()*397) ^ Pipeline.GetHashCode();
            }
        }
        #endregion
    }
}
