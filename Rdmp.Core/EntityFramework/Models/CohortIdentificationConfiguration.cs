using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("CohortIdentificationConfiguration")]

    public class CohortIdentificationConfiguration : DatabaseObject, IHasFolder, ICollectSqlParameters
    {

        public CohortIdentificationConfiguration() { }
        public CohortIdentificationConfiguration(RDMPDbContext catalogueDbContext, string text)
        {
        }

        [Key]
        public override int ID { get; set; }

        public int? Version { get; set; }

        public string Name { get; set => SetField(ref field, value); }
        #nullable enable
        public string? Description { get; set => SetField(ref field, value); }
        public string? Folder { get; set => SetField(ref field, value); }
        public bool Frozen { get; set => SetField(ref field, value); }
        public string? FrozenBy { get; set => SetField(ref field, value); }
        public DateTime? FrozenDate { get; set => SetField(ref field, value); }
        public bool IsTemplate { get; set => SetField(ref field, value); }
        public int? RootCohortAggregateContainer_ID { get; set => SetField(ref field, value); }

        [ForeignKey("RootCohortAggregateContainer_ID")]
        public virtual CohortAggregateContainer? RootCohortAggregateContainer { get; set; }

        public int? QueryCachingServer_ID { get; set => SetField(ref field, value); }

        [ForeignKey("QueryCachingServer_ID")]
        public virtual ExternalDatabaseServer? QueryCachingServer { get; set; }
        public int? ClonedFrom_ID { get; set; }
        public static string CICPrefix => "cic_";

#nullable disable

        public override string ToString()
        {
            return Name;
        }

        public List<CohortIdentificationConfiguration> GetVersions()
        {
            return new List<CohortIdentificationConfiguration>();//TODO
        }

        public List<ISqlParameter> GetAllParameters() => new List<ISqlParameter>();//TODO

        public string GetNamingConventionPrefixForConfigurations() => "CohortIdentificationConfiguration_";//TODO

        public CohortIdentificationConfiguration CreateClone(ThrowImmediatelyCheckNotifier quiet)
        {
            throw new NotImplementedException();
        }

        public List<JoinableCohortAggregateConfiguration> GetAllJoinables()
        {
            throw new NotImplementedException();
        }

        public void Freeze()
        {
            throw new NotImplementedException();
        }

        public void Unfreeze()
        {
            throw new NotImplementedException();
        }

        public bool ShouldBeReadOnly(string name, out string reason)
        {
            throw new NotImplementedException();
        }

        public AggregateConfiguration CreateNewEmptyConfigurationForCatalogue(Catalogue catalogue, CohortIdentificationConfiguration.ChooseWhichExtractionIdentifierToUseFromManyHandler chooseWhichExtractionIdentifierToUseFromManyHandler, bool importMandatoryFilters)
        {
            throw new NotImplementedException();
        }

        public void EnsureNamingConvention(AggregateConfiguration ac)
        {
            throw new NotImplementedException();
        }

        ISqlParameter[] ICollectSqlParameters.GetAllParameters()
        {
            throw new NotImplementedException();
        }

        public void CreateRootContainerIfNotExists()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Delegate for handling the situation in which the user wants to create a cohort based on a given Catalogue but there are multiple IsExtractionIdentifier columns.
        /// For example SMR02 (baby birth records) might have (Mother CHI, Father CHI, Baby CHI).  In this situation the decision on which column to use is resolved by this
        /// class.
        /// </summary>
        /// <param name="catalogue"></param>
        /// <param name="candidates"></param>
        /// <returns></returns>
        public delegate ExtractionInformation ChooseWhichExtractionIdentifierToUseFromManyHandler(ICatalogue catalogue,
            ExtractionInformation[] candidates);
    }
}
