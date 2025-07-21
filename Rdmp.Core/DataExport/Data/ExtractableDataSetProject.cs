using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.DataExport.Data
{
    /// <summary>
    /// Used in a one-to-many relationship for the proejcts that are associated with a certain extractable dataset
    /// </summary>
    public class ExtractableDataSetProject : DatabaseEntity, IMapsDirectlyToDatabaseTable
    {
        #region Database Properties
        private int _projectID;
        private int _extractableDataSetID;

        public int Project_ID
        {
            get => _projectID;
            set => SetField(ref _projectID, value);
        }

        public int ExtractableDataSet_ID
        {
            get => _extractableDataSetID;
            set => SetField(ref _extractableDataSetID, value);
        }
        #endregion
        #region Relationships
        [NoMappingToDatabase]
        public IProject Project => Repository.GetObjectByID<Project>(_projectID);

        [NoMappingToDatabase]
        public ExtractableDataSet DataSet=> Repository.GetObjectByID<ExtractableDataSet>(_extractableDataSetID);
        #endregion

        public ExtractableDataSetProject() { }

        public ExtractableDataSetProject(IDataExportRepository repository, ExtractableDataSet eds, IProject project)
        {
            Repository = repository;
            Repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Project_ID", project.ID},
            { "ExtractableDataSet_ID", eds.ID }
        });
        }

        internal ExtractableDataSetProject(IDataExportRepository repository, DbDataReader r) : base(repository, r)
        {
            Project_ID = int.Parse(r["Project_ID"].ToString());
            ExtractableDataSet_ID = int.Parse(r["ExtractableDataSet_ID"].ToString());
        }
    }
}
