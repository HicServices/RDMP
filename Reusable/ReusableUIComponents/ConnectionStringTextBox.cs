using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FAnsi;

namespace ReusableUIComponents
{
    [System.ComponentModel.DesignerCategory("")]
    public class ConnectionStringTextBox : TextBox
    {
        private DatabaseType _databaseType;
        private List<string> supportedKeywords = new List<string>();
        private bool suppressAutocomplete = false;

        public DatabaseType DatabaseType
        {
            get { return _databaseType; }
            set
            {
                _databaseType = value;
                SetSupportedKeywords();
            }
        }

        private void SetSupportedKeywords()
        {
            supportedKeywords.Clear();
            switch (_databaseType)
            {
                case DatabaseType.MicrosoftSQLServer:

                    supportedKeywords.Add("User ID");
                    supportedKeywords.Add("TrustServerCertificate");
                    supportedKeywords.Add("Pooling");
                    supportedKeywords.Add("Password");
                    supportedKeywords.Add("Trusted_Connection");
                    supportedKeywords.Add("Integrated Security");
                    supportedKeywords.Add("Initial Catalog");
                    supportedKeywords.Add("Database");
                    supportedKeywords.Add("Encrypt");
                    supportedKeywords.Add("Data Source");
                    supportedKeywords.Add("Server");
                    supportedKeywords.Add("Address");
                    supportedKeywords.Add("Addr");
                    supportedKeywords.Add("Network Address");
                    supportedKeywords.Add("Application Name");
                    
                    
                    break;
                case DatabaseType.MySql:
                    throw new NotImplementedException();

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public ConnectionStringTextBox()
        {
            DatabaseType = DatabaseType.MicrosoftSQLServer;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            try
            {
                base.OnTextChanged(e);
            
                //set text color to black by default
                ForeColor = Color.Black;

                if (suppressAutocomplete)
                    return;

                //if user is typing somewhere that is not at the end of the text box
                if (SelectionStart + SelectionLength != TextLength)
                    return;

                //the potential autocomplete keywords
                List<string> candidates = new List<string>();
                candidates.AddRange(supportedKeywords.ToArray());

                //get all the keywords they have typed so far
                string[] keywords = Text.Split(';').ToArray();
                //set text color to black by default
                ForeColor = Color.Black;

                foreach (string keyword in keywords)
                {
                    var kvp = keyword.Split('=').ToArray();

                    //if it is something user has entered
                    if (kvp.Length == 2)
                        if (!supportedKeywords.Any(k => k.ToLower().Equals(kvp[0].ToLower().Trim())))
                            ForeColor = Color.DarkRed; //he entered something unsupported
                }

                //get last thing user is typing
                string lastBitBeingTyped = Text.Substring(Text.LastIndexOf(";") + 1);

                if (string.IsNullOrWhiteSpace(lastBitBeingTyped) //user has not typed anything or has just put in a ;
                    ||
                    lastBitBeingTyped.Contains("="))//user has typed Password=bobsca <- i.e. he is midway through typing a value not a key
                    return;

                //we will suggest Server because user typed se
                var keywordToSuggest = candidates.FirstOrDefault(c => c.ToLower().StartsWith(lastBitBeingTyped.ToLower()));

                if (keywordToSuggest == null)//nothing to suggest, user is typing crazy fool stuff
                {
                    ForeColor = Color.DarkRed;
                    return;
                }

                //dont suggest the whole thing, just suggest the bit they haven't typed yet
                string bitToSuggest = keywordToSuggest.Substring(lastBitBeingTyped.Length);
                if (string.IsNullOrWhiteSpace(bitToSuggest))
                    return;

                //suppress further text changed calls
                suppressAutocomplete = true;
                int whereUserIsCurrently = SelectionStart;

                Text = Text.Insert(whereUserIsCurrently, bitToSuggest);
                SelectionStart = whereUserIsCurrently;
                SelectionLength = TextLength - whereUserIsCurrently;
                suppressAutocomplete = false;//we are done
            } catch (Exception ex) 
            {
                ExceptionViewer.Show(ex);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if(e.Handled)//someone else suppressed it
                return;

            //if user is doing control c or control v or something
            if (e.Control)
                suppressAutocomplete = true;
            else
                if (char.IsLetterOrDigit((char)e.KeyCode) || e.KeyCode == Keys.Space)
                    suppressAutocomplete = false;
                else
                    suppressAutocomplete = true;//user is pressing some arrow keys or delete keys or something, don't suggest anything until 
        }

    }
}
