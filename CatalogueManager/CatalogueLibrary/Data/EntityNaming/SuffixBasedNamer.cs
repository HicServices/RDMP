using System;
using System.Collections.Generic;

namespace CatalogueLibrary.Data.EntityNaming
{
    public class SuffixBasedNamer : INameDatabasesAndTablesDuringLoads
    {
        protected static Dictionary<LoadBubble, string> Suffixes = new Dictionary<LoadBubble, string>
        {
            {LoadBubble.Raw, ""},
            {LoadBubble.Staging, "_STAGING"},
            {LoadBubble.Live, ""},
            {LoadBubble.Archive, "_Archive"}
        };
        
        public virtual string GetDatabaseName(string rootDatabaseName, LoadBubble stage)
        {
            switch (stage)
            {
                case LoadBubble.Raw:
                    return rootDatabaseName + "_RAW";
                case LoadBubble.Staging:
                    return rootDatabaseName + "_STAGING";
                case LoadBubble.Live:
                    return rootDatabaseName;
                default:
                    throw new ArgumentOutOfRangeException("stage");
            }
        }

        public virtual string GetName(string tableName, LoadBubble convention)
        {
            if (!Suffixes.ContainsKey(convention))
                throw new ArgumentException("Do not have a suffix for convention: " + convention);

            return tableName + Suffixes[convention];
        }

        public virtual bool IsNamedCorrectly(string tableName, LoadBubble convention)
        {
            if (!Suffixes.ContainsKey(convention))
                throw new ArgumentException("Do not have a suffix for convention: " + convention);

            return tableName.EndsWith(Suffixes[convention]);
        }

        public virtual string RetrieveTableName(string fullName, LoadBubble convention)
        {
            if (!Suffixes.ContainsKey(convention))
                throw new ArgumentException("Do not have a suffix for convention: " + convention);

            if (string.IsNullOrWhiteSpace(Suffixes[convention]))
                return fullName;

            if (!fullName.EndsWith(Suffixes[convention]))
                throw new ArgumentException("'" + fullName + "' is not named according to the " + convention + " convention");

            return fullName.Remove(fullName.Length - Suffixes[convention].Length);
        }

        public string ConvertTableName(string tableName, LoadBubble from, LoadBubble to)
        {
            return GetName(RetrieveTableName(tableName, from), to);
        }
    }
}