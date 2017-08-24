using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gigobyte.Mockaroo;
using Gigobyte.Mockaroo.Fields;
using Gigobyte.Mockaroo.Fields.Factory;

namespace Diagnostics.TestData
{

    
    public class MockarooTestDataFileGenerator
    {

        public void GenerateFile(string apiKey,int numberOfRecordsToFetch,int numberOfHeaders, FileInfo fileToSaveTo)
        {
            var schema = new Schema();
            List<IField> addedSoFar = new List<IField>();

            for (int i = 0; i < numberOfHeaders; i++)
            {
                var f = GetRandomField();
                
                if (f != null)
                {
                    f.Name = f.GetType().Name;

                    //if we have added one of this type already add a number on the end
                    while (addedSoFar.Any(d => d.Name.Equals(f.Name)))
                        f.Name = f.GetType().Name + new Random().Next(1000);

                    addedSoFar.Add(f);
                    schema.Add(f);
                }
            }

            using(var s = fileToSaveTo.OpenWrite())
            {
                byte[] bytes = null;

                using (var client = new HttpClient())
                {
                    string body = schema.ToJson();

                    var endpoint = Mockaroo.Endpoint(apiKey, numberOfRecordsToFetch, Format.CSV);
                    var response =  client.PostAsync(endpoint, new StringContent(body, Encoding.UTF8, "application/json")).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        bytes = response.Content.ReadAsByteArrayAsync().Result;
                    }
                    else
                    {
                        throw new Exception("Web response said:"+response.StatusCode + " - " + response.Content.ReadAsStringAsync().Result);
                    }
                }
                
                
                s.Write(bytes,0,bytes.Length);

                s.Flush();
                s.Close();
            }

        }

        private IField GetRandomField()
        {
            var r = new Random().Next(101);
            switch (r)
            {
                case 0:return new AppBundleIDField();
                case 1: return new AppNameField();
                case 2: return new AppVersionField();
                case 3: return new AvatarField();
                case 4: return new Base64ImageURLField();
                case 5: return new BitcoinAddressField();
                case 6: return new BlankField();
                case 7: return new BooleanField();
                case 8: return new CityField();
                case 9: return new ColorField();
                case 10: return new CompanyNameField();
                case 11: return new CountryCodeField();
                case 12: return new CountryField();
                case 13: return new CreditCardNumberField();
                case 14: return new CreditCardTypeField();
                case 15: return new CurrencyCodeField();
                case 16: return new CurrencyField();
                case 17: return new CustomListField();
                case 18: return new DatasetColumnField();
                case 19: return new DateField();
                case 20: return new DomainNameField();
                case 21: return new DrugCompanyField();
                case 22: return new DrugNameBrandField();
                case 23: return new DrugNameGenericField();
                case 24: return new DummyImageURLField();
                case 25: return new EmailAddressField();
                case 26: return new EncryptField();
                case 27: return new FDANDCCodeField();
                case 28: return new FamilyNameChineseField();
                case 29: return new FileNameField();
                case 30: return new FirstNameEuropeanField();
                case 31: return new FirstNameFemaleField();
                case 32: return new FirstNameField();
                case 33: return new FirstNameMaleField();
                case 34: return new FormulaField();
                case 35: return new FrequencyField();
                case 36: return new FullNameField();
                case 37: return new GUIDField();
                case 38: return new GenderAbbreviatedField();
                case 39: return new GenderField();
                case 40: return new GivenNameChineseField();
                case 41: return new HexColorField();
                case 42: return new IBANField();
                case 43: return new ICD9DiagnosisCodeField();
                case 44: return new ICD9DxDescLongField();
                case 45: return new ICD9DxDescShortField();
                case 46: return new ICD9ProcDescLongField();
                case 47: return new ICD9ProcDescShortField();
                case 48: return new ICD9ProcedureCodeField();
                case 49: return new IPAddressV4CIDRField();
                case 50: return new IPAddressV4Field();
                case 51: return new IPAddressV6CIDRField();
                case 52: return new IPAddressV6Field();
                case 53: return new ISBNField();
                case 54: return new JSONArrayField();
                case 55: return new JobTitleField();
                case 56: return new LanguageField();
                case 57: return new LastNameField();
                case 58: return new LatitudeField();
                case 59: return new LinkedInSkillField();
                case 60: return new LongitudeField();
                case 61: return new MACAddressField();
                case 62: return new MD5Field();
                case 63: return new MIMETypeField();
                case 64: return new MoneyField();
                case 65: return new MongoDBObjectIDField();
                case 66: return new NaughtyStringField();
                case 67: return new NormalDistributionField();
                case 68: return new NumberField();
                case 69: return new ParagraphsField();
                case 70: return new PasswordField();
                case 71: return new PhoneField();
                case 72: return new PoissonDistributionField();
                case 73: return new PostalCodeField();
                case 74: return new RaceField();
                case 75: return new RegularExpressionField();
                case 76: return new RowNumberField();
                case 77: return new SHA1Field();
                case 78: return new SHA256Field();
                case 79: return new SQLExpressionField();
                case 80: return new SSNField();
                case 81: return new ScenarioField();
                case 82: return new SentencesField();
                case 83: return new SequenceField();
                case 84: return new ShirtSizeField();
                case 85: return new ShortHexColorField();
                case 86: return new StateAbbreviatedField();
                case 87: return new StateField();
                case 88: return new StreetAddressField();
                case 89: return new StreetNameField();
                case 90: return new StreetNumberField();
                case 91: return new StreetSuffixField();
                case 92: return new SuffixField();
                case 93: return new TemplateField();
                case 94: return new TimeField();
                case 95: return new TimeZoneField();
                case 96: return new TitleField();
                case 97: return new TopLevelDomainField();
                case 98: return new URLField();
                case 99: return new UserAgentField();
                case 100: return new UserNameField();
                    
                default:
                    return null;
            }
            

        }
    }
}
