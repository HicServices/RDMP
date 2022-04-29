// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using CsvHelper;


namespace ReusableLibraryCode
{
    /// <summary>
    /// Contains lots of generically useful static methods
    /// </summary>
    public class UsefulStuff
    {
        private static UsefulStuff _instance;
        public static Regex RegexThingsThatAreNotNumbersOrLetters = new Regex("[^0-9A-Za-z]+");
        public static Regex RegexThingsThatAreNotNumbersOrLettersOrUnderscores = new Regex("[^0-9A-Za-z_]+");
        
        public static UsefulStuff GetInstance()
        {
            if (_instance == null)
                _instance = new UsefulStuff();

            return _instance;
        }

        public static bool IsBadName(string name)
        {
            return name != null && name.Any(c=>Path.GetInvalidFileNameChars().Contains(c));
        }

        public static void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }


        internal DateTime LooseConvertToDate(object iO)
        {
            DateTime sDate;

            try
            {
                sDate = Convert.ToDateTime(iO);
            }
            catch (FormatException)
            {
                try
                {
                    string sDateAsString = iO.ToString();

                    if (sDateAsString.Length == 8)
                    {
                        sDateAsString = sDateAsString.Substring(0, 2) + "/" + sDateAsString.Substring(2, 2) + "/" +
                                        sDateAsString.Substring(4, 4);
                        sDate = Convert.ToDateTime(sDateAsString);
                    }
                    else if (sDateAsString.Length == 6)
                    {
                        sDateAsString = sDateAsString.Substring(0, 2) + "/" + sDateAsString.Substring(2, 2) + "/" +
                                        sDateAsString.Substring(4, 2);
                        sDate = Convert.ToDateTime(sDateAsString);
                    }
                    else
                        throw;
                }
                catch (Exception)
                {
                    throw new Exception("Cannot recognise date format :" + iO);

                }
            }
            return sDate;
        }


        public IEnumerable<string> GetArrayOfColumnNamesFromStringPastedInByUser(string text)
        {
            
            if (String.IsNullOrWhiteSpace(text))
                yield break;

            string[] split = text.Split(new char[] { '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries);


            //trim off [db]..[tbl] 1 
            for (int i = 0; i < split.Length; i++)
                split[i] = Regex.Replace(split[i], @"\s+[\d\s]*$", "");    
            
            //identifies the last word in a collection of multiple words (requires you .Trim() so we dont get ending whitespace match)
            Regex regexLastWord = new Regex("\\s[^\\s]*$");
            foreach (string s in split)
            {
                //clean the string

                string toAdd = s.Trim();
                if (toAdd.Contains("."))
                    toAdd = toAdd.Substring(toAdd.LastIndexOf(".") + 1);

                toAdd = toAdd.Replace("]", "");
                toAdd = toAdd.Replace("[", "");

                toAdd = toAdd.Replace("`", "");

                if (String.IsNullOrWhiteSpace(toAdd))
                    continue;

                if (regexLastWord.IsMatch(toAdd))
                    toAdd = regexLastWord.Match(toAdd).Value.Trim();

                yield return toAdd;
            }
        }

        public bool CHIisOK(string sCHI)
        {
            long n;
            DateTime d;
            bool ok = false;

            if (Int64.TryParse(sCHI, out n) && sCHI.Length == 10)
            {
                string sDate = sCHI.Substring(0, 2) + "/" + sCHI.Substring(2, 2) + "/" + sCHI.Substring(4, 2);
                string sCheck = sCHI.Substring(sCHI.Length - 1);
                if (DateTime.TryParse(sDate, out d) && GetCHICheckDigit(sCHI) == sCheck)
                    ok = true;
            }

            return ok;
        }

        public string GetCHICheckDigit(string sCHI)
        {
            int sum = 0, c = 0, lsCHI = 0;

            //sCHI = "120356785";
            lsCHI = sCHI.Length; // Must be 10!!

            sum = 0;
            c = (int)'0';
            for (int i = 0; i < lsCHI - 1; i++)
                sum += ((int)(sCHI.Substring(i, 1)[0]) - c) * (lsCHI - i);
            sum = sum % 11;

            c = 11 - sum;
            if (c == 11) c = 0;

            return ((char)(c + (int)'0')).ToString();

        }

        public static DirectoryInfo GetExecutableDirectory()
        {
            var exeLocation = Process.GetCurrentProcess()?.MainModule?.FileName;

            if(!string.IsNullOrWhiteSpace(exeLocation))
            {
                return new DirectoryInfo(Path.GetDirectoryName(exeLocation));
            }

            return new DirectoryInfo(Environment.CurrentDirectory);
        }

        public static string HashFile(string filename, int retryCount = 6)
        {
            try
            {
                using (var hashProvider = SHA512.Create())
                {
                    using (var stream = File.OpenRead(filename))
                    {
                        return BitConverter.ToString(hashProvider.ComputeHash(stream));
                    }
                }
            }
            catch (IOException)
            {
                //couldn't access the file so wait 1 second then try again
                Thread.Sleep(1000);

                if (retryCount-- > 0)
                    return HashFile(filename, retryCount);//try it again (recursively)
                else
                    throw;
            }


        }
     
        public static bool RequiresLength(string columnType)
        {
            columnType = columnType.ToLower();

            switch (columnType)
            {
                case "binary": return true;
                case "bit": return false;
                case "char": return true;
                case "image": return true;
                case "nchar": return true;
                case "nvarchar": return true;
                case "varbinary": return true;
                case "varchar": return true;
                case "numeric": return true;

                default: return false;
            }
        }

        public static bool HasPrecisionAndScale(string columnType)
        {
            columnType = columnType.ToLower();

            switch (columnType)
            {
                case "decimal": return true;
                case "numeric": return true;
                default: return false;
            }
        }
        
        public static string RemoveIllegalFilenameCharacters(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                foreach (char invalidFileNameChar in System.IO.Path.GetInvalidFileNameChars())
                    value = value.Replace(invalidFileNameChar.ToString(), "");

            return value;
        }

        
        public static void ExecuteBatchNonQuery(string sql, DbConnection conn, DbTransaction transaction = null, int timeout = 30)
        {
            Dictionary<int, Stopwatch> whoCares;
            ExecuteBatchNonQuery(sql, conn, transaction, out whoCares, timeout);
        }

        /// <summary>
        /// Executes the given SQL against the database + sends GO delimited statements as separate batches
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <param name="performanceFigures">Line number the batch started at and the time it took to complete it</param>
        /// <param name="timeout"></param>
        public static void ExecuteBatchNonQuery(string sql, DbConnection conn, DbTransaction transaction,out Dictionary<int, Stopwatch> performanceFigures, int timeout = 30) 
        {
            performanceFigures = new Dictionary<int, Stopwatch>();

            string sqlBatch = string.Empty;
            DbCommand cmd = DatabaseCommandHelper.GetCommand(string.Empty, conn, transaction);
            bool hadToOpen = false;

            if (conn.State != ConnectionState.Open)
            {

                conn.Open();
                hadToOpen = true;
            }

            int lineNumber = 1;

            sql += "\nGO";   // make sure last batch is executed.
            try
            {
                foreach (string line in sql.Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    lineNumber++;

                    if (line.ToUpperInvariant().Trim() == "GO")
                    {
                        if (string.IsNullOrWhiteSpace(sqlBatch))
                            continue;

                        if (!performanceFigures.ContainsKey(lineNumber))
                            performanceFigures.Add(lineNumber, new Stopwatch());
                        performanceFigures[lineNumber].Start();

                        cmd.CommandText = sqlBatch;
                        cmd.CommandTimeout = timeout;
                        cmd.ExecuteNonQuery();

                        performanceFigures[lineNumber].Stop();
                        sqlBatch = string.Empty;
                    }
                    else
                    {
                        sqlBatch += line + "\n";
                    }
                }
            }
            finally
            {
                if (hadToOpen)
                    conn.Close();
            }
        }


        /// <summary>
        /// Locates a manifest resource in the assembly under the manifest name subspace.  If you want to spray the resource MySoftwareSuite.MyApplication.MyResources.Bob.txt then pass:
        /// 1. the assembly containing the resource (e.g. typeof(MyClass1).Assembly)
        /// 2. the full path to the resource file: "MySoftwareSuite.MyApplication.MyResources.Bob.txt"
        /// 3. the filename "Bob.txt"
        /// </summary>
        /// <param name="assembly">The dll e.g. MySoftwareSuite.MyApplication.dll</param>
        /// <param name="manifestName">The full path to the manifest resource e.g. MySoftwareSuite.MyApplication.MyResources.Bob.txt</param>
        /// <param name="file">The filename ONLY of the resource e.g. Bob.txt</param>
        /// <param name="outputDirectory">The directory to put the generated file in.  Defaults to %appdata%/RDMP </param>
        public static FileInfo SprayFile(Assembly assembly, string manifestName, string file, string outputDirectory = null)
        {
            if(outputDirectory == null)
                outputDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RDMP");

            Stream fileToSpray = assembly.GetManifestResourceStream(manifestName);

            if (fileToSpray == null)
                throw new Exception("assembly.GetManifestResourceStream returned null for manifest name " + manifestName + " in assembly " + assembly);

            //get the bytes
            byte[] buffer = new byte[fileToSpray.Length];
            fileToSpray.Read(buffer, 0, buffer.Length);

            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            FileInfo target = new FileInfo(Path.Combine(outputDirectory,file));

            File.WriteAllBytes(target.FullName, buffer);

            fileToSpray.Close();
            fileToSpray.Dispose();
            
            return target;
        }

        public static string GetHumanReadableByteSize(long len)
        {
            string[] sizes = { "bytes", "KB", "MB", "GB" };
            
            int order = 0;
            while (len >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            return String.Format("{0:0.##} {1}", len, sizes[order]);
        }

        public static bool VerifyFileExists(string path, int timeout)
        {
            var task = new Task<bool>(() =>
            {
                try
                {
                    var fi = new FileInfo(path);
                    return fi.Exists;
                }
                catch (Exception)
                {
                    return false;
                }
            });
            task.Start();
            return task.Wait(timeout) && task.Result;
        }

        public static bool VerifyFileExists(Uri uri, int timeout)
        {
            return VerifyFileExists(uri.LocalPath,timeout);

        }

        public string DataTableToHtmlDataTable(DataTable dt)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<Table>");
            sb.Append("<TR>");

            foreach (DataColumn column in dt.Columns)
                sb.Append("<TD>" + column.ColumnName + "</TD>");

            sb.AppendLine("</TR>");

            foreach (DataRow row in dt.Rows)
            {
                sb.Append("<TR>");

                foreach (var cellObject in row.ItemArray)
                    sb.Append("<TD>" + cellObject.ToString() + "</TD>");
                
                sb.AppendLine("</TR>");
            }

            sb.Append("</Table>");

            return sb.ToString();
        }

        public string DataTableToCsv(DataTable dt)
        {
            StringBuilder sb = new StringBuilder();

            using (CsvWriter w = new CsvWriter(new StringWriter(sb),CultureInfo.CurrentCulture))
            {
                foreach (DataColumn column in dt.Columns)
                    w.WriteField(column.ColumnName);

                w.NextRecord();

                foreach (DataRow row in dt.Rows)
                {
                    foreach (var cellObject in row.ItemArray)
                        w.WriteField(cellObject);

                    w.NextRecord();
                }

                w.Flush();
            }
            
            return sb.ToString();
        }
        public void ShowFolderInWindowsExplorer(DirectoryInfo directoryInfo)
        {
            string argument = " \"" + directoryInfo.FullName + "\"";
            Process.Start("explorer.exe", argument);
        }

        public void ShowFileInWindowsExplorer(FileInfo fileInfo)
        {
            string argument = "/select, \"" + fileInfo.FullName + "\"";
            Process.Start("explorer.exe", argument);
        }

        public string GetClipboardFormatedHtmlStringFromHtmlString(string s)
        {
            const int maxLength = 9999999;
            if(s.Length > maxLength)
                throw new ArgumentException("String s is too long, the maximum length is " + maxLength + " but argument s was length " + s.Length,"s");

            var guidStart = Guid.NewGuid().ToString().Substring(0,7);
            var guidEnd = Guid.NewGuid().ToString().Substring(0, 7);

            //one in a million that the first 7 digits of the guid are the same as one another or exist in the data string
            if (s.Contains(guidStart) || s.Contains(guidEnd) || guidStart.Equals(guidEnd))
                return GetClipboardFormatedHtmlStringFromHtmlString(s);//but possible I guess so try again

            string template = "Version:1.0\r\nStartHTML:"+guidStart + "\r\nEndHTML:" +guidEnd +"\r\n";

            s = template
              .Replace(guidStart, template.Length.ToString("0000000"))
              .Replace(guidEnd, (template.Length + s.Length).ToString("0000000")) + s;

            return s;
        }

        public static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            Type baseType = givenType.BaseType;
            if (baseType == null) return false;

            return IsAssignableToGenericType(baseType, genericType);
        }

        public static string PascalCaseStringToHumanReadable(string pascalCaseString)
        {
            if (pascalCaseString == null) return null;

            //Deal with legacy property names by replacing underscore with a space
            pascalCaseString = pascalCaseString.Replace("_", " ");

            //There are two clauses in this Regex
            //Part1: [A-Z][A-Z]*(?=[A-Z][a-z]|\b) - looks for any series of uppercase letters that have a ending uppercase then lowercase OR end of line charater: https://regex101.com/r/mCqVk6/2
            //Part2: [A-Z](?=[a-z])               - looks for any single  of uppercase letters followed by a lower case letter: https://regex101.com/r/hdSCqH/1
            //This is then made into a single group that is matched and we add a space on front during the replacement.
            pascalCaseString = Regex.Replace(pascalCaseString, @"([A-Z][A-Z]*(?=[A-Z][a-z]|\b)|[A-Z](?=[a-z]))", " $1");

            //Remove any double mutliple white space
            //Because this matched the first capital letter in a string with Part2 of our regex above we should TRIM to remove the white space.
            pascalCaseString = Regex.Replace(pascalCaseString, @"\s\s+", " ").Trim();

            return pascalCaseString;
        }

        /// <summary>
        /// Returns the <paramref name="input"/> string split across multiple lines with the 
        /// <paramref name="newline"/> (or <see cref="Environment.NewLine"/> if null) separator
        /// such that no lines are longer than <paramref name="maxLen"/>
        /// </summary>
        /// <param name="input"></param>
        /// <param name="maxLen"></param>
        /// <param name="newline"></param>
        /// <returns></returns>
        public static string SplitByLength(string input, int maxLen, string newline = null)
        {

            return
                string.Join(newline ?? Environment.NewLine,
                Regex.Split(input, @"(.{1," + maxLen + @"})(?:\s|$)")
                        .Where(x => x.Length > 0)
                        .Select(x => x.Trim()));
        }
        

        public void ConfirmContentsOfDirectoryAreTheSame(DirectoryInfo first, DirectoryInfo other)
        {
            if (first.EnumerateFiles().Count() != other.EnumerateFiles().Count())
                throw new Exception("found different number of files in Globals directory " + first.FullName + " and " + other.FullName);

            var filesInFirst = first.EnumerateFiles().ToArray();
            var filesInOther = other.EnumerateFiles().ToArray();

            for (int i = 0; i < filesInFirst.Length; i++)
            {
                FileInfo file1 = filesInFirst[i];
                FileInfo file2 = filesInOther[i];
                if (!file1.Name.Equals(file2.Name))
                    throw new Exception("Although there were the same number of files in Globals directories " + first.FullName + " and " + other.FullName + ", there were differing file names (" + file1.Name + " and " + file2.Name + ")");

                if (!UsefulStuff.HashFile(file1.FullName).Equals(UsefulStuff.HashFile(file2.FullName)))
                    throw new Exception("File found in Globals directory which has a different MD5 from another Globals file.  Files were \"" + file1.FullName + "\" and \"" + file2.FullName + "\"");
            }
        }

        public static Parser GetParser()
        {
            var defaults = Parser.Default.Settings;

            var parser = new Parser(settings =>
            {
                settings.CaseSensitive = false;
                settings.CaseInsensitiveEnumValues = true;
                settings.EnableDashDash = defaults.EnableDashDash;
                settings.HelpWriter = defaults.HelpWriter;
                settings.IgnoreUnknownArguments = defaults.IgnoreUnknownArguments;
                settings.MaximumDisplayWidth = defaults.MaximumDisplayWidth;
                settings.ParsingCulture = defaults.ParsingCulture;
            });

            return parser;
        }

        /// <summary>
        /// Implementation of <see cref="Convert.ChangeType(object,Type)"/> that works with nullable types,
        /// dates etc
        /// </summary>
        /// <param name="value"></param>
        /// <param name="conversionType"></param>
        /// <returns></returns>
        public static object ChangeType(object value, Type conversionType)
        {
            Type t = Nullable.GetUnderlyingType(conversionType) ?? conversionType;

            if (t == typeof(DateTime) && value is string s)
            {
                if (string.Equals(s, "now",StringComparison.CurrentCultureIgnoreCase))
                    return DateTime.Now;

                //Convert.ChangeType doesn't handle dates, so let's deal with that
                return DateTime.Parse(s);
            }

            return value == null || value is string sval && string.IsNullOrWhiteSpace(sval) ? null : Convert.ChangeType(value, t);
        }
    }
}
