using System;
using System.Collections.Generic;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Interfaces.Pipeline;
using DataExportLibrary.Data.DataTables;

namespace DataExportLibrary.CohortCreationPipeline.Destinations.IdentifierAllocation
{
    /// <summary>
    /// Allocates a Guid for each private identifier supplied.  This is similar to <see cref="GuidReleaseIdentifierAllocator"/> except that it will preserve previous
    /// allocations within the <see cref="Project"/>.  For example if you commit a cohort 'Cases' with private id '123' to project '10' then might get a guid 'abc...',
    /// if you then submit verison 2 of the cohort you will get the same guid ('abc...') for persion '123'.  
    /// 
    /// <para>Guids are always different between <see cref="Project"/> for example person 'abc' in project '10' will have a different Guid release identifier than if he
    /// was committed to project '11' and it would be impossible to link the two release identifiers</para>
    /// 
    /// <para>Projects are differentiated by <see cref="Project.ProjectNumber"/> since this is what is stored in your cohort database</para>
    /// </summary>
    public class ProjectConsistentGuidReleaseIdentifierAllocator : IAllocateReleaseIdentifiers
    {
        private int _projectNumber;
        private ICohortCreationRequest _request;
        private Dictionary<object, object> _releaseMap;

        public object AllocateReleaseIdentifier(object privateIdentifier)
        {
            //figure out all the historical release ids for private ids in the Project
            if (_releaseMap == null)
                _releaseMap = GetReleaseMap();

            //if we have a historical release Id use it
            if (_releaseMap.ContainsKey(privateIdentifier))
                return _releaseMap[privateIdentifier];
            
            //otherwise allocate a new guid and lets record it just for prosperity
            var toReturn = Guid.NewGuid().ToString();
            _releaseMap.Add(privateIdentifier,toReturn);

            return toReturn;
        }

        /// <summary>
        /// Figures out all the previously allocated release identifiers for private identifiers for cohorts assigned to the projectNumber
        /// </summary>
        /// <returns></returns>
        private Dictionary<object, object> GetReleaseMap()
        {
            var toReturn = new Dictionary<object, object>();

            var cohortDatabase = _request.NewCohortDefinition.LocationOfCohort.Discover();

            var ect = _request.NewCohortDefinition.LocationOfCohort;

            using (var con = cohortDatabase.Server.GetConnection())
            {
                string sql =
                    string.Format("SELECT DISTINCT {0},{1} FROM {2} JOIN {3} ON {2}.{4}={3}.{5} WHERE {3}.{6}={7}"
                        , ect.PrivateIdentifierField,
                        ect.ReleaseIdentifierField,
                        ect.TableName,
                        ect.DefinitionTableName,
                        ect.DefinitionTableForeignKeyField,
                        "id",
                        "projectNumber",
                        _projectNumber);

                var syntaxHelper = ect.GetQuerySyntaxHelper();

                var priv = syntaxHelper.GetRuntimeName(ect.PrivateIdentifierField);
                var rel = syntaxHelper.GetRuntimeName(ect.ReleaseIdentifierField);

                con.Open();

                using(var r = cohortDatabase.Server.GetCommand(sql, con).ExecuteReader())
                    while (r.Read())
                    {
                        if(toReturn.ContainsKey(r[priv]))
                            throw new Exception("Private identifier '" + r[priv] + "' has more than 1 historical release identifier (" + string.Join(",",toReturn[r[priv]],r[rel]));

                        toReturn.Add(r[priv],r[rel]);
                    }

            }

            return toReturn;
        }

        public void Initialize(ICohortCreationRequest request)
        {
            if(!request.Project.ProjectNumber.HasValue)
                throw new Exception("Project " + request.Project + " must have a ProjectNumber");

            _request = request;
            _projectNumber = request.Project.ProjectNumber.Value;
        }
    }
}