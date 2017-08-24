using System;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax
{
    public class CustomLine
    {
        public string Text { get; set; }
        public QueryComponent LocationToInsert { get; set; }

        /// <summary>
        /// The object associated with this custom line (if any) for example a parameter declaration line might be associated with an ISqlParameter
        /// </summary>
        public object RelatedObject { get; private set; }
        
        /// <summary>
        /// The line of code that caused the CustomLine to be created, this can be a StackTrace passed into the constructor or calculated automatically by CustomLine 
        /// </summary>
        public string StackTrace { get; private set; }

        public CustomLine(string text, QueryComponent locationToInsert):this(text,locationToInsert,null,Environment.StackTrace)
        {
        }

        public override string ToString()
        {
            return Text;
        }

        public CustomLine(string text, QueryComponent locationToInsert, object relatedObject, string environmentDotStackTrace)
        {
            Text = string.IsNullOrWhiteSpace(text) ? text : text.Trim();
            LocationToInsert = locationToInsert;
            RelatedObject = relatedObject;
            StackTrace = environmentDotStackTrace;
        }
    }
}