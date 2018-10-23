using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuDoq;

namespace ReusableLibraryCode.Comments
{
    /// <summary>
    /// Records documentation for classes and keywords (e.g. foreign key names).  This is pwered by NuDoq
    /// </summary>
    public class CommentStore : IEnumerable<KeyValuePair<string, string>>
    {
        private readonly Dictionary<string,string> _dictionary = new Dictionary<string, string>();

        private string[] _ignoreHelpFor = new[]
        {
            "CsvHelper.xml",
            "Google.Protobuf.xml",
            "MySql.Data.xml",
            "Newtonsoft.Json.xml",
            "NLog.xml",
            "NuDoq.xml",
            "ObjectListView.xml",
            "QuickGraph.xml",
            "Renci.SshNet.xml",
            "ScintillaNET.xml",
            "nunit.framework.xml"
        };
        
        public void ReadComments(params string[] directoriesToLookInForComments)
        {

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if(assembly.FullName.Contains("LoadModules"))
                    Console.WriteLine("Here");

                var xmlFile = assembly.GetName().Name + ".xml";

                //we don't need to provide help for system classes
                if (xmlFile.StartsWith("System") || _ignoreHelpFor.Contains(xmlFile))
                    continue;

                foreach (string d in directoriesToLookInForComments)
                {
                    var f = Path.Combine(d, xmlFile);
                    if (File.Exists(f))
                    {
                        var doc = DocReader.Read(assembly, f);
                        doc.Accept(new CommentsVisitor(this));

                    }
                }
                
            }
        }

        public void Add(string name, string summary)
        {
            //these are not helpful!
            if(name == "C" || name == "R")
                return;

            if(_dictionary.ContainsKey(name))
                return;

            _dictionary.Add(name,summary);

        }

        public bool ContainsKey(string keyword)
        {
            return _dictionary.ContainsKey(keyword);
        }

        /// <summary>
        /// Returns documentation for the keyword or null if no documentation exists
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string this[string index]    // Indexer declaration  
        {
            get
            {
                if (_dictionary.ContainsKey(index))
                    return _dictionary[index];

                return null;
            }
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns documentation for the class specified up to maxLength characters (after which ... is appended).  Returns null if no documentation exists for the class
        /// </summary>
        /// <param name="maxLength"></param>
        /// <param name="type"></param>
        /// <param name="allowInterfaceInstead">If no docs are found for Type X then look for IX too</param>
        /// <returns></returns>
        public string GetTypeDocumentationIfExists(int maxLength, Type type, bool allowInterfaceInstead = true)
        {
            var docs = this[type.Name];

            //if it's a generic try looking for an non generic or abstract base etc
            if (docs == null && type.Name.EndsWith("`1"))
                docs = this[type.Name.Substring(0, type.Name.Length - "`1".Length)];

            if (docs == null && allowInterfaceInstead && !type.IsInterface)
                docs = this["I" + type.Name];

            if (string.IsNullOrWhiteSpace(docs))
                return null;

            maxLength = Math.Max(10, maxLength - 3);

            if (docs.Length <= maxLength)
                return docs;

            return docs.Substring(0, maxLength) + "...";
        }
        /// <inheritdoc cref="GetTypeDocumentationIfExists(int,Type,bool)"/>
        public string GetTypeDocumentationIfExists(Type type, bool allowInterfaceInstead = true)
        {
            return GetTypeDocumentationIfExists(int.MaxValue, type, allowInterfaceInstead);
        }
    }
}