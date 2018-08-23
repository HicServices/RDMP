using System;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Pipelines;

namespace CatalogueLibrary.Nodes.PipelineNodes
{
    public class PipelineCompatibleWithUseCaseNode:IMasqueradeAs
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
