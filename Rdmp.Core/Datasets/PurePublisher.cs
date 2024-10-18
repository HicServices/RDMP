using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Datasets;
// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class Description
{
    public string en_GB { get; set; }
}

public class PurePublisher
{
    public int pureId { get; set; }
    public string uuid { get; set; }
    public string createdBy { get; set; }
    public DateTime createdDate { get; set; }
    public string modifiedBy { get; set; }
    public DateTime modifiedDate { get; set; }
    public string version { get; set; }
    public string name { get; set; }
    public Type type { get; set; }
    public PublisherWorkflow workflow { get; set; }
    public string systemName { get; set; }
}

public class Term
{
    public string en_GB { get; set; }
}

public class Type
{
    public string uri { get; set; }
    public Term term { get; set; }
}

public class PublisherWorkflow
{
    public string step { get; set; }
    public Description description { get; set; }
}

