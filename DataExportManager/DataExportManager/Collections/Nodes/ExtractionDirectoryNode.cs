using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Nodes.LoadMetadataNodes;
using DataExportLibrary.Data.DataTables;
using ReusableUIComponents;

namespace DataExportManager.Collections.Nodes
{
    public class ExtractionDirectoryNode : IDirectoryInfoNode, IOrderable
    {
        public Project Project { get; private set; }

        public ExtractionDirectoryNode(Project project)
        {
            Project = project;
        }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Project.ExtractionDirectory))
                return "???";

            return Project.ExtractionDirectory;
        }

        protected bool Equals(ExtractionDirectoryNode other)
        {
            return Equals(Project, other.Project);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ExtractionDirectoryNode) obj);
        }

        public override int GetHashCode()
        {
            return (Project != null ? Project.GetHashCode() : 0);
        }

        public DirectoryInfo GetDirectoryInfoIfAny()
        {
            if (string.IsNullOrWhiteSpace(Project.ExtractionDirectory))
                return null;

            return new DirectoryInfo(Project.ExtractionDirectory);
        }

        public int Order{ get { return 4; } set { }}
    }
}
