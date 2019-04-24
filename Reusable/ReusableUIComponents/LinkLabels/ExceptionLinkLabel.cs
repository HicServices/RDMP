// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Windows.Forms;
using ReusableLibraryCode.Annotations;
using ReusableUIComponents.Dialogs;

namespace ReusableUIComponents.LinkLabels
{
    public class ExceptionLinkLabel : Label
    {
        public ExceptionLinkLabel()
        {
            Font = new Font(Font, FontStyle.Underline);
            ForeColor = Color.Red;
        }

        private Exception _exception = null;
        private bool _displayWhenUserClicks;

        protected override void OnMouseHover(EventArgs e)
        {
            base.OnMouseHover(e);
            this.Cursor = Cursors.Hand;
        }

        public void SetException([CanBeNull] Exception ex,bool displayWhenUserClicks)
        {
            _exception = ex;
            _displayWhenUserClicks = displayWhenUserClicks;

            if (_exception == null)
                Visible = false;
            else
            {
                Visible = true;
                Text = _exception.Message;
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            if (_exception != null && _displayWhenUserClicks)
                ExceptionViewer.Show(_exception);
        }

        public void ClearException()
        {
            SetException(null, false);
        }
    }
}
