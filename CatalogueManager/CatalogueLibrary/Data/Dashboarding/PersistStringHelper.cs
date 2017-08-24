using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CatalogueLibrary.Data.Dashboarding
{
    /// <summary>
    /// Helps you create simple string based argument lists
    /// </summary>
    public class PersistStringHelper
    {
        public string SaveDictionaryToString(Dictionary<string, string> dict)
        {
            
            XElement el = new XElement("root",
                dict.Select(kv => new XElement(kv.Key, kv.Value)));

            return el.ToString();
        }

        public Dictionary<string, string> LoadDictionaryFromString(string str)
        {
            if(string.IsNullOrWhiteSpace(str))
                return new Dictionary<string, string>();

            XElement rootElement = XElement.Parse(str);
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var el in rootElement.Elements())
                dict.Add(el.Name.LocalName, el.Value);

            return dict;
        }

        public string GetValueIfExistsFromPersistString(string key, string persistString)
        {
            var dict = LoadDictionaryFromString(persistString);

            if(dict.ContainsKey(key))
                return dict[key];
            
            return null;
        }
    }
}
