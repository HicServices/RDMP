using Rdmp.Core.Curation.Data;
using Rdmp.Core.EntityFramework;
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
        public IProject Project => CatalogueDbContext.GetObjectByID<Project>(_projectID);

        [NoMappingToDatabase]
        public ExtractableDataSet DataSet => CatalogueDbContext.GetObjectByID<ExtractableDataSet>(_extractableDataSetID);
        #endregion

        public ExtractableDataSetProject() { }

        public ExtractableDataSetProject(RDMPDbContext catalogueDbContext, ExtractableDataSet eds, IProject project)
        {
            CatalogueDbContext = catalogueDbContext;
            //    CatalogueDbContext.InsertAndHydrate(this, new Dictionary<string, object>
            //{
            //    { "Project_ID", project.ID},
            //    { "ExtractableDataSet_ID", eds.ID }
            //});
        }

        internal ExtractableDataSetProject(RDMPDbContext catalogueDbContext, DbDataReader r) : base(catalogueDbContext, r)
        {
            Project_ID = int.Parse(r["Project_ID"].ToString());
            ExtractableDataSet_ID = int.Parse(r["ExtractableDataSet_ID"].ToString());
        }
    }
}
