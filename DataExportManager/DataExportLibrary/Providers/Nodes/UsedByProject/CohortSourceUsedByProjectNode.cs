using System;
using System.Collections.Generic;
using CatalogueLibrary.Data;
using DataExportLibrary.Data.DataTables;
using MapsDirectlyToDatabaseTable;

namespace DataExportLibrary.Providers.Nodes.UsedByProject
{
    public class CohortSourceUsedByProjectNode : IObjectUsedByProjectNode
    {
        public Project Project { get; set; }
        public object ObjectBeingUsed { get { return Source; }}

        public ExternalCohortTable Source { get; set; }
        public List<ExtractableCohortUsedByProjectNode> CohortsUsedByProject { get; set; }

        public bool IsEmptyNode { get; private set; }

        public CohortSourceUsedByProjectNode(Project project,ExternalCohortTable source)
        {
            Project = project;
            Source = source;
            CohortsUsedByProject = new List<ExtractableCohortUsedByProjectNode>();
            IsEmptyNode = false;
        }

        /// <summary>
        /// Creates an empty unknown cohorts node with the text ???
        /// </summary>
        public CohortSourceUsedByProjectNode(Project project)
        {
            IsEmptyNode = true;
            CohortsUsedByProject = new List<ExtractableCohortUsedByProjectNode>();
            Project = project;
        }

        public override string ToString()
        {
            if (IsEmptyNode)
                return "???";
            
            return Source.Name;
        }

        protected bool Equals(CohortSourceUsedByProjectNode other)
        {
            return Equals(Project, other.Project) && Equals(Source, other.Source);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CohortSourceUsedByProjectNode) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Project != null ? Project.GetHashCode() : 0)*397) ^ (Source != null ? Source.GetHashCode() : 0);
            }
        }

        public object MasqueradingAs()
        {
            return ObjectBeingUsed;
        }
    }
}
