using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NuDoq;

namespace ReusableLibraryCode.Comments
{
    /// <summary>
    /// Records documentation for classes and keywords (e.g. foreign key names).  This is pwered by NuDoq
    /// </summary>
    public class CommentStore : IEnumerable<KeyValuePair<string, string>>
    {
        private readonly Dictionary<string,string> _dictionary = new Dictionary<string, string>();

        public void ReadComments()
        {

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {

                var xmlFile = Path.GetFileNameWithoutExtension(assembly.GetName().Name) + ".xml";

                if(File.Exists(xmlFile))
                {
                    var doc = DocReader.Read(assembly,xmlFile);
                    doc.Accept(new CommentsVisitor(this));

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
    }
}