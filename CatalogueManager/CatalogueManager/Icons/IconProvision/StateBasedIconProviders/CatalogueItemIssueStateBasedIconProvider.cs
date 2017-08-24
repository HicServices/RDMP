using System;
using System.Drawing;
using CatalogueLibrary.Data;

namespace CatalogueManager.Icons.IconProvision.StateBasedIconProviders
{
    public class CatalogueItemIssueStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private Bitmap _redIssue;
        private Bitmap _orangeIssue;
        private Bitmap _greenIssue;

        public CatalogueItemIssueStateBasedIconProvider()
        {
            _redIssue = CatalogueIcons.RedIssue;
            _orangeIssue = CatalogueIcons.OrangeIssue;
            _greenIssue = CatalogueIcons.GreenIssue;
        }
        public Bitmap GetImageIfSupportedObject(object o)
        {
            var issue = o as CatalogueItemIssue;

            if (issue == null)
                return null;

            if (issue.Status == IssueStatus.Resolved)
                return _greenIssue;

            switch (issue.Severity)
            {
                case IssueSeverity.Red:
                    return _redIssue;
                case IssueSeverity.Amber:
                    return _orangeIssue;
                case IssueSeverity.Green:
                    return _greenIssue;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}