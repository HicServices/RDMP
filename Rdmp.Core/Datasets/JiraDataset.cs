using Amazon.S3.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Datasets
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Attribute
    {
        public string workspaceId { get; set; }
        public string globalId { get; set; }
        public string id { get; set; }
        public ObjectTypeAttribute objectTypeAttribute { get; set; }
        public string objectTypeAttributeId { get; set; }
        public List<ObjectAttributeValue> objectAttributeValues { get; set; }
        public string objectId { get; set; }
    }
    public class DefaultType
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class ExtendedInfo
    {
        public bool openIssuesExists { get; set; }
        public bool attachmentsExists { get; set; }
    }

    public class Icon
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url16 { get; set; }
        public string url48 { get; set; }
    }

    public class Links
    {
        public string self { get; set; }
    }

    public class MediaClientConfig
    {
        public string clientId { get; set; }
        public string issuer { get; set; }
        public string mediaBaseUrl { get; set; }
        public string mediaJwtToken { get; set; }
        public string fileId { get; set; }
        public int tokenLifespanInMinutes { get; set; }
    }

    public class ObjectAttributeValue
    {
        public object value { get; set; }
        public object searchValue { get; set; }
        public bool referencedType { get; set; }
        public string displayValue { get; set; }
        public ReferencedObject referencedObject { get; set; }
    }

    public class ObjectType
    {
        public string workspaceId { get; set; }
        public string globalId { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public int type { get; set; }
        public string description { get; set; }
        public Icon icon { get; set; }
        public int position { get; set; }
        public DateTime created { get; set; }
        public DateTime updated { get; set; }
        public int objectCount { get; set; }
        public string objectSchemaId { get; set; }
        public bool inherited { get; set; }
        public bool abstractObjectType { get; set; }
        public bool parentObjectTypeInherited { get; set; }
    }

    public class ObjectTypeAttribute
    {
        public string workspaceId { get; set; }
        public string globalId { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public bool label { get; set; }
        public int type { get; set; }
        public DefaultType defaultType { get; set; }
        public bool editable { get; set; }
        public bool system { get; set; }
        public bool sortable { get; set; }
        public bool summable { get; set; }
        public bool indexed { get; set; }
        public int minimumCardinality { get; set; }
        public int maximumCardinality { get; set; }
        public bool removable { get; set; }
        public bool hidden { get; set; }
        public bool includeChildObjectTypes { get; set; }
        public bool uniqueAttribute { get; set; }
        public string options { get; set; }
        public int position { get; set; }
        public string description { get; set; }
        public ReferenceType referenceType { get; set; }
        public string referenceObjectTypeId { get; set; }
        public ReferenceObjectType referenceObjectType { get; set; }
        public string suffix { get; set; }
        public string regexValidation { get; set; }
        public string qlQuery { get; set; }
        public string iql { get; set; }
    }

    public class ReferencedObject
    {
        public string workspaceId { get; set; }
        public string globalId { get; set; }
        public string id { get; set; }
        public string label { get; set; }
        public string objectKey { get; set; }
        public ObjectType objectType { get; set; }
        public DateTime created { get; set; }
        public DateTime updated { get; set; }
        public bool hasAvatar { get; set; }
        public object timestamp { get; set; }
        public Links _links { get; set; }
        public string name { get; set; }
    }

    public class ReferenceObjectType
    {
        public string workspaceId { get; set; }
        public string globalId { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public int type { get; set; }
        public string description { get; set; }
        public Icon icon { get; set; }
        public int position { get; set; }
        public DateTime created { get; set; }
        public DateTime updated { get; set; }
        public int objectCount { get; set; }
        public string objectSchemaId { get; set; }
        public bool inherited { get; set; }
        public bool abstractObjectType { get; set; }
        public bool parentObjectTypeInherited { get; set; }
    }

    public class ReferenceType
    {
        public string workspaceId { get; set; }
        public string globalId { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string color { get; set; }
        public string url16 { get; set; }
        public bool removable { get; set; }
    }

    public class JiraDataset : PluginDataset
    {
        public string workspaceId { get; set; }
        public string globalId { get; set; }
        public string id { get; set; }
        public string label { get; set; }
        public string objectKey { get; set; }
        public ObjectType objectType { get; set; }
        public DateTime created { get; set; }
        public DateTime updated { get; set; }
        public List<Attribute> attributes { get; set; }
        public ExtendedInfo extendedInfo { get; set; }
        public Links _links { get; set; }
        public string name { get; set; }

    }

    public class JiraPutAttributeValueObject{
        public object value;
    }
    public class JiraPutAttribute
    {
        public string objectTypeAttributeId;
        public List<JiraPutAttributeValueObject> objectAttributeValues;

    }
}
