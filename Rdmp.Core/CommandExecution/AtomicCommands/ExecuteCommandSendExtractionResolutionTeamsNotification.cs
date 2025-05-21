using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Rdmp.Core.DataExport.Data;
using System;
using System.Globalization;
using System.Net.Http;
using System.Text;
using Rdmp.Core.ReusableLibraryCode.Settings;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    class ExecuteCommandSendExtractionResolutionTeamsNotification: BasicCommandExecution,IAtomicCommand
    {

        private readonly ExtractionConfiguration _ec;
        private readonly bool _success;
        private static readonly HttpClient client = new HttpClient();
        private string url = UserSettings.ExtractionWebhookUrl;
        private readonly string template = "{  \"type\":\"AdaptiveCard\",  \"attachments\":[      {        \"contentType\":\"application/vnd.microsoft.card.adaptive\",        \"contentUrl\":null,        \"content\":{  \"type\": \"AdaptiveCard\",    \"$schema\": \"https://adaptivecards.io/schemas/adaptive-card.json\",    \"version\": \"1.5\",    \"body\": [        {            \"type\": \"Table\",            \"columns\": [                {                    \"width\": 1                },                {                    \"width\": 7                }            ],            \"rows\": [                {                    \"type\": \"TableRow\",                    \"cells\": [                        {                            \"type\": \"TableCell\",                            \"items\": [                                {                                    \"type\": \"Image\",                                    \"url\": \"\",                                    \"size\": \"Small\",                                    \"width\": \"40px\"                                }                            ]                        },                        {                            \"type\": \"TableCell\",                            \"verticalContentAlignment\": \"Center\",                            \"items\": [                                {                                    \"type\": \"TextBlock\",                                    \"text\": \"\",                                    \"wrap\": true,                                    \"maxLines\": 3                                }                            ],                            \"targetWidth\": \"Wide\",                            \"bleed\": true                        },                        {                            \"type\": \"TableCell\",                            \"isVisible\": false                        }                    ]                }            ]        },        {            \"type\": \"TextBlock\",            \"text\": \"<at></at>\"        }    ],    \"msteams\": {                \"entities\": [                    {                    \"type\": \"mention\",                    \"text\": \"<at></at>\",                    \"mentioned\": {                        \"id\": \"\",                        \"name\": \"\"                    }                    }                ]            }        }       }  ]}";
        public ExecuteCommandSendExtractionResolutionTeamsNotification(IBasicActivateItems activator, ExtractionConfiguration ec, bool success) {
            _ec = ec;
            _success = success;
        }

        public override void Execute()
        {
            base.Execute();
            try
            {
                var c = GetContent();
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            var content = new StringContent(GetContent(), Encoding.UTF8, "application/json");
            client.PostAsync(url, content);

        }

        private string GetContent()
        {
            var badIcon = "https://upload.wikimedia.org/wikipedia/commons/thumb/c/cc/Cross_red_circle.svg/1200px-Cross_red_circle.svg.png";
            var goodIcon = "https://upload.wikimedia.org/wikipedia/commons/thumb/3/3b/Eo_circle_green_checkmark.svg/2048px-Eo_circle_green_checkmark.svg.png";
            var icon = _success ? goodIcon : badIcon;
            var subText = _success ? "completed successfully" : "failed";
            var email = UserSettings.ExtractionWebhookUsername;
            var mention = $"<at>{email}</at>";
            var adatpiveCard = AdaptiveCard.FromJson(template);
            adatpiveCard.Attachments[0].Content.Body[0].Rows[0].Cells[0].Items[0].Url = icon;
            adatpiveCard.Attachments[0].Content.Body[0].Rows[0].Cells[1].Items[0].Text = $"Extraction {_ec.Name}: {subText}.";
            adatpiveCard.Attachments[0].Content.Body[1].Text =mention;
            adatpiveCard.Attachments[0].Content.Msteams.Entities[0].Text = mention;
            adatpiveCard.Attachments[0].Content.Msteams.Entities[0].Mentioned.Id = email;
            adatpiveCard.Attachments[0].Content.Msteams.Entities[0].Mentioned.Name =email;


            return Serialize.ToJson(adatpiveCard);
        }
    }
}

partial class AdaptiveCard
{
    [JsonProperty("type")]
    public string Type { get; set; } 

    [JsonProperty("attachments")]
    public Attachment[] Attachments { get; set; }
}

partial class Attachment
{
    [JsonProperty("contentType")]
    public string ContentType { get; set; }

    [JsonProperty("contentUrl")]
    public object ContentUrl { get; set; }

    [JsonProperty("content")]
    public Content Content { get; set; }
}

partial class Content
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("$schema")]
    public Uri Schema { get; set; }

    [JsonProperty("version")]
    public string Version { get; set; }

    [JsonProperty("body")]
    public Body[] Body { get; set; }

    [JsonProperty("msteams")]
    public Msteams Msteams { get; set; }
}

partial class Body
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("columns", NullValueHandling = NullValueHandling.Ignore)]
    public Column[] Columns { get; set; }

    [JsonProperty("rows", NullValueHandling = NullValueHandling.Ignore)]
    public Row[] Rows { get; set; }

    [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
    public string Text { get; set; }
}

partial class Column
{
    [JsonProperty("width")]
    public long Width { get; set; }
}

partial class Row
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("cells")]
    public Cell[] Cells { get; set; }
}

partial class Cell
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
    public Item[] Items { get; set; }

    [JsonProperty("verticalContentAlignment", NullValueHandling = NullValueHandling.Ignore)]
    public string VerticalContentAlignment { get; set; }

    [JsonProperty("targetWidth", NullValueHandling = NullValueHandling.Ignore)]
    public string TargetWidth { get; set; }

    [JsonProperty("bleed", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Bleed { get; set; }

    [JsonProperty("isVisible", NullValueHandling = NullValueHandling.Ignore)]
    public bool? IsVisible { get; set; }
}

partial class Item
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
    public string Url { get; set; }

    [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
    public string Size { get; set; }

    [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
    public string Width { get; set; }

    [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
    public string Text { get; set; }

    [JsonProperty("wrap", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Wrap { get; set; }

    [JsonProperty("maxLines", NullValueHandling = NullValueHandling.Ignore)]
    public long? MaxLines { get; set; }
}

partial class Msteams
{
    [JsonProperty("entities")]
    public Entity[] Entities { get; set; }
}

partial class Entity
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("text")]
    public string Text { get; set; }

    [JsonProperty("mentioned")]
    public Mentioned Mentioned { get; set; }
}

partial class Mentioned
{
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
}

partial class AdaptiveCard
{
    public static AdaptiveCard FromJson(string json) => JsonConvert.DeserializeObject<AdaptiveCard>(json, Converter.Settings);
}

static class Serialize
{
    public static string ToJson(this AdaptiveCard self) => JsonConvert.SerializeObject(self, Converter.Settings);
}

internal static class Converter
{
    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    {
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        DateParseHandling = DateParseHandling.None,
        Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
    };
}