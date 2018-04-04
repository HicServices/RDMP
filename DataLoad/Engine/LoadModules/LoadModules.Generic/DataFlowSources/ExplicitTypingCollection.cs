using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CatalogueLibrary.Data.DataLoad;

namespace LoadModules.Generic.DataFlowSources
{
    /// <summary>
    /// A collection of column names with explicitly defined column types that the user wants to force where present.  e.g. they loading a CSV and they get values 
    /// "291","195" but they know that some codes are like "012" and wish to preserve this leading 0s so they can explicitly define the column as being a string.
    /// 
    /// <para>This class can be used by [DemandsInitialization] properties and it will launch it's custom UI: ExplicitTypingCollectionUI</para>
    /// </summary>
    [Export(typeof(ICustomUIDrivenClass))]
    public class ExplicitTypingCollection:ICustomUIDrivenClass
    {
        /// <summary>
        /// A dictionary of names (e.g. column names) which must have specific C# data types
        /// </summary>
        public Dictionary<string, Type> ExplicitTypesCSharp = new Dictionary<string, Type>();
        
        public void RestoreStateFrom(string value)
        { 
            if(value == null)
                return;

            XmlDocument doc = new XmlDocument();
            doc.Load(new StringReader(value));


            //get the dictionary tag
            XmlNode typesNode = doc.GetElementsByTagName("ExplicitTypesCSharp").Cast<XmlNode>().Single();
            string dictionary = typesNode.InnerText;

            string[] lines = dictionary.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.Length; i += 2)
                ExplicitTypesCSharp.Add(lines[i], Type.GetType(lines[i + 1]));


        }

        public string SaveStateToString()
        {
           StringBuilder sb = new StringBuilder();

            //Add anything new here

            sb.AppendLine("<ExplicitTypesCSharp>");
            foreach (KeyValuePair<string, Type> kvp in ExplicitTypesCSharp)
            {
                sb.AppendLine(kvp.Key);
                sb.AppendLine(kvp.Value.FullName);
            }
            sb.AppendLine("</ExplicitTypesCSharp>");

            return sb.ToString();
        }
    }
}
