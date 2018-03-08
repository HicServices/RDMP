using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataExportLibrary.Data.DataTables;

namespace DataExportManager.Collections.Nodes
{
    class FrozenExtractionConfigurationsNode
    {
        public Project Project { get; set; }

        public FrozenExtractionConfigurationsNode(Project project)
        {
            Project = project;
        }

        public override string ToString()
        {
            return "Frozen Extraction Configurations";
        }

        protected bool Equals(FrozenExtractionConfigurationsNode other)
        {
            return Equals(Project, other.Project);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FrozenExtractionConfigurationsNode) obj);
        }

        public override int GetHashCode()
        {
            return (Project != null ? Project.GetHashCode() : 0);
        }
    }
}
