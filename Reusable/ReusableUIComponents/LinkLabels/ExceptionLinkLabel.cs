using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using ReusableUIComponents.Annotations;

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
