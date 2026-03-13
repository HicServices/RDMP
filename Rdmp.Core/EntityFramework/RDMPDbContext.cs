using MathNet.Numerics.RootFinding;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Linq;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.DataExtraction.Pipeline;
using Rdmp.Core.DataExport.DataRelease.Pipeline;
using Rdmp.Core.DataLoad.Engine.Pipeline;
using Rdmp.Core.EntityFramework.Models;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Providers.Nodes.CohortNodes;
using Rdmp.Core.Providers.Nodes.PipelineNodes;
using Rdmp.Core.Providers.Nodes.SharingNodes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rdmp.Core.Curation.Data.Catalogue;

namespace Rdmp.Core.EntityFramework
{
    public class RDMPDbContext : DbContext
    {

        public RDMPDbContext(DbContextOptions<RDMPDbContext> options)
           : base(options)
        { }

        public DbSet<Models.Catalogue> Catalogues { get; set; }
        public DbSet<Models.DashboardControl> DashboardControls { get; set; }
        public DbSet<Models.Pipeline> Pipelines { get; set; }
        public DbSet<Models.ConnectionStringKeyword> ConnectionStringKeywords { get; set; }
        public DbSet<Models.CatalogueItem> CatalogueItems { get; set; }
        public DbSet<Models.ColumnInfo> ColumnInfos { get; set; }
        public DbSet<Models.PipelineComponentArgument> PipelineComponentArguments { get; set; }

        public DbSet<Models.ExtractionInformation> ExtractionInformation { get; set; }

        public DbSet<Models.Dataset> Datasets { get; set; }
        public DbSet<Models.TableInfo> TableInfos { get; set; }
        public DbSet<Models.Lookup> Lookups { get; set; }
        public DbSet<Models.PipelineComponent> PipelineComponents { get; set; }
        public DbSet<Models.LookupCompositeJoinInfo> LookupCompositeJoinInfos { get; set; }
        public DbSet<ANOTable> ANOTables { get; set; }
        public DbSet<Models.StandardRegex> StandardRegexes { get; set; }
        public DbSet<RemoteRDMP> RemoteRDMPs { get; set; }
        public DbSet<Models.CohortIdentificationConfiguration> CohortIdentificationConfigurations { get; set; }
        public DbSet<Models.DashboardLayout> DashboardLayouts { get; set; }
        public DbSet<Models.DataAccessCredentials> DataAccessCredentials { get; set; }
        public DbSet<Models.ExternalDatabaseServer> ExternalDatabaseServers { get; set; }

        public T[] GetAllObjects<T>()
        {
            return null;//todo
        }

        public IEnumerable<IMapsDirectlyToDatabaseTable> GetAllObjects(Type t)
        {
            return null;//todo
        }

        public T GetObjectByID<T>(int id)
        {
            throw new NotImplementedException();
        }
        public T[] GetAllObjectsWithParent<T>(object o) { return null; }

        public T[] GetReferencesTo<T>(object o) { return null; }//todo

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.CatalogueItem>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
                entity.HasIndex(e => e.Name);
            });
            modelBuilder.Entity<Models.ColumnInfo>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
                entity.HasIndex(e => e.Name);
            });
            modelBuilder.Entity<Models.ExtractionInformation>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.SelectSQL).IsRequired();
            });

            modelBuilder.Entity<Models.Dataset>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).IsRequired();
            });
            modelBuilder.Entity<Models.TableInfo>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).IsRequired();
            });
            
            modelBuilder.Entity<Models.CohortIdentificationConfiguration>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
            });
            modelBuilder.Entity<Models.DashboardLayout>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasMany(e => e.Controls).WithOne(e => e.ParentLayout).HasForeignKey(e => e.DashboardLayout_ID);
            });
            modelBuilder.Entity<Models.DashboardControl>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasOne(e => e.ParentLayout).WithMany(e => e.Controls).HasForeignKey(e => e.DashboardLayout_ID);
            });
            modelBuilder.Entity<Models.RemoteRDMP>(entity =>
            {
                entity.HasKey(e => e.ID);
            });
            modelBuilder.Entity<Models.ConnectionStringKeyword>(entity =>
            {
                entity.HasKey(e => e.ID);
            });
            modelBuilder.Entity<Models.StandardRegex>(entity =>
            {
                entity.HasKey(e => e.ID);
            });
            modelBuilder.Entity<Models.DataAccessCredentials>(entity =>
            {
                entity.HasKey(e => e.ID);
            });
            modelBuilder.Entity<Models.Pipeline>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasMany(e => e.PipelineComponents).WithOne(e => e.Pipeline);
                entity.HasOne(e => e.Source);
                entity.HasOne(e => e.Destination);
            });
            modelBuilder.Entity<Models.PipelineComponent>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasMany(e => e.Arguments).WithOne();
                entity.HasOne(e => e.Pipeline).WithMany(e => e.PipelineComponents).HasForeignKey(e => e.Pipeline_ID);
                //entity.HasMany(e => e.Arguments).WithOne(e => e.PipelineComponent).HasForeignKey(e => e.PipelineComponent_ID);
            });
            modelBuilder.Entity<Models.PipelineComponentArgument>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasOne(e => e.PipelineComponent).WithMany(e => e.Arguments).HasForeignKey(e => e.PipelineComponent_ID);
            });
            modelBuilder.Entity<Models.Catalogue>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Acronym).HasMaxLength(100);
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Acronym);
                entity.HasIndex(e => e.IsDeprecated);
            });
        }

        public Curation.Data.Cohort.CohortAggregateContainer GetParent(AggregateConfiguration aggregateConfiguration)
        {
            throw new NotImplementedException();
        }

        public bool StillExists<T>(int allegedParent)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetAllObjectsWhere<T>(string v, int iD)
        {
            throw new NotImplementedException();
        }

        public IExternalDatabaseServer GetDefaultFor(PermissableDefaults liveLoggingServer_ID)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetAllObjectsInIDList<T>(List<int> list)
        {
            throw new NotImplementedException();
        }

        public T1[] GetAllObjectsWithParent<T1, T2>(T2 catalogue)
        {
            throw new NotImplementedException();
        }

        public CatalogueExtractabilityStatus GetExtractabilityStatus(Curation.Data.Catalogue catalogue)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AnyTableSqlParameter> GetAllParametersForParentTable(IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            throw new NotImplementedException();
        }

        public IMapsDirectlyToDatabaseTable GetObjectByID(Type type, int referencedObjectID)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IMapsDirectlyToDatabaseTable> GetAllObjectsInIDList(Type elementType, int[] ids)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ExtendedProperty> GetExtendedProperties(string replacedBy)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ExtendedProperty> GetExtendedProperties(string propertyName, IMapsDirectlyToDatabaseTable obj)
        {
            throw new NotImplementedException();
        }

        public bool SupportsObjectType(Type requiredType)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> SelectAllWhere<T>(string v1, string v2)
        {
            throw new NotImplementedException();
        }

        public DescendancyList GetDescendancyListIfAnyFor(object modelObject)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> GetChildren(object obj)
        {
            if (obj is FolderNode<Models.Catalogue> fnc)
            {
                return fnc.ChildFolders.Cast<object>().Union(fnc.ChildObjects);//Catalogues.Where(c => c.Folder == fnc.FullName);
            }
            if (obj is Models.Catalogue catalogue)
            {
                var catalogueItems = CatalogueItems.Where(ci => ci.Catalogue.ID == catalogue.ID).ToList();
                List<object> children = new();
                children.Add(new CatalogueItemsNode(catalogue, catalogueItems, ExtractionCategory.Core));
                children.Add(new CatalogueItemsNode(catalogue, catalogueItems, ExtractionCategory.Supplemental));
                children.Add(new CatalogueItemsNode(catalogue, catalogueItems, ExtractionCategory.SpecialApprovalRequired));
                children.Add(new CatalogueItemsNode(catalogue, catalogueItems, ExtractionCategory.Internal));
                children.Add(new CatalogueItemsNode(catalogue, catalogueItems, ExtractionCategory.Deprecated));
                children.Add(new CatalogueItemsNode(catalogue, catalogueItems, null));
                return children;
            }
            if (obj is CatalogueItemsNode cin)
            {
                return cin.CatalogueItems;
            }
            if (obj is Models.CatalogueItem catalogueItem)
            {
                var extractionInformations = ExtractionInformation.Where(ei => ei.CatalogueItem_ID == catalogueItem.ID).ToList();
                return [.. ColumnInfos.Where(ci => ci.CatalogueItems.Select(ci => ci.ID).Contains(catalogueItem.ID)), .. extractionInformations];
            }
            if (obj is FolderNode<Core.EntityFramework.Models.CohortIdentificationConfiguration> fcic)
            {
                return fcic.ChildFolders.Cast<object>().Union(fcic.ChildObjects);//Catalogues.Where(c => c.Folder == fnc.FullName);
            }
            if (obj is AllTemplateCohortIdentificationConfigurationsNode atcicn)
            {
                return CohortIdentificationConfigurations.Where(cic => cic.IsTemplate).ToList();
            }
            if (obj is AllOrphanAggregateConfigurationsNode aocicn)
            {
                return new List<string>() { }; ;// CohortIdentificationConfigurations.Where(cic => cic.IsTemplate).ToList();
            }
            if (obj is AllTemplateAggregateConfigurationsNode atacicn)
            {
                return new List<string>() { }; ;// CohortIdentificationConfigurations.Where(cic => cic.IsTemplate).ToList();
            }
            if (obj is AllDashboardsNode adn)
            {
                return DashboardLayouts.ToList();
            }
            if (obj is AllPluginsNode aplgn)
            {
                //TODO
            }
            if (obj is AllConnectionStringKeywordsNode acskn)
            {
                return ConnectionStringKeywords.ToList();
            }
            if (obj is AllRDMPRemotesNode arrn)
            {
                return RemoteRDMPs.ToList();
            }
            if (obj is AllANOTablesNode aanotn)
            {
                return ANOTables.ToList();
            }
            if (obj is AllObjectSharingNode aosn) { }
            if (obj is AllPipelinesNode apn)
            {
                return new List<StandardPipelineUseCaseNode>() {
                    new StandardPipelineUseCaseNode("Aggregate Committing", CreateTableFromAggregateUseCase.DesignTime(null), null),

                    new StandardPipelineUseCaseNode("File Import",UploadFileUseCase.DesignTime(),null),
                    new StandardPipelineUseCaseNode("Extraction",ExtractionPipelineUseCase.DesignTime(),null),
                    new StandardPipelineUseCaseNode("Release",ReleaseUseCase.DesignTime(),null),
                    new StandardPipelineUseCaseNode("Cohort Creation",CohortCreationRequest.DesignTime(),null),
                };
            }
            if (obj is StandardPipelineUseCaseNode splucn)
            {
                return splucn.GetCompatiblePipelines(Pipelines.ToList()).Select(pipeline => new PipelineCompatibleWithUseCaseNode(null, pipeline, splucn.UseCase)).ToList();
            }
            if (obj is PipelineCompatibleWithUseCaseNode pcwucn)
            {
                return pcwucn.Pipeline.PipelineComponents.ToList();
            }
            if (obj is Models.PipelineComponent pc)
            {
                return PipelineComponentArguments.ToList().Where(PipelineComponentArgument => PipelineComponentArgument.PipelineComponent_ID == pc.ID).ToList();//pc.Arguments.ToList();
            }
            if (obj is AllExternalServersNode aesn)
            {
                return ExternalDatabaseServers.ToList();
            }
            if (obj is AllDataAccessCredentialsNode adacn)
            {
                return [.. DataAccessCredentials.ToList(), new DecryptionPrivateKeyNode(true)];
            }
            if (obj is AllServersNode asn)
            {
                //todo
            }
            if (obj is AllStandardRegexesNode asrn)
            {
                return StandardRegexes.ToList();
            }
            return new List<string>() { };
        }
    }

}

