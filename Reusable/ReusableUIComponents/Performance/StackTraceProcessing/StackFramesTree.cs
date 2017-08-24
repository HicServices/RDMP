using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ReusableLibraryCode.Performance;
using ScintillaNET;

namespace ReusableUIComponents.Performance.StackTraceProcessing
{
    class StackFramesTree
    {
        public string CurrentFrame { get; private set; }
        public int QueryCount{ get; private set; }
        
        public bool HasSourceCode { get; private set; }

        public string Method { get; private set; }
        public string Filename { get; private set; }
        public int LineNumber { get; private set; }

        public bool IsInDatabaseAccessAssembly { get;private set; }

        public Dictionary<string,StackFramesTree> Children = new Dictionary<string, StackFramesTree>();

        public StackFramesTree(string[] stackFrameAndSubframes,QueryPerformed performed, bool isInDatabaseAccessAssemblyYet)
        {
            QueryCount = 0;
            
            PopulateSourceCode(stackFrameAndSubframes[0]);

            CurrentFrame = stackFrameAndSubframes[0];
            AddSubframes(stackFrameAndSubframes,performed);
            
            IsInDatabaseAccessAssembly = isInDatabaseAccessAssemblyYet || CurrentFrame.Contains("MapsDirectlyToDatabaseTable") || CurrentFrame.Contains("DatabaseCommandHelper");
        }

        private bool PopulateSourceCode(string frame)
        {
            string filenameMatch;
            int lineNumberMatch;
            string method;

            HasSourceCode = ExceptionViewerStackTraceWithHyperlinks.MatchStackLine(frame, out filenameMatch, out lineNumberMatch, out method);

            Filename = filenameMatch;
            LineNumber = lineNumberMatch;
            Method = method;

            return HasSourceCode;
        }

        public static bool FindSourceCode(string frame)
        {
            string filenameMatch;
            int lineNumberMatch;
            string method;

            return ExceptionViewerStackTraceWithHyperlinks.MatchStackLine(frame, out filenameMatch, out lineNumberMatch, out method);
        }
        public static string GetMethodName(string frame)
        {
            string filenameMatch;
            int lineNumberMatch;
            string method;

            ExceptionViewerStackTraceWithHyperlinks.MatchStackLine(frame, out filenameMatch, out lineNumberMatch, out method);

            return method;
        }


        public override string ToString()
        {
            if (!HasSourceCode)
                return CurrentFrame;
            
            return Path.GetFileNameWithoutExtension(Filename) + "." + Method;
        }
        
        public void AddSubframes(string[] lines, QueryPerformed query)
        {
            if(!lines[0].Equals(CurrentFrame))
                throw new Exception("Current frame did not match expected lines[0]");
            
            QueryCount += query.TimesSeen;

            //we are the last line
            if(lines.Length == 1)
                return;
            
            //we know about the child
            if (Children.ContainsKey(lines[1]))
                Children[lines[1]].AddSubframes(lines.Skip(1).ToArray(), query);//tell child to audit the relevant subframes
            else
                Children.Add(lines[1], new StackFramesTree(lines.Skip(1).ToArray(), query, IsInDatabaseAccessAssembly));
        }

    }
}
