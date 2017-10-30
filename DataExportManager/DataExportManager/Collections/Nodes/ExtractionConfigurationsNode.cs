using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataExportLibrary.Data.DataTables;

namespace DataExportManager.Collections.Nodes
{
    public class ExtractionConfigurationsNode
    {
        public Project Project { get; set; }

        public ExtractionConfigurationsNode(Project project)
        {
            Project = project;
        }

        public override string ToString()
        {
            return "Extraction Configurations";
        }

        protected bool Equals(ExtractionConfigurationsNode other)
        {
            return Equals(Project, other.Project);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ExtractionConfigurationsNode) obj);
        }

        public override int GetHashCode()
        {
            return (Project != null ? Project.GetHashCode() : 0);
        }
    }
}
