using System;
using CatalogueLibrary.Properties;
using ReusableLibraryCode.Comments;

namespace CatalogueLibrary.Repositories.Managers
{
    /// <summary>
    /// Subclass of <see cref="CommentStore"/> which also loads <see cref="Resources.KeywordHelp"/>
    /// </summary>
    public class CommentStoreWithKeywords : CommentStore
    {
        public override void ReadComments(params string[] directoriesToLookInForComments)
        {
            base.ReadComments(directoriesToLookInForComments);

            AddToHelp(Resources.KeywordHelp);
        }

        private void AddToHelp(string keywordHelpFileContents)
        {
            var lines = keywordHelpFileContents.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var split = line.Split(':');

                if (split.Length != 2)
                    throw new Exception("Malformed line in Resources.KeywordHelp, line is:" + Environment.NewLine + line + Environment.NewLine + "We expected it to have exactly one colon in it");

                if (!ContainsKey(split[0]))
                    Add(split[0], split[1]);
            }
        }
    }
}