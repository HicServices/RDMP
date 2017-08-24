using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using DataLoadEngine.DatabaseManagement.EntityNaming;

namespace LoadModules.Generic.Mutilators.QueryBuilders
{
    public class CascadeSqlBuilder
    {
        private readonly StringBuilder _result = new StringBuilder();
        private readonly LoadStage _loadStage;
        private readonly HICDatabaseConfiguration _databaseConfiguration;
        private readonly CatalogueRepository _repository;

        public CascadeSqlBuilder(LoadStage loadStage, HICDatabaseConfiguration databaseConfiguration, CatalogueRepository repository)
        {
            
            _loadStage = loadStage;
            _databaseConfiguration = databaseConfiguration;
            _repository = repository;
        }

        public string GetSql(TableInfo primaryTableInfo, params TableInfo[] candidatesToJoinAgainst)
        {
            var allCandidateColumnInfos = candidatesToJoinAgainst.Except(new[] { primaryTableInfo }).SelectMany(t => t.ColumnInfos).ToArray();

            // partition candidate column infos into tables
            var columnInfosGroupedByTable = allCandidateColumnInfos.GroupBy(info => info.TableInfo_ID);
            var parentColumnInfos = primaryTableInfo.ColumnInfos.ToArray();

            // gather the JoinInfos for each set of column infos into one big ol' collection
            var nextLevelJoins = columnInfosGroupedByTable.SelectMany(group => _repository.JoinInfoFinder.GetAllJoinInfosBetweenColumnInfoSets(parentColumnInfos, group.ToArray()));
            //var nextLevelJoins = JoinInfo.GetAllJoinInfosBetweenColumnInfoSets(parentColumnInfos, allCandidateColumnInfos);

            var comboJoinsDict = new Dictionary<int, List<JoinInfo>>();
            foreach (var join in nextLevelJoins)
            {
                if (!comboJoinsDict.ContainsKey(join.ForeignKey.TableInfo_ID))
                    comboJoinsDict.Add(join.ForeignKey.TableInfo_ID, new List<JoinInfo>());

                comboJoinsDict[join.ForeignKey.TableInfo_ID].Add(join);
            }

            foreach (var kvp in comboJoinsDict)
            {
                if (kvp.Key == primaryTableInfo.ID)
                    continue;

                CreateCascadeDeleteSql(kvp.Value);
                GetSql(_repository.GetObjectByID<TableInfo>(kvp.Key), candidatesToJoinAgainst);
            }

            return _result.ToString();
        }

        private void CreateCascadeDeleteSql(List<JoinInfo> joinInfos)
        {
            var pkTableInfo = _repository.GetObjectByID<TableInfo>(joinInfos.Select(j => j.PrimaryKey.TableInfo_ID).Distinct().Single());
            var fkTableInfo = _repository.GetObjectByID<TableInfo>(joinInfos.Select(j => j.ForeignKey.TableInfo_ID).Distinct().Single());

            var pkTableName = pkTableInfo.GetRuntimeName(_loadStage, _databaseConfiguration.DatabaseNamer);
            var fkTableName = fkTableInfo.GetRuntimeName(_loadStage, _databaseConfiguration.DatabaseNamer);

            _result.AppendLine(String.Format(
                @"ALTER TABLE {0}
DROP CONSTRAINT {1}

ALTER TABLE {0} 
ADD CONSTRAINT {1} FOREIGN KEY({2})
REFERENCES {3} ({4})
ON DELETE CASCADE",
                fkTableName,
                "FK_" + pkTableName + "_" + fkTableName,
                string.Join(",", joinInfos.Select(j => j.ForeignKey.GetRuntimeName(_loadStage))),
                pkTableName,
                string.Join(",", joinInfos.Select(j => j.PrimaryKey.GetRuntimeName(_loadStage)))
                ));
        }
    }
}