using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data.Datasets;

public interface IDatasetProviderConfiguration
{
    public string Type { get; }
    public string Name { get; }
    public string Url { get; }

    public int DataAccessCredentials_ID { get; }

    public string Organisation_ID { get; }


}
