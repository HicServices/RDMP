using Rdmp.Core.Curation.Data.Datasets.HDR.HDRDatasetItems;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data.Datasets.HDR
{
    public class HDRDatasetPost
    {
     
        public PostMetadata metadata = new PostMetadata();
        public HDRDatasetPost(Catalogue catalogue) {
            metadata.identifier = "";
            metadata.version = "1.0.0";
            metadata.revisions = new List<Revision>();
            metadata.modified = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            metadata.issued = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            metadata.summary.title=catalogue.Name;
            metadata.summary.@abstract = catalogue.ShortDescription.Length>4?catalogue.ShortDescription:"";
            metadata.summary.dataCustodian = new DataCustodian();
            metadata.summary.dataCustodian.identifier= "unknown";
            metadata.summary.dataCustodian.name = "name";
            metadata.summary.dataCustodian.contactPoint = "test@example.com";
            metadata.summary.populationSize = 0;
            metadata.summary.contactPoint = "";
            metadata.provenance.temporal = new Temporal();
            metadata.provenance.origin = new Origin();
            metadata.provenance.temporal.timeLag = "Variable";
            metadata.provenance.temporal.startDate = DateTime.UtcNow;
            metadata.provenance.temporal.publishingFrequency = "Irregular";
            metadata.provenance.origin.datasetSubType = new List<string>();
            metadata.provenance.origin.datasetType = new List<string>();
            metadata.accessibility.access = new Access();
            metadata.accessibility.access.accessRights = "";
        }
    }
}
