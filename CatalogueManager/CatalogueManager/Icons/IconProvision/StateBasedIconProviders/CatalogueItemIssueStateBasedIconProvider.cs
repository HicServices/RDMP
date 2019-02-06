// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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